using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainProvider : MonoBehaviour
{
    [SerializeField, InspectorName("Terrain Type")] private TerrainType _type;

    public TerrainType Type => _type;
}

/// <summary>
/// Used to identify specific terrain types
/// </summary>
public enum TerrainType
{
    Road = 10,
    Dirt = 0
}

public static class TerrainTypeExtensions
{
    /// <summary>
    /// Gets the priority of terrain types to be used when in contact with multiple types at once
    /// </summary>
    public static int GetPriority(this TerrainType type) => (int)type; 
}