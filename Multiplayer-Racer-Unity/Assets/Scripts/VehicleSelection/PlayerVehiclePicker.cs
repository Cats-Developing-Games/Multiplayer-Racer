
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
    private int selectedVehicleIndex = 0;

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        InitializeVehiclePrefabs();
    }

    public void Update() {
        if(!IsOwner) return;

        if(Input.GetKeyDown(KeyCode.RightArrow)) 
            SelectNextVehicleServerRpc();
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
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

        SelectVehicle(0);
    }

    private void SelectVehicle(int index)
    {
        var total = vehicles.Count;
        var clampedIndex = (index + total) % total;

        vehiclePreviews[selectedVehicleIndex].SetActive(false);
        vehiclePreviews[clampedIndex].SetActive(true);
        selectedVehicleIndex = clampedIndex;
    }


    [ServerRpc]
    private void SelectNextVehicleServerRpc() {
        SelectVehicle(selectedVehicleIndex + 1);
    }

    [ServerRpc]
    private void SelectPreviousVehicleServerRpc() {
        SelectVehicle(selectedVehicleIndex - 1);
    }

    private GameObject InitializeVehiclePrefab(VehicleSO vehicle)
    {
        var prefab = vehicle.PreviewPrefab;

        var instance = Instantiate(prefab, previewAnchor.transform.position, Quaternion.identity, previewAnchor.transform);

        var networkObj = instance.GetComponent<NetworkObject>();
        if(!networkObj.TrySetParent(previewAnchor.transform, true))Debug.Log("Unable to set parent");
        networkObj.SpawnWithOwnership(this.OwnerClientId);

        instance.SetActive(false);

        return instance;
    }

    public void SetCameraRect(Rect cameraRect, float time = 0f)
    {
        if (time != 0f) throw new NotImplementedException();
        
        SetCameraRectClientRpc(cameraRect.x, cameraRect.y, cameraRect.width, cameraRect.height, time);
    }

    [ClientRpc]
    private void SetCameraRectClientRpc(float x, float y, float width, float height, float time = 0f) {
        var rect = new Rect(x, y, width, height);

        playerCamera.rect = rect;
    }
}
