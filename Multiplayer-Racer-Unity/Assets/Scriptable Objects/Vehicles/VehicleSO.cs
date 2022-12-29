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
    [DisplayStat(3, 2000f, 6000f, StatName = "Engine Power")]
    public float EngineForce = VehicleDefaults.EngineForce;

    [DisplayStat(2, 2f, 30f, StatName = "Max Speed")]
    public float MaxSpeed = VehicleDefaults.MaxSpeed;
    
    [DisplayStat(4, -2f, -.5f)]
    private float Handling => -MinTurnRadius;

    [Header("Steering")]
    public float MinTurnRadius = VehicleDefaults.MinTurnRadius;
    [Range(0f, 90f)]
    /// <summary>Degrees</summary>
    public float SteeringAngle = VehicleDefaults.SteeringAngle;

    //[Header("Breaking")]
    //public static readonly float DefaultBreakDeceleration = 10f;
    //public float BreakDeceleration = DefaultBreakDeceleration;

    [Header("Traction")]
    [Description("(Experimental) Traction is multiplied to the cars coefficient of friction")]
    // TODO: applying traction this way does not feel right, but I am not sure how it should be done
    // Also see ArcadePlayerController.calcFrictionCoefficient
    public float WheelTraction = VehicleDefaults.Traction;
}

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

    // Traction
    public const float Traction = 1f;
}
