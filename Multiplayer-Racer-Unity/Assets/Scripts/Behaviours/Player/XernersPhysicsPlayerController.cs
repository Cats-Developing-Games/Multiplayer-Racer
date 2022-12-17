using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class XernersPhysicsPlayerController : NetworkBehaviour
{
    new CinemachineVirtualCamera camera;

    [Header("Movement")]
    //[SerializeField] float groundDrag;
    //[SerializeField] NetworkObject networkObject
    [SerializeField] float motorForce;
    [SerializeField] float breakForce;
    [SerializeField] float maxSteeringAngle;
    Quaternion originalRotation;

    [SerializeField] bool preventZFlipping = false;
    [SerializeField] float maxAbsZRotationAllowed = 45f;

    float horizontalInput;
    float verticalInput;
    float currentSteerAngle;
    float currentBreakForce;
    bool isBreaking;

    [Header("Colliders")]
    [SerializeField] WheelCollider frontLeftWheel;
    [SerializeField] WheelCollider backLeftWheel;
    [SerializeField] WheelCollider backRightWheel;
    [SerializeField] WheelCollider frontRightWheel;

    public override void OnNetworkSpawn() {
        originalRotation = transform.rotation;
        var spawn = GameObject.Find("Spawn");
        if (spawn != null) UpdatePositionServerRpc(OwnerClientId, spawn.transform.position);
        if (!IsOwner) return;
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CinemachineVirtualCamera>();
        if (camera == null) Debug.LogError("Can't find a GameObject with the tag 'MainCamera'");
        //camera.Follow = transform;
        camera.LookAt = transform;
    }

    void FixedUpdate() {
        if (!IsOwner) return;
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        Unflip();
        PreventHorizontalTipping();
        //UpdateVelocityServerRpc(OwnerClientId, moveDir, Time.deltaTime);
    }

    void PreventHorizontalTipping() {
        if (preventZFlipping && Math.Abs(transform.rotation.z) > maxAbsZRotationAllowed) {
            Vector3 myRotation = transform.rotation.eulerAngles;
            myRotation.z = Mathf.Clamp(myRotation.z, -45f, 45f);
            transform.rotation = Quaternion.Euler(myRotation);
        }
    }

    void Unflip() {
        if (Vector3.Angle(Vector3.up, transform.up) > 90f) {
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
            //transform.rotation = originalRotation;
            Vector3 myRotation = transform.rotation.eulerAngles;
            myRotation.z = Mathf.Clamp(myRotation.x, -45f, 45f);
            transform.rotation = Quaternion.Euler(myRotation);
        }
    }

    void UpdateWheels() {
        // No wheels, no cap. fr fr
    }

    void HandleSteering() {
        currentSteerAngle = maxSteeringAngle * horizontalInput;
        frontLeftWheel.steerAngle = currentSteerAngle;
        frontRightWheel.steerAngle = currentSteerAngle;
    }

    void HandleMotor() {
        frontLeftWheel.motorTorque = verticalInput * motorForce;
        frontRightWheel.motorTorque = verticalInput * motorForce;
        currentBreakForce = isBreaking ? breakForce : 0f;
        if (isBreaking) {
            ApplyBreaking();
        }
    }

    public virtual void ApplyBreaking() {
        frontLeftWheel.brakeTorque = currentBreakForce;
        frontRightWheel.brakeTorque = currentBreakForce;
        backLeftWheel.brakeTorque = currentBreakForce;
        backRightWheel.brakeTorque = currentBreakForce;
    }

    void GetInput() {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateVelocityServerRpc(ulong clientId, Vector3 moveDir, float deltaTime) {
        //Debug.Log("Moving client " + OwnerClientId.ToString() + " in the direction " + moveDir.ToString());
        //NetworkManager.ConnectedClients[clientId].PlayerObject.transform.position += moveDir * moveSpeed * deltaTime;
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdatePositionServerRpc(ulong clientId, Vector3 pos) {
        Debug.Log("Moving client " + OwnerClientId.ToString() + " to position " + pos.ToString());
        NetworkManager.ConnectedClients[clientId].PlayerObject.transform.position = pos;
    }
}
