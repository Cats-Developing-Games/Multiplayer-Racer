using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    // Update is called once per frame
    void Update()
    {
        //if(!IsOwner) return;

        var moveDir = Vector3.zero;

        if(Input.GetKey(KeyCode.UpArrow)) moveDir.z += 1f;
        if(Input.GetKey(KeyCode.DownArrow)) moveDir.z -= 1f;
        if(Input.GetKey(KeyCode.LeftArrow)) moveDir.x -= 1f;
        if(Input.GetKey(KeyCode.RightArrow)) moveDir.x += 1f;

        float moveSpeed = 5f;

        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}
