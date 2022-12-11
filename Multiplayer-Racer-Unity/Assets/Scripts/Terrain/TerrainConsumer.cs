using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainConsumer : MonoBehaviour
{
    private readonly HashSet<TerrainProvider> _terrainProviders = new HashSet<TerrainProvider>();
    private TerrainProvider _activeTerrain = null;

    public TerrainType GetTerrainType() => _activeTerrain?.Type ?? TerrainType.Road;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<TerrainProvider>(out var terrainProvider))
        {
            _terrainProviders.Add(terrainProvider);
            RecalculateActiveTerrain();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<TerrainProvider>(out var terrainProvider))
        {
            _terrainProviders.Remove(terrainProvider);
            RecalculateActiveTerrain();
        }
    }

    /// <summary>
    /// Determines which terrain modifier is active based on the terrain providers the collider is in contact with
    /// </summary>
    private void RecalculateActiveTerrain()
    {
        int bestPriority = int.MinValue;
        _activeTerrain = null;
        foreach (TerrainProvider provider in _terrainProviders)
        {
            var priority = provider.Type.GetPriority();
            if (priority > bestPriority)
            {
                _activeTerrain = provider;
                bestPriority = priority;
            }
        }

        Debug.Log("Active Terrain: " + _activeTerrain?.Type ?? "NONE");
    }
}
