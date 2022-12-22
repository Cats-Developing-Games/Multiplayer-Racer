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
    private int currentRows = 1;
    private int currentColumns = 1;

    [SerializeField]
    [Range(0f, 5f)]
    private float animationSpeed = .5f;

    [ClientRpc]
    public void RecalculateCameraRectsClientRpc()
    {
        RecalculateCameraRects();
    }

    [ContextMenu("Recalculate Cameras")]
    public void RecalculateCameras() => RecalculateCameraRects();

    private void RecalculateCameraRects(MulticamGridCamera cameraToIgnore = null)
    {
        var discoveredCameras = DiscoverCameras(cameraToIgnore);

        Debug.Log($"Found {discoveredCameras.Count} cameras");
        var cameraBounds = CreateViewportRectsForClients(discoveredCameras);
        for (int i = 0; i < discoveredCameras.Count; i++)
        {
            var (target, source) = cameraBounds[i];

            discoveredCameras[i].SetInitializerPosition(source);
            discoveredCameras[i].SetCameraRect(target, animationSpeed);
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

    private List<(Rect target, Rect optionalSource)> CreateViewportRectsForClients(List<MulticamGridCamera> cameras)
    {
        var total = cameras.Count;

        var output = new List<(Rect target, Rect optionalSource)>();
        if (total == 0) return output;

        float width = Display.main.renderingWidth;
        float height = Display.main.renderingHeight;

        float targetRatio = 1f;

        float bestDistance = float.MaxValue;
        var (bestRows, bestColumns) = (1, 1);

        Debug.Log($"Actual dimensions {width} x {height}");

        for (int columns = 1; columns <= total; columns++)
        {
            int rows = Mathf.CeilToInt(total / (float)columns);

            // Don't want to add more cells than is needed
            if (columns != Mathf.CeilToInt(total / (float)rows)) continue;

            var testCellWidth = width / columns;
            var testCellHeight = height / rows;

            var ratio = testCellHeight / testCellWidth;

            Debug.Log($"Test row {rows} by {columns} with ratio: {ratio} with sides {testCellWidth} x {testCellHeight}");


            var distanceToTarget = Mathf.Abs(targetRatio - ratio);

            if (distanceToTarget < bestDistance)
            {
                Debug.Log("Updating Best");
                bestDistance = distanceToTarget;
                (bestRows, bestColumns) = (rows, columns);
            }
        }

        Debug.Log($"Best row {bestRows} by {bestColumns}");

        bool newRowAdded = bestRows != currentRows;
        bool newColumnAdded = bestColumns != currentColumns;

        float rectWidth = 1f / bestColumns;
        float rectHeight = 1f / bestRows;
        for (int rows = bestRows - 1; rows >= 0; rows--)
        {
            for (int columns = 0; columns < bestColumns; columns++)
            {
                var xOffset = rectWidth * columns;
                var yOffset = rectHeight * rows;

                var targetRect = new Rect(xOffset, yOffset, rectWidth, rectHeight);
                Rect optionalSourceRect = new Rect(newRowAdded ? 1f : xOffset, newColumnAdded ? -rectHeight : yOffset, rectWidth, rectHeight);

                output.Add((targetRect, optionalSourceRect));
            }
        }

        (currentRows, currentColumns) = (bestRows, bestColumns);

        return output;
    }
}