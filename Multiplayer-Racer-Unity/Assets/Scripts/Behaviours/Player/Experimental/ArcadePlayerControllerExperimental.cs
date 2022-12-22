using System;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using Unity.Netcode.Components;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ArcadePlayerControllerExperimental : NetworkBehaviour
{
    // TODO: change vehicle accel, velocity, and force calculations to be Vector3. not floats

    [Header("Vehicle")]
    [Foldout][SerializeField] VehicleSO vehicleSO;

    [Space]
    [Header("Camera")]
    [SerializeField] bool follow = false;
    [SerializeField] bool lookAt = false;
    new CinemachineVirtualCamera camera;

    [Header("Gizmos")]
    [SerializeField] bool drawGizmos = false;
    [SerializeField] bool drawMininumTurnRadius = false;

    [Header("Cheats")]
    [SerializeField] bool NoRollingFriction;
    [SerializeField] bool NoMaxSpeed;

    // Input
    VehicleInputHandler input;
    VehicleMovementRigidbody movement;

    Rigidbody rb;

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
        rb = GetComponent<Rigidbody>();
        movement = new VehicleMovementRigidbody(vehicleSO, input, transform, rb);
        movement.NoRollingFriction = NoRollingFriction;
        movement.NoMaxSpeed = NoMaxSpeed;
    }

    void FixedUpdate() {
        if (!IsOwner) return;
        if (rb is null || GetComponent<Collider>() is null || movement is null) return;

        movement.Move(Time.deltaTime, RoadFrictionCoefficient);
    }

    void Update() {
        if (!IsOwner) return;
        //if (movement is null) return;
        //transform.position = movement.GetMove(Time.deltaTime, RoadFrictionCoefficient);
        UpdateWheels();
    }

    void OnDrawGizmos() {
        if (drawGizmos && movement != null) {
            // Grounded checker
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position - (transform.up * 0.5f));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + ( transform.forward * 1f));
            // Direction of Movement
            if (movement.IsTurning()) {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(movement.circularMovement, movement.circularMovement + (transform.up * 1f));
                Gizmos.DrawLine(transform.position, transform.position + ( transform.forward * 1f));
                Gizmos.DrawLine(transform.position, movement.CircleMovementCenter());
                Gizmos.DrawWireSphere(movement.CircleMovementCenter(), Math.Abs(movement.TurnRadius));
                if (drawMininumTurnRadius) {
                    Vector3 minCircleCenter = transform.position + (transform.right * (movement.TurnRadius < 0 ? -1 : 1) * vehicleSO.MinTurnRadius);
                    Gizmos.DrawWireSphere(minCircleCenter, vehicleSO.MinTurnRadius);
                }
                return;
            }
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + (transform.forward * 100f));
        }
    }

    void UpdateWheels() {
        // no wheels fr fr. no cap
    }

    public void OnCollisionEnter(Collision collision) {
        //Debug.Log(collision.gameObject.name);
    }

    public void HandleTerrainChange(TerrainSO newTerrain) {
        Debug.Log(newTerrain);
    }
}
