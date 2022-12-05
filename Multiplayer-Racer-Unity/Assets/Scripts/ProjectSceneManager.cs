using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectSceneManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log("Spanwed on network");
        base.OnNetworkSpawn();
    }
}
