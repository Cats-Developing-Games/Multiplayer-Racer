using System.Collections.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MulticamGridCamera : NetworkBehaviour 
{
    private bool isInitialPosition = true;
    public bool IsInitialPosition => isInitialPosition;

    public Action<MulticamGridCamera> OnCameraDestroy;
    [SerializeField] private Camera controlledCamera;

    public void SetCameraRect(Rect cameraRect, float time = 0f)
    {
        if (time != 0f) throw new NotImplementedException();
        isInitialPosition = false;

        controlledCamera.rect = cameraRect;
    }

    public override void OnDestroy() {
        OnCameraDestroy?.Invoke(this);
    }
}
