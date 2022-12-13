using System;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using Unity.Netcode.Components;

[RequireComponent(typeof(NetworkTransform))]
public class XernersArcadePlayerController : NetworkBehaviour
{
    [Header("Vehicle")]
    [Vehicle]
    [SerializeField] VehicleSO SO;

    float forwardVelocity = 0f;
    float currentAcceleration = 0f;
    float currentTurnRadius = 0f;

    new CinemachineVirtualCamera camera;
    [Header("Camera")]
    [SerializeField] bool follow = false;
    [SerializeField] bool lookAt = false;

    [Header("Gizmos")]
    [SerializeField] bool drawGizmos = false;
    [SerializeField] bool drawMininumTurnRadius = false;

    // Variables used in Gizmos
    Vector3 newPosition;
    Vector3 movementCircleCenter;

    // Input related
    float horizontalInput = 0f;
    float verticalInput = 0f;
    bool isBreaking = false;

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
    }

    void Update() {
        if (!IsOwner) return;
        GetInput();
        HandleMovement();
        UpdateWheels();
    }

    void OnDrawGizmos() {
        if (drawGizmos) {
            if (isTurning()) {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(movementCircleCenter, newPosition);
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(movementCircleCenter, Math.Abs(currentTurnRadius));
                if (drawMininumTurnRadius) {
                    Vector3 minCircleCenter = transform.position + (transform.right * (currentTurnRadius < 0 ? -1 : 1) * SO.MinTurnRadius);
                    Gizmos.DrawWireSphere(minCircleCenter, SO.MinTurnRadius);
                }
                return;
            }
            Gizmos.DrawLine(transform.position, transform.position + (transform.forward * 100f));
        }
    }

    bool isTurning() {
        return currentTurnRadius != 0;
    }

    bool isTurningRight() => currentTurnRadius > 0;

    void GetInput() {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMovement() {
        HandleMotor();
        HandleSteering();

        newPosition = isTurning() ? getCircleMovement() : getStraightMovement();
        //Debug.Log("Moving client " + OwnerClientId.ToString() + " to position " + newPosition.ToString() + ". Turn radius is " + currentTurnRadius.ToString());
        UpdateRotation();
        transform.position = newPosition;
        void UpdateRotation() {
            // I may be wrong here, but I don't think the car should update its x or z rotation in its own control method (unless its a plane-car)
            // If the cars yaw is changing, it should be from outside sources
            Vector3 normalizedPosition = transform.position - movementCircleCenter;
            Vector3 normalizedNewPosition = newPosition - movementCircleCenter;
            float yAxisRotation = Vector2.Angle(new Vector2(normalizedNewPosition.x, normalizedNewPosition.z), new Vector2(normalizedPosition.x, normalizedPosition.z));
            Vector3 rotateBy = new Vector3(transform.rotation.x, yAxisRotation, transform.rotation.z);
            rotateBy = rotateBy * (isTurningRight() ? 1 : -1);
            transform.Rotate(rotateBy);
        }
    }

    Vector3 getStraightMovement() {
        return transform.position + transform.forward * getDisplacement(forwardVelocity);
    }

    float getDisplacement(float velocity) {
        return (velocity * Time.deltaTime) + (0.5f) * (currentAcceleration * ((float)Math.Pow(Time.deltaTime, 2)));
    }

    Vector3 getCircleMovement() {
        movementCircleCenter = transform.position + (transform.right * currentTurnRadius);
        float angleToMoveBy = (forwardVelocity / currentTurnRadius) * Time.deltaTime * Mathf.Rad2Deg;
        Vector3 newPositionOnCircle = Quaternion.AngleAxis(angleToMoveBy, Vector3.up) * -transform.right * currentTurnRadius;
        return newPositionOnCircle + movementCircleCenter;
    }

    protected virtual float GetTurnRadius() {
        // The less the player pushes on the horizontal controls, the wider the turn radius
        if (forwardVelocity == 0 || horizontalInput == 0)
            return 0f;

        return CalculateTurnRadius();
    }

    protected virtual float CalculateTurnRadius() {
        return (SO.MinTurnRadius / horizontalInput) * (forwardVelocity / SO.MaxVelocity) * 5f;
    }

    void UpdateWheels() {
        // fr fr no wheels. no cap
    }

    void HandleSteering() {
        currentTurnRadius = GetTurnRadius();
    }

    void HandleMotor() {
        currentAcceleration = verticalInput * SO.Acceleration;
        forwardVelocity = Math.Clamp(forwardVelocity + (currentAcceleration * Time.deltaTime), -SO.MaxVelocity, SO.MaxVelocity);
        if (isBreaking) {
            ApplyBreaking();
        }
    }

    public virtual void ApplyBreaking() {
        forwardVelocity -= verticalInput * SO.BreakDeceleration * Time.deltaTime;
    }

    //[ServerRpc(RequireOwnership = false)]
    //void UpdatePositionServerRpc(ulong clientId, Vector3 pos) {
    //    Debug.Log("Moving client " + OwnerClientId.ToString() + " to position " + pos.ToString());
    //    NetworkManager.ConnectedClients[clientId].PlayerObject.transform.position = pos;
    //}

    public void OnCollisionEnter(Collision collision) {
        Debug.Log(collision);
    }
}
