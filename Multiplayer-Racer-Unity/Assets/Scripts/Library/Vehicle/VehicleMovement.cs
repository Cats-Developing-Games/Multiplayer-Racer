using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleMovement {
    /// <summary>Relative to the vehicles rotation</summary>
    Vector3 acceleration = new Vector3();
    /// <summary>Relative to the vehicles rotation</summary>
    public Vector3 AccelerationModifier = new Vector3(1f, 1f, 1f);
    /// <summary>Relative to the vehicles rotation</summary>
    Vector3 velocity = new Vector3();
    /// <summary>The maximum forward speed of the vehicle (velocity.z)</summary>
    float maxDrivingSpeed = 0f;
    public float MaxSpeedModifier = 1f;
    /// <summary>A value ranging from -1 to 1. Negative is left, positive is right</summary>
    public float steeringValue = 0f;
    /// <summary>Negative is left, positive is right</summary>
    float turnRadius = 0f;
    VehicleSO vehicleSO;
    VehicleInputHandler input;
    /// <summary>The vehicles transform</summary>
    Transform transform;

    // Cheats
    /// <summary>Whether or not the vehicle experiences deceleration when no input is received</summary>
    public bool NoRollingFriction = false;
    public bool NoMaxSpeed = false;

    /// <summary>Relative to the vehicles rotation</summary>
    public Vector3 Velocity { get; set; }

    /// <summary>Negative is left, positive is right</summary>
    public float TurnRadius { get => turnRadius; }

    public VehicleMovement(VehicleSO vehicleSO, VehicleInputHandler input, Transform transform) {
        this.vehicleSO = vehicleSO;
        turnRadius = vehicleSO.MinTurnRadius;
        this.input = input;
        this.transform = transform;
    }

    public Vector3 GetMove(float deltaTime, float coefficientOfFriction) {
        // kinetics
        maxDrivingSpeed = vehicleSO.MaxSpeed * MaxSpeedModifier;
        acceleration = Vector3.Scale(CalcAcceleration(coefficientOfFriction), AccelerationModifier);
        velocity = CalcVelocity(deltaTime);

        // steering
        steeringValue = CalcSteeringValue();
        turnRadius = CalcTurnRadius();

        Vector3 newPosition;
        if (IsTurning())
            newPosition = GetCircleMovement(deltaTime);
        else
            newPosition = GetStraightMovement(deltaTime);
        return newPosition;
    }

    Vector3 GetStraightMovement(float deltaTime) {
        return transform.position + RotateToZAxis(KineticPhysics.Displacement(velocity, acceleration, deltaTime));
    }

    Vector3 RotateToZAxis(Vector3 vector) {
        float sign = Vector3.Angle(vector, Vector3.forward) > 90f ? -1f : 1f;
        return Vector3.RotateTowards(vector, sign * transform.forward, 2 * Mathf.PI, 0f);
    }

    public Vector3 CircleMovementCenter(float? turnRadius = null) {
        turnRadius ??= this.turnRadius;
        return transform.position + (transform.right * turnRadius.Value);
    }

    public float AngleToMoveBy(float deltaTime, Vector3? velocity = null, float? turnRadius = null) {
        velocity ??= this.velocity;
        turnRadius ??= this.turnRadius;
        return (velocity.Value.z / turnRadius.Value) * deltaTime* Mathf.Rad2Deg;
    }

    public Vector3 GetCircleMovement(float deltaTime, float? turnRadius = null, bool updateRotation = true) {
        turnRadius ??= this.turnRadius;
        Vector3 movementCircleCenter = CircleMovementCenter(turnRadius);
        Vector3 newPositionOnCircle = Quaternion.AngleAxis(AngleToMoveBy(deltaTime), transform.up) * -transform.right * turnRadius.Value;
        Vector3 newPosition = newPositionOnCircle + movementCircleCenter;
        if (updateRotation) transform.Rotate(CalcCirleMovementRotation(movementCircleCenter, newPosition));
        return newPosition;
    }

    public Vector3 CalcCirleMovementRotation(Vector3 circleCenter, Vector3 newPositionOnCircle) {
        Vector3 normalizedPosition = transform.position - circleCenter;
        Vector3 normalizedNewPosition = newPositionOnCircle - circleCenter;
        float yAxisRotation = Vector2.Angle(new Vector2(normalizedNewPosition.x, normalizedNewPosition.z), new Vector2(normalizedPosition.x, normalizedPosition.z));
        Vector3 rotateBy = new Vector3(transform.rotation.x, yAxisRotation, transform.rotation.z);
        return rotateBy * (IsTurningRight() ? 1f : -1f);
    }

    public bool IsTurning() => steeringValue != 0;

    public bool IsTurningRight() => steeringValue > 0f;

    bool IsMovingForwards() => velocity.z > 0f;

    bool IsMovingBackwards() => velocity.z < 0f;

    Vector3 CalcFrictionForce(float coefficientOfFriction) {
        if (input.IsBreaking) {
            return CalcBreakForce(coefficientOfFriction);
        }
        if (input.VerticalInput == 0) {
            // TODO: rolling friction (natural deceleration) is applied as a constant decel in handleMotor()
            // I'm really not sure how to properly calculate rolling friction
            return Vector3.zero;
        }
        return Vector3.zero;
    }

    Vector3 CalcBreakForce(float coefficientOfFriction) {
        Vector3 force = KineticPhysics.ForceOfFriction(velocity, vehicleSO.Mass, transform, coefficientOfFriction * vehicleSO.WheelTraction);
        //force = IsMovingForwards() ? force : -force;
        Debug.Log(force);
        return force;
    }

    Vector3 CalcEngineForce(float coefficientOfFriction) {
        if (input.IsBreaking) {
            return Vector3.zero;
        }
        return vehicleSO.EngineForce * input.VerticalInput * coefficientOfFriction * vehicleSO.WheelTraction;
    }

    public Vector3 CalcNetForce(float coefficientOfFriction) {
        return CalcEngineForce(coefficientOfFriction) + CalcFrictionForce(coefficientOfFriction);
    }

    Vector3 CalcAcceleration(float coefficientOfFriction) {
        if (!NoRollingFriction && (!input.IsBreaking && input.VerticalInput == 0f)) {
            if (IsMovingForwards())
                return VehicleDefaults.RollingDeceleration;
            else if (IsMovingBackwards())
                return -VehicleDefaults.RollingDeceleration;
        } else {
            Vector3 netForce = CalcNetForce(coefficientOfFriction);
            return KineticPhysics.Acceleration(netForce, vehicleSO.Mass);
        }
        return Vector3.zero;
    }

    public Vector3 CalcVelocity(float deltaTime) {
        velocity = KineticPhysics.Velocity(velocity, acceleration, deltaTime);
        velocity = velocity.RoundIfBasicallyZero();
        float maxSpeed = CalcMaxSpeed();
        return new Vector3(velocity.x, velocity.y, Mathf.Clamp(velocity.z, (float)-maxSpeed, (float)maxSpeed));
    }

    /// <returns>A value from -1 to 1</returns>
    public float CalcSteeringValue() {
        float steering = input.SteeringMethod.Invoke(input.HorizontalInput);
        return steering * vehicleSO.SteeringModifier;
    }

    float CalcMaxSpeed() => NoMaxSpeed ? float.MaxValue : vehicleSO.MaxSpeed * MaxSpeedModifier;

    /// <returns>Returns a negative value if turning left, and a positive one if turning right</returns>
    // FIXME: this function sucks. I (kenny) don't think it feels good to turn the car using this calculation
    public float CalcTurnRadius(float? steeringValue = null, Vector3? velocity = null) {
        steeringValue ??= this.steeringValue;
        velocity ??= this.velocity;
        if (steeringValue == 0f)
            return 0f;
        float turnRadius = vehicleSO.MinTurnRadius;
        turnRadius *= (Mathf.Sign(velocity.Value.z)) * (1f + velocity.Value.magnitude);
        turnRadius /= (Mathf.Sign(steeringValue.Value) * (Math.Abs(steeringValue.Value)));
        return turnRadius;
        //return Mathf.Clamp(steeringValue * velocity.magnitude, vehicleSO.MinTurnRadius, float.MaxValue);// * 5f;
    }
}
