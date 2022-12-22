using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnimationTestController : MonoBehaviour
{
    [SerializeField] private GameObject cameraPrefab;
    private MulticamGrid grid;

    public void Start() {
        grid = GetComponent<MulticamGrid>();
    }

    [ContextMenu("Spawn Camera")]
    public void SpawnCamera() {
        var newCamera = Instantiate(cameraPrefab);
        grid.RecalculateCameras();
    }
}
