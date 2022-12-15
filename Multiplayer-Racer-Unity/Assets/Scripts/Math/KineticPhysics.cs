using System;
using UnityEngine;

public static class KineticPhysics {
    public const float GravitationalAcceleration = 9.80665f; // https://en.wikipedia.org/wiki/Gravity_of_Earth
    public static readonly Vector3 DirectionOfGravity = new Vector3(0f, -1f, 0f);

    #region Displacement

    public static float Displacement(float velocity, float acceleration, float deltaTime) {
        return (velocity * deltaTime) + (0.5f) * (acceleration * ((float)Mathf.Pow(deltaTime, 2)));
    }

    public static Vector3 Displacement(Vector3 velocity, Vector3 acceleration, float deltaTime) {
        return (velocity * deltaTime) + (0.5f) * (acceleration * ((float)Mathf.Pow(deltaTime, 2)));
    }

    #endregion

    /// <summary>Calculate acceleration using Newtons formula A=f/m</summary>
    public static float Acceleration(float force, float mass) => force / mass;

    #region Force

    /// <summary>Calculate the force of gravity</summary>
    public static float ForceOfGravity(float mass, float angleOfIncline = 0f) {
        return NormalForce(mass, GravitationalAcceleration, angleOfIncline);
    }

    /// <summary>Calculate the force of gravity, assuming the object rests on an incline</summary>
    public static float NormalForce(float mass, float normalAcceleration = GravitationalAcceleration, float angleOfIncline = 0f) {
        return Force(mass, normalAcceleration) * Mathf.Cos(angleOfIncline);
    }

    /// <summary>
    /// Calculate the force of gravity, assuming the object rests on an incline and the normal plane is the XZ plane (1, 0, 1)
    /// Uses the objects forward and right vectors (XZ plane) to calculate the angle of incline
    /// </summary>
    public static float NormalForce(float mass, Transform transform, float normalAcceleration = GravitationalAcceleration) {
        float angleOfIncline = AngleOfIncline(-transform.up, DirectionOfGravity);
        return NormalForce(mass, normalAcceleration, angleOfIncline);
    }

    /// <summary>Get the absolute angle of incline relative to the direction of gravity (0, -1, 0)</summary>
    public static float AngleOfIncline(Vector3 up) {
        return AngleOfIncline(-up, DirectionOfGravity);
    }

    /// <summary>Get the absolute angle of incline relative to the direction of worldUp</summary>
    public static float AngleOfIncline(Vector3 up, Vector3 directionOfGravity) {
        return Mathf.Abs(Vector3.Angle(up, directionOfGravity));
    }

    /// <summary>Calculate the force (with friction) against an object, assuming the normal force is gravity</summary>
    public static float ForceOfFriction(float mass, float frictionCoefficient = 0f, float angleOfIncline = 0f) {
        return frictionCoefficient * ForceOfGravity(mass, angleOfIncline);
    }

    /// <summary>Calculate the force (with friction) against an object, assuming the normal force is <b>NOT</b> gravity</summary>
    public static float ForceOfFriction2(float normalForce, float frictionCoefficient = 0f) {
        return frictionCoefficient * normalForce;
    }

    /// <summary>Calculate force with friction on an incline assuming the force of gravity is coming from (0, -1, 0)</summary>
    public static float Force(float mass, float acceleration, Transform transform, float frictionCoefficient = 0f) {
        float angleOfIncline = AngleOfIncline(transform.up);
        return Force(mass, acceleration) - ForceOfFriction(mass, frictionCoefficient, angleOfIncline);
    }

    /// <summary>Calculate force with friction</summary>
    public static float Force(float mass, float acceleration, float frictionCoefficient = 0f, float angleOfIncline = 0f) {
        return Force(mass, acceleration) - ForceOfFriction(mass, frictionCoefficient, angleOfIncline);
    }

    /// <summary>Calculate force based on Newtons formula F=ma</summary>
    public static float Force(float mass, float acceleration) => mass / acceleration;

    #endregion
}
