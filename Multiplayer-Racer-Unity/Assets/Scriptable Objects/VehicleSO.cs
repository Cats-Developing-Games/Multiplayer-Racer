using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle", menuName = "Vehicles/Vehicle", order = 1)]
public class VehicleSO : ScriptableObject
{
    [Header("General")]
    /// <summary>Kilograms</summary>
    public float Mass = VehicleDefaults.Mass;

    [Header("Engine")]
    public float EngineForce = VehicleDefaults.EngineForce;
    public float MaxVelocity = VehicleDefaults.MaxVelocity;

    public float GetAcceleration() => KineticPhysics.Acceleration(EngineForce, Mass);

    [Header("Steering")]
    public float MinTurnRadius = VehicleDefaults.MinTurnRadius;
    public float SteeringModifier = VehicleDefaults.SteeringModifier;

    //[Header("Breaking")]
    //public static readonly float DefaultBreakDeceleration = 10f;
    //public float BreakDeceleration = DefaultBreakDeceleration;

    [Header("Traction")]
    [Description("Traction is multiplied to the cars coefficient of friction")]
    // TODO: applying traction this way does not feel right, but I am not sure how it should be done
    // Also see ArcadePlayerController.calcFrictionCoefficient
    public float WheelTraction = VehicleDefaults.Traction;
}

struct VehicleDefaults {
    public const float RollingDeceleration = -1f;
    // Unity's old input systems GetAxis increased/decreased 0.05 per step. Assuming it was ran in FixedUpdate (50/sec), thats 0->1 in 2.5 seconds
    public const float KeyboardTimeToMaxTurn = 2.5f;

    // General
    /// <summary>Kilograms</summary>
    public const float Mass = 1360f;

    // Engine
    public const float EngineForce = 10f;
    public const float MaxVelocity = 10f;
    public const float Acceleration = 10f;

    // Steering
    public const float MinTurnRadius = 1f;
    public const float SteeringModifier = 1f;

    // Traction
    public const float Traction = 1f;
}
