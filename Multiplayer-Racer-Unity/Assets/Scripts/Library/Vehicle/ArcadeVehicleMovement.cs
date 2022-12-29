using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadeVehicleMovement {

    #region Movement Variables

    /// <summary>Relative to the vehicles rotation</summary>
    Vector3 acceleration = new Vector3();
    /// <summary>Relative to the vehicles rotation</summary>
    public Vector3 AccelerationModifier = new Vector3(1f, 1f, 1f);
    /// <summary>Relative to the vehicles rotation</summary>
    Vector3 velocity = new Vector3();
    public float MaxSpeedModifier = 1f;
    /// <summary>A value ranging from -1 to 1. Negative is left, positive is right</summary>
    public float steeringValue = 0f;
    public float steeringAngle = 0f;
    /// <summary>Negative is left, positive is right</summary>
    float turnRadius = 0f;
    public Vector3 CircleMovementCenter;

    /// <summary>Acceleration is applied to the vehicles local direction. When applying worldspace vectors, rotate them accordingly (see transform.TransformDirection)</summary>
    public Dictionary<string, Func<float, Vector3>> Accelerations;
    
    #endregion

    #region Prefab sourced variables

    VehicleSO vehicleSO;
    VehicleInputHandler input;
    VehicleCollisionBehaviour collision;
    /// <summary>The vehicles transform</summary>
    Transform transform;

    #endregion

    bool useGravity;
    public bool OnlyVelocityBasedMovement = true;

    // Cheats
    /// <summary>Whether or not the vehicle experiences deceleration when no input is received</summary>
    public bool NoRollingFriction = false;
    public bool NoMaxSpeed = false;

    /// <summary>Relative to the vehicles rotation</summary>
    public Vector3 Velocity { get; set; }

    /// <summary>Negative is left, positive is right</summary>
    public float TurnRadius { get => turnRadius; }
    public bool GravityEnabled { 
        get => useGravity; 
        set {
            useGravity = value;
            if (useGravity) {
                AddAcceleration(GRAVITY_ACCEL_KEY, CalcGravitationalAcceleration);
                AddAcceleration(NORMAL_ACCEL_KEY, CalcNormalAcceleration);
            } else {
                RemoveAcceleration(GRAVITY_ACCEL_KEY);
                RemoveAcceleration(NORMAL_ACCEL_KEY);
            }
        }
    }

    public const string GRAVITY_ACCEL_KEY = "gravity";
    public const string NORMAL_ACCEL_KEY = "normal";
    public const string ENGINE_ACCEL_KEY = "engine";

    /// <summary>Handles all calculations, physics, and placements of moving an Arcade style vehicle</summary>
    /// <param name="useGravity">If using a rigidbody to calculate gravity, then useGravity should probably be false</param>
    public ArcadeVehicleMovement(VehicleSO vehicleSO, VehicleInputHandler input, Transform transform, VehicleCollisionBehaviour collision) {
        this.vehicleSO = vehicleSO;
        turnRadius = vehicleSO.MinTurnRadius;
        this.input = input;
        this.transform = transform;
        this.collision = collision;
        Accelerations = new Dictionary<string, Func<float, Vector3>>();
        Accelerations.Add(ENGINE_ACCEL_KEY, CalcAccelerationFromVehicle);
    }

    public Vector3 GetMove(float deltaTime, float coefficientOfFriction) {
        acceleration = Vector3.Scale(ResolveAccelerations(coefficientOfFriction), AccelerationModifier);
        velocity = CalcVelocity(deltaTime);
        steeringAngle = CalcSteeringAngle();

        Vector3 newPosition;
        if (OnlyVelocityBasedMovement) {
            newPosition = GetPureVelocityBasedMovement(deltaTime);
        } else {
            turnRadius = CalcTurnRadius();

            if (IsTurning())
                newPosition = GetCircleMovement(deltaTime);
            else
                newPosition = GetStraightMovement(deltaTime);
        }

        return newPosition;
    }

    public void AddAcceleration(string key, Func<float, Vector3> acceleration) {
        if (Accelerations.ContainsKey(key)) return;
        Accelerations.Add(key, acceleration);
    }

    public void RemoveAcceleration(string key) {
        if (!Accelerations.ContainsKey(key)) return;
        Accelerations.Remove(key);
    }

    Vector3 GetPureVelocityBasedMovement(float deltaTime) {
        Vector3 displacement = KineticPhysics.Displacement(velocity, acceleration, deltaTime);
        Vector3 newPosition = transform.position + displacement;
        //Vector3 lookAt = IsMovingForwards() ? transform.position + displacement : transform.position - displacement;
        //transform.LookAt(lookAt);
        //Vector3.
        //float yaw = Vector3.Angle(transform.forward.y, velocity.);
        //transform.Rotate();
        return newPosition;
    }

    Vector3 GetStraightMovement(float deltaTime) {
        Vector3 displacement = RotateToZAxis(KineticPhysics.Displacement(velocity, acceleration, deltaTime));
        return transform.position + displacement;
    }

    Vector3 RotateToZAxis(Vector3 vector) {
        float sign = Vector3.Angle(vector, Vector3.forward) > 90f ? -1f : 1f;
        return Vector3.RotateTowards(vector, sign * transform.forward, 2 * Mathf.PI, 0f);
    }

    Vector3 CalcCircleMovementCenter(float? turnRadius = null) {
        turnRadius ??= this.turnRadius;
        return transform.position + (transform.right * turnRadius.Value);
    }

    float AngleToMoveBy(float deltaTime, Vector3? velocity = null, float? turnRadius = null) {
        velocity ??= this.velocity;
        turnRadius ??= this.turnRadius;
        return (velocity.Value.z / turnRadius.Value) * deltaTime * Mathf.Rad2Deg;
    }

    Vector3 GetCircleMovement(float deltaTime, float? turnRadius = null, bool updateRotation = true) {
        turnRadius ??= this.turnRadius;
        CircleMovementCenter = CalcCircleMovementCenter(turnRadius);
        float angleToMoveBy = AngleToMoveBy(deltaTime);
        // TODO: moving backwards inverts the steering. this is not correct
        // For example: steering left and moving backwards moves the car backwards to the RIGHT (should be left)
        // please hwelp me Mike
        //angleToMoveBy -= IsMovingBackwards() ? 180f : 0f;
        //float sign = IsMovingBackwards() ? -1f : 1f;
        Quaternion axisRotation = Quaternion.AngleAxis(angleToMoveBy, transform.up);
        Vector3 newPositionOnCircle = axisRotation * -transform.right * turnRadius.Value;
        Vector3 newPosition = CircleMovementCenter + newPositionOnCircle;
        if (updateRotation) transform.Rotate(CalcCirleMovementRotation(CircleMovementCenter, newPosition));
        return newPosition;
    }

    Vector3 CalcCirleMovementRotation(Vector3 circleCenter, Vector3 newPositionOnCircle) {
        Vector3 normalizedPosition = transform.position - circleCenter;
        Vector3 normalizedNewPosition = newPositionOnCircle - circleCenter;
        float yAxisRotation = Vector2.Angle(new Vector2(normalizedNewPosition.x, normalizedNewPosition.z), new Vector2(normalizedPosition.x, normalizedPosition.z));
        Vector3 rotateBy = new Vector3(transform.rotation.x, yAxisRotation, transform.rotation.z);
        return rotateBy * (IsTurningRight() ? 1f : -1f);
    }

    public bool IsTurning() => steeringAngle!= 0f;

    public bool IsTurningRight() => steeringAngle > 0f;

    public bool IsMovingForwards() => Vector3.Dot(transform.forward, velocity) > 0f;

    public bool IsMovingBackwards() => Vector3.Dot(transform.forward, velocity) < 0f;

    Vector3 CalcFrictionForce(float coefficientOfFriction) {
        if (input.IsBreaking) {
            return CalcBreakForce(coefficientOfFriction);
        }
        // TODO: rolling friction (natural deceleration) is applied as a constant decel in CalcAcceleration()
        // I'm really not sure how to properly calculate rolling friction
        //if (input.VerticalInput == 0) { // Rolling friction
        //    return Vector3.zero;
        //}
        return Vector3.zero;
    }

    Vector3 CalcBreakForce(float coefficientOfFriction) {
        Vector3 force = KineticPhysics.ForceOfFriction(velocity, vehicleSO.Mass, transform, coefficientOfFriction * vehicleSO.WheelTraction);
        return force;
    }

    Vector3 CalcEngineForce(float coefficientOfFriction) {
        if (input.IsBreaking) {
            return Vector3.zero;
        }
        if (OnlyVelocityBasedMovement)
            return DirectionOfEngineForce() * vehicleSO.EngineForce * input.VerticalInput * coefficientOfFriction * vehicleSO.WheelTraction;
        else
            return new Vector3(0f, 0f, vehicleSO.EngineForce) * input.VerticalInput * coefficientOfFriction * vehicleSO.WheelTraction;

        Vector3 DirectionOfEngineForce() {
            return Quaternion.AngleAxis(steeringAngle, transform.up) * transform.forward;
        }
    }

    Vector3 CalcNetForce(float coefficientOfFriction) {
        return CalcEngineForce(coefficientOfFriction) + CalcFrictionForce(coefficientOfFriction);
    }

    Vector3 ResolveAccelerations(float coefficientOfFriction) {
        Vector3 resolvedAcceleration = new Vector3(0f, 0f, 0f);

        foreach (var key in Accelerations.Keys) {
            resolvedAcceleration += Accelerations[key](coefficientOfFriction);
        }

        return resolvedAcceleration;
    }

    Vector3 CalcAccelerationFromVehicle(float coefficientOfFriction) {
        if (!collision.IsGrounded) return Vector3.zero;
        if (!NoRollingFriction && (!input.IsBreaking && input.VerticalInput == 0f)) {
            if (IsMovingForwards())
                return VehicleDefaults.RollingDeceleration;
            else if (IsMovingBackwards())
                return -VehicleDefaults.RollingDeceleration;
        } else {
            Vector3 netForce = CalcNetForce(coefficientOfFriction);
            Vector3 acceleration = KineticPhysics.Acceleration(netForce, vehicleSO.Mass);
            return acceleration;
        }
        return Vector3.zero;
    }

    Vector3 CalcGravitationalAcceleration(float coefficientOfFriction) => transform.TransformDirection(KineticPhysics.GravitationalAcceleration);

    Vector3 CalcNormalAcceleration(float coefficientOfFriction) {
        if (collision.IsGrounded) return transform.TransformDirection(-KineticPhysics.GravitationalAcceleration);
        else return Vector3.zero;
    }

    Vector3 CalcVelocity(float deltaTime) {
        Vector3 velocity = KineticPhysics.Velocity(this.velocity, acceleration, deltaTime);
        velocity = velocity.RoundIfBasicallyZero();
        float maxSpeed = CalcMaxSpeed();
        velocity = new Vector3(velocity.x, velocity.y, Mathf.Clamp(velocity.z, (float)-maxSpeed, (float)maxSpeed));
        return velocity;
    }

    /// <returns>A value from -1 to 1</returns>
    float CalcSteeringAngle() {
        if (!collision.IsGrounded) return 0f;
        steeringValue = input.SteeringMethod.Invoke(input.HorizontalInput);
        return steeringValue * Math.Abs(vehicleSO.SteeringAngle);
    }

    float CalcMaxSpeed() => NoMaxSpeed ? float.MaxValue : vehicleSO.MaxSpeed * MaxSpeedModifier;

    /// <returns>Returns a negative value if turning left, and a positive one if turning right</returns>
    // FIXME: this function sucks. I (kenny) don't think it feels good to turn the car using this calculation
    // OR 
    // maybe change how steeringAngle is calculated in VehicleInput
    float CalcTurnRadius(float? steeringValue = null, Vector3? velocity = null) {
        steeringValue ??= this.steeringValue;
        velocity ??= this.velocity;
        if (steeringValue == 0f)
            return 0f;
        float turnRadius = vehicleSO.MinTurnRadius;
        turnRadius *= (1f + velocity.Value.magnitude);
        turnRadius /= steeringValue.Value;
        return turnRadius;
    }
}
