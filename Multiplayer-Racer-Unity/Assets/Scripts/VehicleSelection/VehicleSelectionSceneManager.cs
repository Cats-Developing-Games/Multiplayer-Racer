using System.Runtime.InteropServices;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VehicleSelectionSceneManager : NetworkBehaviour
{
    [SerializeField] Camera noPlayerCamera;
    [SerializeField] GameObject playerViewPrefab;

    private Dictionary<ulong, PlayerVehiclePicker> s_players;

    private MulticamGrid s_multicamGrid;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            NetworkManager.OnClientDisconnectCallback += (u) => SceneManager.LoadScene("MainMenuScene");
            return;
        }
        var nm = NetworkManager.Singleton;
        s_multicamGrid = GetComponent<MulticamGrid>();
        InitializePlayerVehiclePickers(nm.ConnectedClientsIds);
        GetOwnerPlayerPicker().ShowStartGameButton();

        nm.OnClientConnectedCallback += InitializeNewClient;
        nm.OnClientDisconnectCallback += RemoveClient;
    }

    private PlayerVehiclePicker GetOwnerPlayerPicker() => s_players[NetworkManager.Singleton.LocalClientId];

    private void InitializePlayerVehiclePickers(IReadOnlyList<ulong> clientIds)
    {
        s_players = new Dictionary<ulong, PlayerVehiclePicker>();

        foreach (var clientId in clientIds)
        {
            var player = SpawnClientVehiclePicker(clientId);
            s_players.Add(clientId, player);
        }

        s_multicamGrid.RecalculateCameraRectsClientRpc();
    }

    private void PlayerChangedReadyUp(bool _, bool readyUp) => DetermineStartGameState();
    private void DetermineStartGameState()
    {
        bool allReady = s_players.All(kvp => kvp.Value.PlayerReady.Value);
        GetOwnerPlayerPicker().SetStartGameInteractable(allReady);
    }

    private void InitializeNewClient(ulong clientId)
    {
        var newPlayer = SpawnClientVehiclePicker(clientId);
        s_players.Add(clientId, newPlayer);
        s_multicamGrid.RecalculateCameraRectsClientRpc();
        DetermineStartGameState();
    }

    private void RemoveClient(ulong clientId)
    {
        s_players.Remove(clientId);
    }

    private PlayerVehiclePicker SpawnClientVehiclePicker(ulong clientId)
    {
        GameObject spawnedPrefab = Instantiate(playerViewPrefab, new Vector3((clientId + 1) * 100, 0, 0), Quaternion.identity);
        spawnedPrefab.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        var player = spawnedPrefab.GetComponent<PlayerVehiclePicker>();
        player.PlayerReady.OnValueChanged += PlayerChangedReadyUp;
        return player;
    }
}

