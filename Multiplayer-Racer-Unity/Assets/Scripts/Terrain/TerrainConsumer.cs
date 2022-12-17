using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TerrainConsumer : MonoBehaviour
{
    public UnityEvent<TerrainSO> OnActiveTerrainChange;

    private readonly HashSet<TerrainProvider> terrainProviders = new HashSet<TerrainProvider>();
    private TerrainProvider activeTerrain = null;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<TerrainProvider>(out var terrainProvider))
        {
            terrainProviders.Add(terrainProvider);
            RecalculateActiveTerrain();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<TerrainProvider>(out var terrainProvider))
        {
            terrainProviders.Remove(terrainProvider);
            RecalculateActiveTerrain();
        }
    }

    /// <summary>
    /// Determines which terrain modifier is active based on the terrain providers the collider is in contact with
    /// </summary>
    private void RecalculateActiveTerrain()
    {
        int bestPriority = int.MinValue;
        activeTerrain = null;
        TerrainProvider bestTerrain = null;
        foreach (TerrainProvider provider in terrainProviders)
        {
            var priority = provider.Terrain.Priority;
            if (priority > bestPriority)
            {
                bestTerrain = provider;
                bestPriority = priority;
            }
        }

        if(bestTerrain is not null && bestTerrain != activeTerrain) 
            OnActiveTerrainChange.Invoke(bestTerrain.Terrain);

        activeTerrain = bestTerrain;
    }
}
