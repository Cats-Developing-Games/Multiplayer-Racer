
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerVehiclePicker : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject previewAnchor;
    [SerializeField] private List<VehicleSO> vehicles;
    private List<GameObject> vehiclePreviews;
    private NetworkVariable<int> selectedVehicleIndex = new NetworkVariable<int>(0);

    public override void OnNetworkSpawn()
    {
        selectedVehicleIndex.OnValueChanged += ChangeSelectedVehicle;
        InitializeVehiclePrefabs();
    }

    private void ChangeSelectedVehicle(int previousIndex, int newIndex)
    {
        vehiclePreviews[previousIndex].SetActive(false);
        EnableVehiclePreview(newIndex);
    }

    public void Update()
    {


        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SelectNextVehicleServerRpc();
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            SelectPreviousVehicleServerRpc();
    }



    private void InitializeVehiclePrefabs()
    {
        vehiclePreviews = new List<GameObject>();
        if (vehicles.Count == 0) return;

        foreach (var vehicle in vehicles)
        {
            vehiclePreviews.Add(InitializeVehiclePrefab(vehicle));
        }

        EnableVehiclePreview(selectedVehicleIndex.Value);
    }

    private GameObject InitializeVehiclePrefab(VehicleSO vehicle)
    {
        var prefab = vehicle.PreviewPrefab;

        var instance = Instantiate(prefab, previewAnchor.transform.position, Quaternion.identity, previewAnchor.transform);
        instance.SetActive(false);

        return instance;
    }

    private void EnableVehiclePreview(int index) => vehiclePreviews[index].SetActive(true);


    [ServerRpc]
    private void SetSelectedVehicleIndexServerRpc(int index)
    {
        var total = vehicles.Count;
        var clampedIndex = (index + total) % total;

        selectedVehicleIndex.Value = clampedIndex;
    }

    [ServerRpc] public void SelectNextVehicleServerRpc() => SetSelectedVehicleIndexServerRpc(selectedVehicleIndex.Value + 1);
    [ServerRpc] public void SelectPreviousVehicleServerRpc() => SetSelectedVehicleIndexServerRpc(selectedVehicleIndex.Value - 1);

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
