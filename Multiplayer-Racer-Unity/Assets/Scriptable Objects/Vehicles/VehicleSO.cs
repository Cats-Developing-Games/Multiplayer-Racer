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
    public float SteeringModifier = VehicleDefaults.SteeringModifier;

    //[Header("Breaking")]
    //public static readonly float DefaultBreakDeceleration = 10f;
    //public float BreakDeceleration = DefaultBreakDeceleration;

    [Header("Traction")]
    [Description("(Experimental) Traction is multiplied to the cars coefficient of friction")]
    // TODO: applying traction this way does not feel right, but I am not sure how it should be done
    // Also see ArcadePlayerController.calcFrictionCoefficient
    public float WheelTraction = VehicleDefaults.Traction;
}
