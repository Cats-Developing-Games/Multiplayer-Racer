using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class KennysPlayerBehaviour : NetworkBehaviour
{
    new CinemachineVirtualCamera camera;

    NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        var spawn = GameObject.Find("Spawn");
        //if (spawn != null) transform.position = spawn.transform.position;
        if (spawn != null) UpdatePositionServerRpc(NetworkObjectId, spawn.transform.position);
        if (!IsOwner) return;
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CinemachineVirtualCamera>();
        if (camera == null) Debug.LogError("Can't find a GameObject with the tag 'MainCamera'");
        camera.Follow = transform;
        camera.LookAt = transform;
    }

    void Update() {
        transform.position = netPosition.Value;
        
        if (!IsOwner) return;
        
        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        
        UpdatePositionServerRpc(NetworkObjectId, transform.position + moveDir * moveSpeed * Time.deltaTime);
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdatePositionServerRpc(ulong clientId, Vector3 pos) {
        UpdatePositionClientRpc(clientId, pos);
    }

    [ClientRpc]
    void UpdatePositionClientRpc(ulong clientId, Vector3 pos) {
        NetworkManager.ConnectedClients[clientId].PlayerObject.transform.position = pos;
        //NetworkManager.ConnectedClients[serverParams.Receive.SenderClientId].PlayerObject.netPosition = serverParams.Receive.
    }
}
