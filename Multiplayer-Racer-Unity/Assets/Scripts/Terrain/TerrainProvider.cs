using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainProvider : MonoBehaviour
{
    [SerializeField, InspectorName("Terrain Type")] private TerrainSO _terrain;
    
    public TerrainSO Terrain => _terrain;
}