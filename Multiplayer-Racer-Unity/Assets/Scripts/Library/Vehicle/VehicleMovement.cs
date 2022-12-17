using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleMovement {
    Vector3 acceleration = new Vector3();
    public Vector3 AccelerationModifier = new Vector3();
    Vector3 velocity = new Vector3();
    float maxDrivingSpeed = 0f;
    public float MaxSpeedModifier = 0f;
    float steeringValue = 0f;
    float turnRadius = 0f;
    VehicleSO vehicleSO;
    VehicleInputHandler input;
    Transform transform;

    public float TurnRadius { get; }

    public VehicleMovement(VehicleSO vehicleSO, VehicleInputHandler input, Transform transform) {
        this.vehicleSO = vehicleSO;
        this.input = input;
        this.transform = transform;
    }
    
    public Vector3 GetMove(float deltaTime, float coefficientOfFriction) {
        // kinetics
        maxDrivingSpeed = vehicleSO.MaxSpeed * MaxSpeedModifier;
        acceleration = Vector3.Scale(CalcAcceleration(coefficientOfFriction), AccelerationModifier);
        velocity = CalcVelocity(deltaTime);

        // steering
        steeringValue = CalcSteeringValue(input, vehicleSO.SteeringModifier);
        turnRadius = CalcTurnRadius(velocity, steeringValue, vehicleSO.MinTurnRadius, maxDrivingSpeed);

        if (!input.IsBreaking && input.VerticalInput == 0 && velocity.magnitude != 0f)
            applyRollingDeceleration();

        Vector3 newPosition;
        if (IsTurning(turnRadius))
            newPosition = GetCircleMovement(deltaTime);
        else
            newPosition = GetStraightMovement(deltaTime);
        return newPosition;

        void applyRollingDeceleration() {
            Vector3 rollingDecel = velocity.magnitude > 0f ? VehicleDefaults.RollingDeceleration : -VehicleDefaults.RollingDeceleration;
            velocity = CalcVelocity(velocity, rollingDecel, deltaTime, maxDrivingSpeed);
        }
    }

    Vector3 GetStraightMovement(float deltaTime) => GetStraightMovement(transform, velocity, acceleration, deltaTime);

    Vector3 GetStraightMovement(Transform transform, Vector3 velocity, Vector3 acceleration, float deltaTime) {
        //return transform.position + transform.forward * KineticPhysics.Displacement(velocity, acceleration, deltaTime);
        return transform.position + KineticPhysics.Displacement(velocity, acceleration, deltaTime);
    }

    public Vector3 CircleMovementCenter() => CircleMovementCenter(transform, turnRadius);

    Vector3 CircleMovementCenter(Transform transform, float turnRadius) {
        return transform.position + (transform.right * turnRadius);
    }

    Vector3 GetCircleMovement(float deltaTime) => GetCircleMovement(transform, velocity, turnRadius, deltaTime);

    Vector3 GetCircleMovement(Transform transform, Vector3 velocity, float turnRadius, float deltaTime) {
        Vector3 movementCircleCenter = CircleMovementCenter(transform, turnRadius);
        float angleToMoveBy = (velocity.magnitude / turnRadius) * deltaTime * Mathf.Rad2Deg;
        Vector3 newPositionOnCircle = Quaternion.AngleAxis(angleToMoveBy, Vector3.up) * -transform.right * turnRadius;
        Vector3 newPosition = newPositionOnCircle + movementCircleCenter;
        UpdateRotation();
        return newPosition;

        void UpdateRotation() {
            // I may be wrong here, but I don't think the car should update its x or z rotation in its own control method (unless its a plane-car)
            // If the cars yaw is changing, it should be from outside sources
            Vector3 normalizedPosition = transform.position - movementCircleCenter;
            Vector3 normalizedNewPosition = newPosition - movementCircleCenter;
            float yAxisRotation = Vector2.Angle(new Vector2(normalizedNewPosition.x, normalizedNewPosition.z), new Vector2(normalizedPosition.x, normalizedPosition.z));
            Vector3 rotateBy = new Vector3(transform.rotation.x, yAxisRotation, transform.rotation.z);
            rotateBy = rotateBy * (IsTurningRight(turnRadius) ? 1 : -1);
            transform.Rotate(rotateBy);
        }
    }

    public bool IsTurning() => turnRadius != 0;

    public bool IsTurning(float turnRadius) => turnRadius != 0;

    bool IsTurningRight() => turnRadius > 0;

    bool IsTurningRight(float turnRadius) => turnRadius > 0;

    Vector3 CalcFrictionForce(float coefficientOfFriction) {
        return CalcFrictionForce(input, coefficientOfFriction);
    }

    Vector3 CalcFrictionForce(VehicleInputHandler input, float coefficientOfFriction) {
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
        return CalcBreakForce(transform, vehicleSO.Mass, velocity, vehicleSO.WheelTraction, coefficientOfFriction);
    }

    Vector3 CalcBreakForce(Transform transform, float mass, Vector3 velocity, float wheelTraction, float coefficientOfFriction) {
        Vector3 force = KineticPhysics.ForceOfFriction(mass, transform, coefficientOfFriction * wheelTraction);
        force = velocity.magnitude > 0 ? force : -force;
        return force;
    }

    Vector3 CalcEngineForce(float coefficientOfFriction) => CalcEngineForce(input, vehicleSO.EngineForce, vehicleSO.WheelTraction, coefficientOfFriction);

    Vector3 CalcEngineForce(VehicleInputHandler input, float engineForce, float wheelTraction, float coefficientOfFriction) {
        if (input.IsBreaking) {
            return Vector3.zero;
        }
        return new Vector3(0f, 0f, engineForce) * input.VerticalInput * coefficientOfFriction * wheelTraction;
    }

    Vector3 CalcNetForce(float coefficientOfFriction) => CalcNetForce(input, vehicleSO.EngineForce, vehicleSO.WheelTraction, coefficientOfFriction);

    Vector3 CalcNetForce(VehicleInputHandler input, float engineForce, float wheelTraction, float coefficientOfFriction) {
        return CalcEngineForce(input, engineForce, wheelTraction, coefficientOfFriction) - CalcFrictionForce(input, coefficientOfFriction);
    }

    Vector3 CalcAcceleration(float coefficientOfFriction) => CalcAcceleration(input, vehicleSO.EngineForce, vehicleSO.WheelTraction, coefficientOfFriction, vehicleSO.Mass);

    Vector3 CalcAcceleration(VehicleInputHandler input, float engineForce, float wheelTraction, float coefficientOfFriction, float mass) {
        return KineticPhysics.Acceleration(CalcNetForce(input, engineForce, wheelTraction, coefficientOfFriction), mass);
    }

    Vector3 CalcVelocity(float deltaTime) => CalcVelocity(velocity, acceleration, deltaTime, maxDrivingSpeed);

    Vector3 CalcVelocity(Vector3 originalVelocity, Vector3 acceleration, float deltaTime, float? maxSpeed = null) {
        Vector3 velocity = KineticPhysics.Velocity(originalVelocity, acceleration, deltaTime);
        if (maxSpeed is not null) {
            return new Vector3(velocity.x, velocity.y, Mathf.Clamp(velocity.z, (float)-maxSpeed, (float)maxSpeed));
        }
        return velocity;
    }

    /// <returns>A value from -1 to 1</returns>
    float CalcSteeringValue(VehicleInputHandler input, float steeringModifier) {
        float steering = input.SteeringMethod.Invoke(input.HorizontalInput);
        return steering * steeringModifier;
    }

    float CalcTurnRadius(Vector3 velocity, float steering, float minTurnRadius, float maxSpeed) {
        if (velocity.magnitude == 0f || steering == 0)
            return 0f;
        else
            return (minTurnRadius / steering) * (velocity.magnitude / maxSpeed) * 5f;
    }
}
