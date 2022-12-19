
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerVehiclePicker : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject previewAnchor;
    [SerializeField] private List<VehicleSO> vehicles;
    [SerializeField] private GameObject[] hideWhenNotOwner;
    [SerializeField] private Button[] lockWhenReadiedUp;
    [SerializeField] private Button readyUpButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private VehicleStatsViewer statsViewer;

    private TMP_Text readyUpButtonText;
    private List<GameObject> vehiclePreviews;
    private NetworkVariable<int> selectedVehicleIndex = new NetworkVariable<int>(0);
    public NetworkVariable<bool> PlayerReady = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        readyUpButtonText = readyUpButton.GetComponentInChildren<TMP_Text>();
        selectedVehicleIndex.OnValueChanged += ChangeSelectedVehicle;
        PlayerReady.OnValueChanged += ToggleVehicleLocked;
        InitializeVehiclePrefabs();

        if (!IsOwner)
        {
            foreach (var hideGameObject in hideWhenNotOwner) hideGameObject.SetActive(false);
            readyUpButton.interactable = false;
        }
    }

    public void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SelectNextVehicleServerRpc();
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            SelectPreviousVehicleServerRpc();
    }

    #region Ready Up UI
    public void ShowStartGameButton() => startGameButton.gameObject.SetActive(true);

    public void SetStartGameInteractable(bool interactable) => startGameButton.interactable = interactable;

    private void ToggleVehicleLocked(bool _, bool setIsReady)
    {
        readyUpButtonText.SetText(setIsReady ? "Unready" : "Ready Up");

        // Making it so prev and next button are disabled
        foreach(var btn in lockWhenReadiedUp) btn.interactable = !setIsReady;
    }
    #endregion

    #region Vehicle Selection
    private void ChangeSelectedVehicle(int previousIndex, int newIndex)
    {
        vehiclePreviews[previousIndex].SetActive(false);
        EnableVehiclePreview(newIndex);
        statsViewer.DisplayStats(vehicles[newIndex]);
    }


    private void InitializeVehiclePrefabs()
    {
        vehiclePreviews = new List<GameObject>();
        if (vehicles.Count == 0) return;

        foreach (var vehicle in vehicles)
        {
            vehiclePreviews.Add(InitializeVehiclePrefab(vehicle));
        }

        ChangeSelectedVehicle(0, selectedVehicleIndex.Value);
    }

    private GameObject InitializeVehiclePrefab(VehicleSO vehicle)
    {
        var prefab = vehicle.PreviewPrefab;

        var instance = Instantiate(prefab, previewAnchor.transform.position, Quaternion.identity, previewAnchor.transform);
        instance.SetActive(false);

        return instance;
    }

    private void EnableVehiclePreview(int index) => vehiclePreviews[index].SetActive(true);

    public void SelectNextVehicle()
    {
        if (IsOwner) SelectNextVehicleServerRpc();
    }

    public void ToggleVehicleReady()
    {
        if (IsOwner) ToggleVehicleReadyServerRpc();
    }

    [ServerRpc]
    void ToggleVehicleReadyServerRpc() => PlayerReady.Value = !PlayerReady.Value;


    public void SelectPreviousVehicle()
    {
        if (IsOwner) SelectPreviousVehicleServerRpc();
    }

    [ServerRpc]
    private void SetSelectedVehicleIndexServerRpc(int index)
    {
        // Can't change selected vehicle index when player is ready
        if(PlayerReady.Value) return;  

        var total = vehicles.Count;
        var clampedIndex = (index + total) % total;

        selectedVehicleIndex.Value = clampedIndex;
    }

    [ServerRpc] private void SelectNextVehicleServerRpc() => SetSelectedVehicleIndexServerRpc(selectedVehicleIndex.Value + 1);
    [ServerRpc] private void SelectPreviousVehicleServerRpc() => SetSelectedVehicleIndexServerRpc(selectedVehicleIndex.Value - 1);

    #endregion

    #region Camera Viewbox
    public void SetCameraRect(Rect cameraRect, float time = 0f)
    {
        if (time != 0f) throw new NotImplementedException();

        SetCameraRectClientRpc(cameraRect.x, cameraRect.y, cameraRect.width, cameraRect.height, time);
    }

    [ClientRpc]
    private void SetCameraRectClientRpc(float x, float y, float width, float height, float time = 0f)
    {
        var rect = new Rect(x, y, width, height);

        playerCamera.rect = rect;
    }
    #endregion
}
