using System;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using Unity.Netcode.Components;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(PlayerInput))]
public class ArcadeCarController : NetworkBehaviour
{
    #region Vars

    [Header("Vehicle")]
    [Foldout][SerializeField] VehicleSO vehicleSO;
    [SerializeField] Transform transformToMove;
    [SerializeField] VehicleCollisionBehaviour collision;
    [SerializeField] CheckpointController checkpointHandler;

    [Header("Environment")]
    // temporary. should be passed from outside this class
    public float RoadFrictionCoefficient = 0.7f;

    [Header("Camera")]
    [SerializeField] bool follow = false;
    [Description("Looks for a GameObject named 'Cinemachine' on network spawn.\nChange the CinemachineVirtualCamera's Body -> Follow Offset for different camera angles")]
    [SerializeField] bool lookAt = false;
    new CinemachineVirtualCamera camera;

    [Header("Gizmos")]
    public bool DrawDrivingDirection = false;
    public bool DrawMininumTurnRadius = false;
    public bool LogTerrainChange = false;
    //public bool DrawDriveableIcon = false;

    [Header("Cheats")]
    public bool NoRollingFriction;
    public bool NoMaxSpeed;

    // Input
    VehicleInputHandler input;
    ArcadeVehicleMovement movement;

    #endregion

    public override void OnNetworkSpawn() {
        if (!IsOwner) return;
        //if (spawn != null) UpdatePositionServerRpc(OwnerClientId, spawn.transform.position);
        NetworkManager.Singleton.SceneManager.OnLoadComplete += MoveToSpawn;
        NetworkManager.Singleton.SceneManager.OnLoadComplete += checkpointHandler.OnLevelStart;
        if (follow || lookAt) {
            GameObject cameraObject = null;
            if (camera is null) cameraObject = GameObject.Find("Cinemachine");
            if (cameraObject is null) {
                Debug.LogError("'Look At' and/or 'Follow' flags set, but can't find an object named 'Cinemachine'");
            } else {
                camera = cameraObject.GetComponent<CinemachineVirtualCamera>();
                if (follow) camera.Follow = transformToMove;
                if (lookAt) camera.LookAt = transformToMove;
            }
        }
        input = GetComponent<VehicleInputHandler>();
        if (transformToMove == null) Debug.LogError("No Transform was specified in " + gameObject.name);
        if (collision == null) Debug.LogError("No VehicleCollisionBehaviour was attached to " + gameObject.name);
        movement = new ArcadeVehicleMovement(vehicleSO, input, transformToMove, collision);
        movement.NoRollingFriction = NoRollingFriction;
        movement.NoMaxSpeed = NoMaxSpeed;
    }
    
    void Update() {
        if (!IsOwner) return;
        if (movement is null) return;
        transformToMove.position = movement.GetMove(Time.deltaTime, RoadFrictionCoefficient);
        UpdateWheels();
    }

    void OnDrawGizmos() {
        if (movement != null) {
            //if (DrawDriveableIcon) {
            //    if (collision.IsGrounded) Gizmos.DrawIcon(transform.position + (1f * transform.up), "steering-wheel.png", true, Color.green);
            //    else Gizmos.DrawIcon(transform.position + (1f * transform.up), "steering-wheel.png", true, Color.red);
            //}
            if (DrawDrivingDirection) {
                if (movement.IsTurning()) {
                    Gizmos.color = Color.gray;
                    Gizmos.DrawLine(transformToMove.position, movement.CircleMovementCenter);
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(movement.CircleMovementCenter, Math.Abs(movement.TurnRadius));
                    if (DrawMininumTurnRadius) {
                        Vector3 minCircleCenter = transformToMove.position + (transformToMove.right * (movement.TurnRadius < 0 ? -1 : 1) * vehicleSO.MinTurnRadius);
                        Gizmos.DrawWireSphere(minCircleCenter, vehicleSO.MinTurnRadius);
                    }
                } else {
                    if (DrawDrivingDirection) Gizmos.DrawLine(transformToMove.position, transformToMove.position + (transformToMove.forward * 100f));
                }
            }
        }
    }

    void UpdateWheels() {
        // no wheels fr fr. no cap
    }

    public void MoveToSpawn(ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode sceneMode) {
        var spawn = GameObject.Find("Spawn");
        if (spawn is null) {
            Debug.LogError("No GameObject named 'Spawn' found");
            return;
        }
        transformToMove.position = spawn.transform.position;
        transformToMove.rotation = spawn.transform.rotation;
    }

    public void HandleTerrainChange(TerrainSO newTerrain) {
        if (LogTerrainChange) Debug.Log(newTerrain);
        movement.VelocityModifier = newTerrain.TerrainEffect.VelocityModifier;
        //RoadFrictionCoefficient = newTerrain.TerrainEffect;
    }
}
