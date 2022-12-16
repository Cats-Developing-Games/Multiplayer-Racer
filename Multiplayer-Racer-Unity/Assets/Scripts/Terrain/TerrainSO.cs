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