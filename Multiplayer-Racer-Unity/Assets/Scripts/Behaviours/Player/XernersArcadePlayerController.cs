using System;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using Unity.Netcode.Components;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(PlayerInput))]
public class XernersArcadePlayerController : NetworkBehaviour
{
    // TODO: change vehicle accel, velocity, and force calculations to be Vector3. not floats

    [Header("Vehicle")]
    [Vehicle][SerializeField] VehicleSO vehicleSO;

    [Space]
    [Header("Camera")]
    [SerializeField] bool follow = false;
    [SerializeField] bool lookAt = false;
    new CinemachineVirtualCamera camera;

    [Header("Gizmos")]
    [SerializeField] bool drawGizmos = false;
    [SerializeField] bool drawMininumTurnRadius = false;

    // Input
    VehicleInputHandler input;
    VehicleMovement movement;

    // temporary. should be passed from outside this class
    public float RoadFrictionCoefficient = 0.7f;

    public override void OnNetworkSpawn() {
        if (!IsOwner) return;
        var spawn = GameObject.Find("Spawn");
        //if (spawn != null) UpdatePositionServerRpc(OwnerClientId, spawn.transform.position);
        if (spawn != null) transform.position = spawn.transform.position;
        if (follow || lookAt) {
            camera = GameObject.Find("Cinemachine").GetComponent<CinemachineVirtualCamera>();
            if (camera == null) Debug.LogError("Can't find a GameObject with the tag 'MainCamera'");
            else {
                if (follow) camera.Follow = transform;
                if (lookAt) camera.LookAt = transform;
            }
        }
        input = GetComponent<VehicleInputHandler>();
        movement = new VehicleMovement(vehicleSO, input, transform);
    }

    void Update() {
        if (!IsOwner) return;
        //if (movement is null) return;
        transform.position = movement.GetMove(Time.deltaTime, RoadFrictionCoefficient);
        UpdateWheels();
    }

    void OnDrawGizmos() {
        if (drawGizmos) {
            if (movement.IsTurning()) {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(movement.CircleMovementCenter(), Math.Abs(movement.TurnRadius));
                if (drawMininumTurnRadius) {
                    Vector3 minCircleCenter = transform.position + (transform.right * (movement.TurnRadius < 0 ? -1 : 1) * vehicleSO.MinTurnRadius);
                    Gizmos.DrawWireSphere(minCircleCenter, vehicleSO.MinTurnRadius);
                }
                return;
            }
            Gizmos.DrawLine(transform.position, transform.position + (transform.forward * 100f));
        }
    }

    void UpdateWheels() {
        // no wheels fr fr. no cap
    }

    public void OnCollisionEnter(Collision collision) {
        Debug.Log(collision);
    }

    public void HandleTerrainChange(TerrainSO newTerrain) {
        Debug.Log(newTerrain);
    }
}
