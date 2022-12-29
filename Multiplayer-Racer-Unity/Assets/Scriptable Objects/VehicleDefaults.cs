using UnityEngine;

struct VehicleDefaults {
    public static readonly Vector3 RollingDeceleration = new Vector3(0f, 0f, -1f);

    // General
    /// <summary>Kilograms</summary>
    public const float Mass = 1360f;

    // Engine
    /// <summary>Assumed to be applied in the Z direction</summary>
    public const float EngineForce = 3000f;
    public const float MaxSpeed = 10f;
    public const float Acceleration = 10f;

    // Steering
    public const float MinTurnRadius = 1f;
    /// <summary>Degrees</summary>
    public const float SteeringAngle = 65f;
    public const float SteeringModifier = 1f;

    // Traction
    public const float Traction = 1f;
}