using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MulticamGrid : NetworkBehaviour
{
    [ClientRpc]
    public void RecalculateCameraRectsClientRpc()
    {
        RecalculateCameraRects();
    }

    private void RecalculateCameraRects(MulticamGridCamera cameraToIgnore = null)
    {
        var discoveredCameras = DiscoverCameras(cameraToIgnore);

        Debug.Log($"Found {discoveredCameras.Count} cameras");
        var cameraBounds = CreateViewportRectsForClients(discoveredCameras);
        for (int i = 0; i < discoveredCameras.Count; i++)
        {
            discoveredCameras[i].SetCameraRect(cameraBounds[i]);
        }
    }

    private List<MulticamGridCamera> DiscoverCameras(MulticamGridCamera cameraToIgnore = null)
    {
        List<MulticamGridCamera> cameras = new List<MulticamGridCamera>();
        foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
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

        float width = Screen.width;
        float height = Screen.height;

        float targetRatio = 1f;

        float bestDistance = float.MaxValue;
        var (bestRows, bestColumns) = (1, 1);

        for(int columns = 1; columns <= total; columns++) {
            int rows = Mathf.CeilToInt(total / (float)columns);

            // Don't want to add more cells than is needed
            if(columns != Mathf.CeilToInt(total / (float)rows)) continue;
            
            var testCellWidth = width / columns;
            var testCellHeight = height / rows;

            var ratio = testCellHeight / testCellWidth;

            var distanceToTarget = Mathf.Abs(targetRatio - ratio);

            if(distanceToTarget < bestDistance) {
                bestDistance = distanceToTarget;
                (bestRows, bestColumns) = (rows, columns);
            }            
        }

        Debug.Log($"Best row {bestRows} by {bestColumns}");

        float rectWidth = 1f / bestColumns;
        float rectHeight = 1f / bestRows;
        for (int columns = 0; columns < bestColumns; columns++)
        {
            for (int rows = bestRows - 1; rows >= 0; rows--) {
                var xOffset = rectWidth * columns;
                var yOffset = rectHeight * rows;

                output.Add(new Rect(xOffset, yOffset, rectWidth, rectHeight));
            }
        }

        return output;
    }
}