using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Terrain", menuName = "Level/Terrain", order = 1)]
public class TerrainSO : ScriptableObject
{
    [SerializeField, InspectorName("Collision Priority")]
    private int _priority = int.MinValue;

    [HideInInspector]
    public int Priority => _priority;

    [SerializeField, InspectorName("Terrain Type")]
    private TerrainType _type = TerrainType.None;
    public TerrainType Type => _type;

    public TerrainEffect TerrainEffect;
}

[Serializable]
public class TerrainEffect {
    [Range(0.1f, 2f)]
    public float AccelerationModifier = 1f;
    [Range(0.1f, 2f)]
    public float VelocityModifier = 1f;
    [Range(0.1f, 2f)]
    [Tooltip("Affects Breaking")]
    public float CoefficientOfFriction = 1f;
}

public class TerrainEffectAccelerationModifier : TerrainEffect {
    float modifier = 1f;
}

/// <summary>
/// Used to identify specific terrain types
/// </summary>
public enum TerrainType
{
    Road,
    Dirt,
    None
}