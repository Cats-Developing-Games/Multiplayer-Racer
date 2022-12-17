using System;
using UnityEngine;

public static class KineticPhysics {
    public static readonly Vector3 GravitationalAcceleration = new Vector3(0f, -9.80665f, 0f); // https://en.wikipedia.org/wiki/Gravity_of_Earth

    #region Displacement

    public static float Displacement(float velocity, float acceleration, float deltaTime) {
        return (velocity * deltaTime) + (0.5f) * (acceleration * ((float)Mathf.Pow(deltaTime, 2)));
    }

    public static Vector3 Displacement(Vector3 velocity, Vector3 acceleration, float deltaTime) {
        return (velocity * deltaTime) + (0.5f) * (acceleration * ((float)Mathf.Pow(deltaTime, 2)));
    }

    #endregion

    #region Torque

    /// <summary>
    /// Calculate torque. T = F * r
    /// <br/><br/>
    /// <see href="https://en.wikipedia.org/wiki/Torque"/>
    /// <br/><br/>
    /// Note: If lengthOfMomentArm is = 1, then this technically returns newton-meters
    /// </summary>
    public static float Torque(float force, float RadiusOfArm = 1f) {
        return force * RadiusOfArm;
    }

    #endregion

    #region Velocity

    public static float Velocity(float originalVelocity, float acceleration, float deltaTime) {
        return originalVelocity + (acceleration * deltaTime);
    }

    public static Vector3 Velocity(Vector3 originalVelocity, Vector3 acceleration, float deltaTime) {
        return originalVelocity + (acceleration * deltaTime);
    }

    #endregion

    #region Acceleration

    /// <summary>Calculate acceleration using Newtons formula A=f/m</summary>
    public static Vector3 Acceleration(Vector3 force, float mass) => force / mass;

    #endregion

    #region Force

    /// <summary>Calculate the force of gravity</summary>
    public static Vector3 ForceOfGravity(float mass, float angleOfIncline = 0f) {
        return NormalForce(mass, GravitationalAcceleration, angleOfIncline);
    }

    /// <summary>Calculate the force of gravity, assuming the object rests on an incline</summary>
    public static Vector3 NormalForce(float mass, Vector3? normalAcceleration = null, float angleOfIncline = 0f) {
        if (normalAcceleration == null) normalAcceleration = GravitationalAcceleration;
        return Force(mass, (Vector3)normalAcceleration) * Mathf.Cos(angleOfIncline);
    }

    /// <summary>
    /// Calculate the force of gravity, assuming the object rests on an incline and the normal plane is the XZ plane (1, 0, 1)
    /// Uses the objects forward and right vectors (XZ plane) to calculate the angle of incline
    /// </summary>
    public static Vector3 NormalForce(float mass, Transform transform, Vector3? normalAcceleration = null) {
        if (normalAcceleration == null) normalAcceleration = GravitationalAcceleration;
        float angleOfIncline = AngleOfIncline(-transform.up, GravitationalAcceleration);
        return NormalForce(mass, normalAcceleration, angleOfIncline);
    }

    /// <summary>Get the absolute angle of incline relative to the direction of gravity (0, -1, 0)</summary>
    public static float AngleOfIncline(Vector3 up) {
        return AngleOfIncline(-up, GravitationalAcceleration);
    }

    /// <summary>Get the absolute angle of incline relative to the direction of worldUp</summary>
    public static float AngleOfIncline(Vector3 up, Vector3 gravity) {
        return Mathf.Abs(Vector3.Angle(up, gravity));
    }

    /// <summary>Calculate the force (with friction) against an object, assuming the normal force is gravity</summary>
    public static Vector3 ForceOfFriction(float mass, float angleOfIncline = 0f, float frictionCoefficient = 0f) {
        return frictionCoefficient * ForceOfGravity(mass, angleOfIncline);
    }

    /// <summary>Calculate the force (with friction) against an object, assuming the normal force is gravity</summary>
    public static Vector3 ForceOfFriction(float mass, Transform transform, float frictionCoefficient = 0f) {
        float angleOfIncline = AngleOfIncline(transform.up);
        //Debug.Log("Angle of Incline: " + angleOfIncline.ToString());
        return frictionCoefficient * ForceOfGravity(mass, angleOfIncline);
    }

    /// <summary>Calculate the force (with friction) against an object, assuming the normal force is <b>NOT</b> gravity</summary>
    public static Vector3 ForceOfFriction2(Vector3 normalForce, float frictionCoefficient = 0f) {
        return frictionCoefficient * normalForce;
    }

    /// <summary>Calculate force with friction on an incline assuming the force of gravity is coming from (0, -1, 0)</summary>
    public static Vector3 Force(float mass, Vector3 acceleration, Transform transform, float frictionCoefficient = 0f) {
        float angleOfIncline = AngleOfIncline(transform.up);
        return Force(mass, acceleration) - ForceOfFriction(mass, angleOfIncline, frictionCoefficient);
    }

    /// <summary>Calculate force with friction</summary>
    public static Vector3 Force(float mass, Vector3 acceleration, float frictionCoefficient = 0f, float angleOfIncline = 0f) {
        return Force(mass, acceleration) - ForceOfFriction(mass, angleOfIncline, frictionCoefficient);
    }

    /// <summary>Calculate force based on Newtons formula F=ma</summary>
    public static Vector3 Force(float mass, Vector3 acceleration) => mass * acceleration;

    #endregion
}
