using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MulticamGrid : NetworkBehaviour
{
    private int rows { get; set; } = 0;
    private int columns { get; set; } = 0;

    // public override void OnNetworkSpawn()
    // {
    //     if (!IsServer) return;
    //     var nm = NetworkManager.Singleton;
    // }

    [ClientRpc]
    public void RecalculateCameraRectsClientRpc()
    {
        RecalculateCameraRects();
    }

    private void RecalculateCameraRects(MulticamGridCamera cameraToIgnore = null)
    {
        var discoveredCameras = DiscoverCameras(cameraToIgnore);

        Debug.Log("Discovered Count: " + discoveredCameras.Count);

        var cameraBounds = CreateViewportRectsForClients(discoveredCameras);
        for (int i = 0; i < cameraBounds.Count; i++)
        {
            discoveredCameras[i].SetCameraRect(cameraBounds[i]);
        }
    }

    private List<MulticamGridCamera> DiscoverCameras(MulticamGridCamera cameraToIgnore = null)
    {
        List<MulticamGridCamera> cameras = new List<MulticamGridCamera>();
        foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            // Debug.Log("Found go: " + go.name);

            if (go.TryGetComponent<MulticamGridCamera>(out MulticamGridCamera cam) && cam != cameraToIgnore)
            {
                cameras.Add(cam);
                cam.OnCameraDestroy += RecalculateCameraRects;
            }
        }

        return cameras;
    }

    private List<Rect> CreateViewportRectsForClients(List<MulticamGridCamera> cameras)
    {
        var total = cameras.Count;

        var output = new List<Rect>();
        if (total == 0) return output;

        var width = 1f / total;

        for (int i = 0; i < total; i++)
        {
            var offset = width * i;

            output.Add(new Rect(offset, 0, width, 1f));
        }

        return output;
    }

    // public void RegisterCamera(MulticamGridCamera cam)
    // {
    //     cam.NetworkCameraNumber.Value = GetNextCameraNumber();
    // }

    // private int GetNextCameraNumber() => discoveredCamera.Select(dc => dc.CameraNumber).DefaultIfEmpty(0).Max() + 1;
}

// public interface IControlCamera
// {
//     void SetCameraRect(Rect cameraRect, float time = 0f);
//     int CameraNumber { get; }
// }
