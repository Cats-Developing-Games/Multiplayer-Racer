using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class XernersPlayerBehaviour : NetworkBehaviour
{
    new CinemachineVirtualCamera camera;

    float moveSpeed = 3f;

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        var spawn = GameObject.Find("Spawn");
        if (spawn != null) UpdatePositionServerRpc(OwnerClientId, spawn.transform.position);
        if (!IsOwner) return;
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CinemachineVirtualCamera>();
        if (camera == null) Debug.LogError("Can't find a GameObject with the tag 'MainCamera'");
        camera.Follow = transform;
        camera.LookAt = transform;
    }

    void Update() {
        if (!IsOwner) return;
        
        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;
        if (!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))) return;

        UpdateVelocityServerRpc(OwnerClientId, moveDir, Time.deltaTime);
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateVelocityServerRpc(ulong clientId, Vector3 moveDir, float deltaTime) {
        Debug.Log("Moving client " + OwnerClientId.ToString() + " in the direction " + moveDir.ToString());
        NetworkManager.ConnectedClients[clientId].PlayerObject.transform.position += moveDir * moveSpeed * deltaTime;
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdatePositionServerRpc(ulong clientId, Vector3 pos) {
        Debug.Log("Moving client " + OwnerClientId.ToString() + " to position " + pos.ToString());
        NetworkManager.ConnectedClients[clientId].PlayerObject.transform.position = pos;
    }
}
