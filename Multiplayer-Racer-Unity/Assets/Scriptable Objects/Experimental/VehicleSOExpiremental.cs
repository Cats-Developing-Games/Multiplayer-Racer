using UnityEngine;

//[CreateAssetMenu(fileName = "Vehicle", menuName = "Vehicles/Vehicle", order = 1)]
public class VehicleSOExperimental : ScriptableObject
{
    [Header("General")]
    /// <summary>Kilograms</summary>
    public float Mass = VehicleDefaults.Mass;

    [Header("Engine")]
    public float EngineForce = VehicleDefaults.EngineForce;
    public float MaxSpeed = VehicleDefaults.MaxSpeed;

    [Header("Steering")]
    public float MinTurnRadius = VehicleDefaults.MinTurnRadius;
    [Range(0f, 90f)][Description("Experimental. Doesnt do anything if the car is using circle based movement")]
    /// <summary>Degrees</summary>
    public float SteeringAngle = VehicleDefaults.SteeringAngle;
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
