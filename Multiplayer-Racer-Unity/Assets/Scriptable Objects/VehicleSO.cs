using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle", menuName = "Vehicles/Vehicle", order = 1)]
public class VehicleSO : ScriptableObject
{
    [Header("Motor")]
    public float MaxVelocity = 10f;
    public float Acceleration = 10f;

    [Header("Breaking")]
    public float BreakDeceleration = 10f;

    [Header("Steering")]
    public float MinTurnRadius = 1f;
}
