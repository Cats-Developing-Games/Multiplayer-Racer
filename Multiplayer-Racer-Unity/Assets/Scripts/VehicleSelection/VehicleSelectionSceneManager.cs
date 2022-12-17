using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VehicleSelectionSceneManager : NetworkBehaviour
{
    [SerializeField] Camera noPlayerCamera; 
    [SerializeField] GameObject playerViewPrefab;

    private int currentTrackedPlayers = 0;

    void OnNetworkSpawn()
    {
        if(!IsServer) return;
        var nm = NetworkManager.Singleton;
        SetUpCameraForClients(nm.ConnectedClientsIds);

        //nm.OnClientConnectedCallback

        //Camera.current.rec
    }

    private void SetUpCameraForClients(IReadOnlyList<ulong> clientIds)
    {
        var clientViewports = CreateViewportRectsForClients(clientIds);

        foreach(var clientId in clientIds) {
            
        }
    }

    private void SpawnCameraForClient(ulong clientId)
    {

    }

    private Dictionary<ulong, Rect> CreateViewportRectsForClients(IReadOnlyList<ulong> clientIds) 
    {
        var total = clientIds.Count;

        var output = new Dictionary<ulong, Rect>();
        if(total == 0) return output;

        var width = 1f / total;

        for(int i = 0; i < total; i++) {
            var offset = width * i;

            output.Add(clientIds[i], new Rect(offset, 0, width, 1f));
        }

        return output;
    }
}

