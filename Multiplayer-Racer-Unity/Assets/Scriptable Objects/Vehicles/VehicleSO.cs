using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle", menuName = "Vehicles/Vehicle", order = 1)]
public class VehicleSO : ScriptableObject
{
    [Header("Vehicle Selection")]
    public string VehicleName = "";
    public GameObject InGamePrefab;
    public GameObject PreviewPrefab;
    

    [Header("General")]
    [DisplayStat(1, 300, 3000)]
    /// <summary>Kilograms</summary>
    public float Mass = VehicleDefaults.Mass;

    [Header("Engine")]
    public Vector3 EngineForce = new Vector3(0f, 0f, VehicleDefaults.EngineForce);

    [DisplayStat(3, 5f, 20f, StatName = "Engine Power")]
    private float GetEngineForceFloat() => EngineForce.z;


    [DisplayStat(2, 2f, 30f, StatName = "Max Speed")]
    public float MaxSpeed = VehicleDefaults.MaxSpeed;

    public Vector3 GetAcceleration() => KineticPhysics.Acceleration(EngineForce, Mass);

    

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
    public static readonly Vector3 RollingDeceleration = new Vector3(0f, 0f, -1f);
    // Unity's old input systems GetAxis increased/decreased 0.05 per step. Assuming it was ran in FixedUpdate (50/sec), thats 0->1 in 2.5 seconds
    public const float KeyboardTimeToMaxTurn = 2.5f;

    // General
    /// <summary>Kilograms</summary>
    public const float Mass = 1360f;

    // Engine
    public const float EngineForce = 10f;
    public const float MaxSpeed = 10f;
    public const float Acceleration = 10f;

    // Steering
    public const float MinTurnRadius = 1f;
    public const float SteeringModifier = 1f;

    // Traction
    public const float Traction = 1f;
}
