using System.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MulticamGridCamera : NetworkBehaviour 
{
    public Action<MulticamGridCamera> OnCameraDestroy;
    [SerializeField] private Camera controlledCamera;

   // public NetworkVariable<int> NetworkCameraNumber = new NetworkVariable<int>();

    //public int CameraNumber => NetworkCameraNumber.Value;

    public void SetCameraRect(Rect cameraRect, float time = 0f)
    {
        if (time != 0f) throw new NotImplementedException();

        controlledCamera.rect = cameraRect;
        //SetCameraRectClientRpc(cameraRect.x, cameraRect.y, cameraRect.width, cameraRect.height, time);
    }

    public override void OnDestroy() {
        OnCameraDestroy?.Invoke(this);
    }

    // [ClientRpc]
    // private void SetCameraRectClientRpc(float x, float y, float width, float height, float time = 0f)
    // {
    //     var rect = new Rect(x, y, width, height);

    //     controlledCamera.rect = rect;
    // }
}
