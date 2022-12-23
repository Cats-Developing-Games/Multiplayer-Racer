using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleMovementRigidbody {
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
    Rigidbody rb;
    public Vector3 force;
    public Vector3 circleMovementCenter;
    public Vector3 circularMovement;

    // Cheats
    /// <summary>Whether or not the vehicle experiences deceleration when no input is received</summary>
    public bool NoRollingFriction = false;
    public bool NoMaxSpeed = false;

    /// <summary>Relative to the vehicles rotation</summary>
    public Vector3 Velocity { get; set; }

    /// <summary>Negative is left, positive is right</summary>
    public float TurnRadius { get => turnRadius; }

    public VehicleMovementRigidbody(VehicleSO vehicleSO, VehicleInputHandler input, Transform transform, Rigidbody rb) {
        this.vehicleSO = vehicleSO;
        turnRadius = vehicleSO.MinTurnRadius;
        this.input = input;
        this.transform = transform;
        this.rb = rb;
    }

    public void Move(float deltaTime, float coefficientOfFriction) {
        maxDrivingSpeed = vehicleSO.MaxSpeed * MaxSpeedModifier;

        var grounded = Physics.Raycast(transform.position, -transform.up, 0.5f);
        if (!grounded) return;

        force = CalcNetForce(coefficientOfFriction);
        steeringValue = CalcSteeringValue();
        turnRadius = CalcTurnRadius();

        if (IsTurningRight()) {
            circularMovement = GetCircleMovement(Time.deltaTime, turnRadius, false);
            //float angleToMoveBy = movement.AngleToMoveBy(Time.deltaTime, rb.velocity, turnRadius);
            //angleToMoveBy = movement.IsTurningRight() ? angleToMoveBy : -angleToMoveBy;
            Vector3 rotateBy = CalcCirleMovementRotation(circleMovementCenter, circularMovement);
            transform.Rotate(rotateBy);
            //rb.rotation = Quaternion.RotateTowards(rb.rotation, Quaternion.AngleAxis(angleToMoveBy, transform.up), 90f);
            force = Vector3.RotateTowards(force, circularMovement - transform.position, Mathf.PI, 0f);
        }
        if (force.magnitude > 0f)
            Debug.Log(force);
        rb.AddRelativeForce(force * Time.deltaTime);
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
        circleMovementCenter = CircleMovementCenter(turnRadius);
        Vector3 newPositionOnCircle = Quaternion.AngleAxis(AngleToMoveBy(deltaTime), transform.up) * -transform.right * turnRadius.Value;
        Vector3 newPosition = newPositionOnCircle + circleMovementCenter;
        if (updateRotation) transform.Rotate(CalcCirleMovementRotation(circleMovementCenter, newPosition));
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
        return DirectionOfEngineForce() * vehicleSO.EngineForce * input.VerticalInput * coefficientOfFriction * vehicleSO.WheelTraction;
        
        Vector3 DirectionOfEngineForce() {
            return Quaternion.AngleAxis(steeringValue, transform.up) * transform.forward;
        }
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
        return steering * vehicleSO.SteeringAngle;
    }

    float CalcMaxSpeed() => NoMaxSpeed ? float.MaxValue : vehicleSO.MaxSpeed * MaxSpeedModifier;

    /// <returns>Returns a negative value if turning left, and a positive one if turning right</returns>
    // FIXME: this function sucks. I (kenny) don't think it feels good to turn the car using this calculation
    public float CalcTurnRadius() {
        if (steeringValue == 0f)
            return 0f;
        float turnRadius = vehicleSO.MinTurnRadius;
        turnRadius *= (Mathf.Sign(rb.velocity.z)) * (1f + rb.velocity.magnitude);
        turnRadius /= (Mathf.Sign(steeringValue) * (Math.Abs(steeringValue)));
        return turnRadius;
        //return Mathf.Clamp(steeringValue * velocity.magnitude, vehicleSO.MinTurnRadius, float.MaxValue);// * 5f;
    }
}
