using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(TMPro.TextMeshPro))]
[RequireComponent(typeof(NetworkTransform))]
public class WorldSpaceFloatingName : NetworkBehaviour
{
    [SerializeField] new string name;
    [SerializeField] Transform follow;
    new GameObject camera;

    // Update is called once per frame
    void Start()
    {
        EnsureHasCamera();
        TMPro.TextMeshPro text = GetComponent<TMPro.TextMeshPro>();
        if (name == "") name = "Player";
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.text = name;
    }

    void Update()
    {
        if(EnsureHasCamera()) {
            transform.LookAt(camera.transform);
            transform.position = new Vector3(follow.position.x, follow.position.y + 1f, follow.position.z);
        }
       
    }

    private bool EnsureHasCamera()
    {
        // Already has valid camera target
        if(HasCamera()) return true;

        // Get main camera
        camera = GameObject.FindGameObjectWithTag("MainCamera");

        Debug.Log("Reacquired camera target in scene: " + camera?.scene.name ?? "[NOT FOUND]");
        return HasCamera();
    }

    private bool HasCamera() => camera != null;
}
