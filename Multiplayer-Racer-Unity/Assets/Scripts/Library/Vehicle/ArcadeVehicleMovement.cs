using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadeVehicleMovement {

    #region Movement Variables

    public static float RollingFrictionModifier = 0.01f;

    /// <summary>Relative to the vehicles rotation</summary>
    Vector3 acceleration = new Vector3();
    /// <summary>Relative to the vehicles rotation</summary>
    public float AccelerationModifier = 1f;
    /// <summary>Relative to the vehicles rotation</summary>
    public float VelocityModifier = 1f;
    /// <summary>Relative to the vehicles rotation</summary>
    Vector3 velocity = new Vector3();
    public float MaxSpeedModifier = 1f;
    /// <summary>A value ranging from -1 to 1. Negative is left, positive is right</summary>
    public float steeringValue = 0f;
    /// <summary>Negative is left, positive is right</summary>
    float turnRadius = 0f;
    public Vector3 CircleMovementCenter;

    /// <summary>Acceleration is applied to the vehicles local direction. When applying worldspace vectors, rotate them accordingly (see transform.TransformDirection)</summary>
    Dictionary<string, VectorCalc> Accelerations;

    #endregion

    #region Prefab sourced variables

    VehicleSO vehicleSO;
    VehicleInputHandler input;
    VehicleCollisionBehaviour collision;
    /// <summary>The vehicles transform</summary>
    Transform transform;

    #endregion

    // Cheats
    /// <summary>Whether or not the vehicle experiences deceleration when no input is received</summary>
    public bool NoRollingFriction = false;
    public bool NoMaxSpeed = false;

    /// <summary>Relative to the vehicles rotation</summary>
    public Vector3 Velocity { get; set; }

    /// <summary>Negative is left, positive is right</summary>
    public float TurnRadius { get => turnRadius; }

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
        Accelerations = new Dictionary<string, VectorCalc>();
        AddAcceleration(ENGINE_ACCEL_KEY, CalcAccelerationFromVehicle);
    }

    public Vector3 GetMove(float deltaTime, float coefficientOfFriction) {
        acceleration = ResolveAccelerations(coefficientOfFriction);
        velocity = CalcVelocity(deltaTime, null, acceleration * AccelerationModifier);
        steeringValue = CalcSteeringValue();

        Vector3 newPosition;
        turnRadius = CalcTurnRadius();

        if (IsTurning())
            newPosition = GetCircleMovement(deltaTime, velocity * VelocityModifier);
        else
            newPosition = GetStraightMovement(deltaTime, velocity * VelocityModifier, acceleration * AccelerationModifier);

        return newPosition;
    }

    public void AddAcceleration(string key, Func<float, Vector3> accelerationCalc) {
        if (Accelerations.ContainsKey(key)) return;
        Accelerations.Add(key, new VectorCalc(key, accelerationCalc));
    }

    public void RemoveAcceleration(string key) {
        if (!Accelerations.ContainsKey(key)) return;
        Accelerations.Remove(key);
    }

    Vector3 GetStraightMovement(float deltaTime, Vector3? velocity = null, Vector3? acceleration = null) {
        velocity ??= this.velocity;
        acceleration ??= this.acceleration;
        Vector3 displacement = RotateToZAxis(KineticPhysics.Displacement(velocity.Value, acceleration.Value, deltaTime));
        return transform.position + displacement;
    }

    Vector3 RotateToZAxis(Vector3 vector) {
        float sign = Vector3.Angle(vector, Vector3.forward) > 90f ? -1f : 1f;
        return Vector3.RotateTowards(vector, sign * transform.forward, 2 * Mathf.PI, 0f);
    }

    Vector3 GetCircleMovement(float deltaTime, Vector3? velocity = null, float? turnRadius = null, bool updateRotation = true) {
        velocity ??= this.velocity;
        turnRadius ??= this.turnRadius;
        CircleMovementCenter = CalcCircleMovementCenter(turnRadius);
        float angleToMoveBy = AngleToMoveBy(deltaTime, velocity, turnRadius);
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

    Vector3 CalcCircleMovementCenter(float? turnRadius = null) {
        turnRadius ??= this.turnRadius;
        return transform.position + (transform.right * turnRadius.Value);
    }

    float AngleToMoveBy(float deltaTime, Vector3? velocity = null, float? turnRadius = null) {
        velocity ??= this.velocity;
        turnRadius ??= this.turnRadius;
        return (velocity.Value.z / turnRadius.Value) * deltaTime * Mathf.Rad2Deg;
    }

    Vector3 CalcCirleMovementRotation(Vector3 circleCenter, Vector3 newPositionOnCircle) {
        Vector3 normalizedPosition = transform.position - circleCenter;
        Vector3 normalizedNewPosition = newPositionOnCircle - circleCenter;
        float yAxisRotation = Vector2.Angle(new Vector2(normalizedNewPosition.x, normalizedNewPosition.z), new Vector2(normalizedPosition.x, normalizedPosition.z));
        Vector3 rotateBy = new Vector3(transform.rotation.x, yAxisRotation, transform.rotation.z);
        return rotateBy * (IsTurningRight() ? 1f : -1f);
    }

    public bool IsTurning() => steeringValue != 0f;

    public bool IsTurningRight() => steeringValue > 0f;

    public bool IsMovingForwards() => velocity.z > 0f;

    public bool IsMovingBackwards() => velocity.z < 0f;

    Vector3 CalcFrictionForce(float coefficientOfFriction, Vector3? velocity = null) {
        velocity ??= this.velocity;
        
        // TODO: rolling friction (natural deceleration) is applied as a constant decel in CalcAcceleration()
        // I'm really not sure how to properly calculate rolling friction
        //if (input.VerticalInput == 0) { // Rolling friction
        //    return Vector3.zero;
        //}
        //Vector3 frictionForce = KineticPhysics.NormalForce(vehicleSO.Mass, transform);
        //frictionForce = frictionForce * input.VerticalInput * coefficientOfFriction * RollingFrictionModifier;
        //frictionForce = Vector3.RotateTowards(frictionForce, Vector3.back, (float)Math.PI, 0f);
        //return frictionForce;
        return Vector3.zero;
    }

    Vector3 CalcBreakForce(float coefficientOfFriction, Vector3? velocity = null) {
        velocity ??= this.velocity;
        Vector3 force = KineticPhysics.ForceOfFriction(velocity.Value, vehicleSO.Mass, transform, coefficientOfFriction * vehicleSO.WheelTraction);
        return force;
    }

    Vector3 CalcEngineForce(float coefficientOfFriction) {
        if (input.IsBreaking) {
            return CalcBreakForce(coefficientOfFriction, velocity);
        }
        Vector3 engineForce = new Vector3(0f, 0f, vehicleSO.EngineForce) * input.VerticalInput;
        return engineForce;
    }

    Vector3 CalcNetForce(float coefficientOfFriction) {
        Vector3 frictionForce = CalcFrictionForce(coefficientOfFriction);
        Vector3 engineForce = CalcEngineForce(coefficientOfFriction);
        float sign = engineForce.z > 0f ? 1f : -1f;
        Vector3 netForce = engineForce + (sign * frictionForce);
        return netForce;
    }

    Vector3 ResolveAccelerations(float coefficientOfFriction) {
        Vector3 resolvedAcceleration = new Vector3(0f, 0f, 0f);

        foreach (var key in Accelerations.Keys) {
            resolvedAcceleration += Accelerations[key].calc(coefficientOfFriction);
        }

        return resolvedAcceleration;
    }

    Vector3 CalcAccelerationFromVehicle(float coefficientOfFriction) {
        if (!collision.IsGrounded) return Vector3.zero;
        if (!NoRollingFriction && (!input.IsBreaking && input.VerticalInput == 0f)) {
            if (IsMovingForwards()) return VehicleDefaults.RollingDeceleration;
            else if (IsMovingBackwards()) return -VehicleDefaults.RollingDeceleration;
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

    Vector3 CalcVelocity(float deltaTime, Vector3? velocity = null, Vector3? acceleration = null) {
        velocity ??= this.velocity;
        acceleration ??= this.acceleration;
        Vector3 newVelocity = KineticPhysics.Velocity(velocity.Value, acceleration.Value, deltaTime);
        newVelocity = newVelocity.RoundIfBasicallyZero();
        float maxSpeed = CalcMaxSpeed();
        newVelocity = new Vector3(newVelocity.x, newVelocity.y, Mathf.Clamp(newVelocity.z, (float)-maxSpeed, (float)maxSpeed));
        return newVelocity;
    }

    float CalcSteeringValue() {
        if (!collision.IsGrounded) return 0f;
        float steeringValue = input.SteeringMethod.Invoke(input.HorizontalInput);
        return Mathf.Clamp(steeringValue * vehicleSO.SteeringModifier, -1f, 1f);
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

    class VectorCalc {
        public string name;
        /// <summary>If pure, is not affected by any modifiers</summary>
        public Func<float, Vector3> calc;
        /// <summary>If pure, is not affected by any modifiers</summary>
        public float modifier = 1f;

        public VectorCalc(string name, Func<float, Vector3> calc) {
            this.name = name;
            this.calc = calc;
        }
    }
}
