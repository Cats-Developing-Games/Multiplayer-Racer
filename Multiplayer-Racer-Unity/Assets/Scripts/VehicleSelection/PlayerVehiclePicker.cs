using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerVehiclePicker : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private VehicleSO[] vehicles;

    public void SetCameraRect(Rect cameraRect, float time = 0f) 
    {
        if(time != 0f) throw new NotImplementedException();
        else playerCamera.rect = cameraRect;
    }
}
