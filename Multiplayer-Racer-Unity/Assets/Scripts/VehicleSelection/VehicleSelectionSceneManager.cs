using System.Runtime.InteropServices;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VehicleSelectionSceneManager : NetworkBehaviour
{
    [SerializeField] Camera noPlayerCamera;
    [SerializeField] GameObject playerViewPrefab;

    private Dictionary<ulong, PlayerVehiclePicker> players;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        var nm = NetworkManager.Singleton;
        SetUpCameraForClients(nm.ConnectedClientsIds);
        GetOwnerPlayerPicker().ShowStartGameButton();

        nm.OnClientConnectedCallback += InitializeNewClient;
        nm.OnClientDisconnectCallback += RemoveClient;
    }

    private PlayerVehiclePicker GetOwnerPlayerPicker() => players[NetworkManager.Singleton.LocalClientId];

    private void SetUpCameraForClients(IReadOnlyList<ulong> clientIds)
    {
        players = new Dictionary<ulong, PlayerVehiclePicker>();

        foreach (var clientId in clientIds)
        {
            var player = SpawnClientVehiclePicker(clientId);
            players.Add(clientId, player);
        }

        RecalculateCameraRects(clientIds);
    }

    private void PlayerChangedReadyUp(bool _, bool readyUp) => DetermineStartGameState();
    private void DetermineStartGameState()
    {
        bool allReady = players.All(kvp => kvp.Value.PlayerReady.Value);
        GetOwnerPlayerPicker().SetStartGameInteractable(allReady);
    }

    private void InitializeNewClient(ulong clientId)
    {
        var newPlayer = SpawnClientVehiclePicker(clientId);
        players.Add(clientId, newPlayer);
        RecalculateCameraRects(NetworkManager.Singleton.ConnectedClientsIds);
        DetermineStartGameState();
    }

    private void RemoveClient(ulong clientId)
    {
        players.Remove(clientId);

        var reducedClientIds = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != clientId));
        RecalculateCameraRects(reducedClientIds);
    }

    private void RecalculateCameraRects(IReadOnlyList<ulong> clientIds)
    {
        var clientViewports = CreateViewportRectsForClients(clientIds);

        foreach (var (clientId, player) in players)
        {
            player.SetCameraRect(clientViewports[clientId]);
        }
    }

    private PlayerVehiclePicker SpawnClientVehiclePicker(ulong clientId)
    {
        GameObject spawnedPrefab = Instantiate(playerViewPrefab, new Vector3((clientId + 1) * 100, 0, 0), Quaternion.identity);
        spawnedPrefab.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        var player = spawnedPrefab.GetComponent<PlayerVehiclePicker>();
        player.PlayerReady.OnValueChanged += PlayerChangedReadyUp;
        return player;
    }

    private Dictionary<ulong, Rect> CreateViewportRectsForClients(IReadOnlyList<ulong> clientIds)
    {
        var total = clientIds.Count;

        var output = new Dictionary<ulong, Rect>();
        if (total == 0) return output;

        var width = 1f / total;

        for (int i = 0; i < total; i++)
        {
            var offset = width * i;

            output.Add(clientIds[i], new Rect(offset, 0, width, 1f));
        }

        return output;
    }
}

