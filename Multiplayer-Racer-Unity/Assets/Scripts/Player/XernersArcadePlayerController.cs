using System;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using Unity.Netcode.Components;

[RequireComponent(typeof(NetworkTransform))]
public class XernersArcadePlayerController : NetworkBehaviour
{
    new CinemachineVirtualCamera camera;
    [Header("Camera")]
    [SerializeField] bool follow = false;
    [SerializeField] bool lookAt = false;

    [Header("Movement")]
    [SerializeField] float maxVelocity = 10f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float breakDeceleration = 10f;
    [SerializeField] float minTurnRadius = 1f;
    float forwardVelocity = 0f;
    float currentAcceleration = 0f;
    float currentTurnRadius = 0f;

    [Header("Gizmos")]
    [SerializeField] bool drawGizmos = false;
    [SerializeField] bool drawMininumTurnRadius = false;

    // Gizmo variables
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
                    Vector3 minCircleCenter = transform.position + (transform.right * (currentTurnRadius < 0 ? -1 : 1) * minTurnRadius);
                    Gizmos.DrawWireSphere(minCircleCenter, minTurnRadius);
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
        //if (Input.GetKeyDown(KeyCode.W)) {
        UpdateRotation();
        transform.position = newPosition;
        //}
        void UpdateRotation() {
            // I may be wrong here, but I don't think the car should update its x or z rotation in its own control method (unless its a plane-car)
            // If the cars yaw is changing, it should be from outside sources
            Vector3 normalizedPosition = transform.position - movementCircleCenter;
            Vector3 normalizedNewPosition = newPosition - movementCircleCenter;
            float yAxisRotation = Vector2.Angle(new Vector2(normalizedNewPosition.x, normalizedNewPosition.z), new Vector2(normalizedPosition.x, normalizedPosition.z));
            Vector3 rotateBy = new Vector3(transform.rotation.x, yAxisRotation, transform.rotation.z);
            rotateBy = rotateBy * (isTurningRight() ? 1 : -1);
            transform.Rotate(rotateBy);
            //Vector3 normalizedDisplacement = (newPosition - transform.position).normalized;
            //if (isMovingBackwards(normalizedDisplacement)) {
            //    normalizedDisplacement = -normalizedDisplacement;
            //}
            //transform.LookAt(transform.position + normalizedDisplacement, Vector3.up);

            //bool isMovingBackwards(Vector3 normalizedDisplacement) {
            //    return Vector3.Dot(normalizedDisplacement, transform.forward) < 0f;
            //}
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
        return (minTurnRadius / horizontalInput) * (forwardVelocity / maxVelocity) * 5f;
    }

    void UpdateWheels() {
        // fr fr no wheels. no cap
    }

    void HandleSteering() {
        currentTurnRadius = GetTurnRadius();
    }

    void HandleMotor() {
        currentAcceleration = verticalInput * acceleration;
        forwardVelocity = Math.Clamp(forwardVelocity + (currentAcceleration * Time.deltaTime), -maxVelocity, maxVelocity);
        if (isBreaking) {
            ApplyBreaking();
        }
    }

    public virtual void ApplyBreaking() {
        forwardVelocity -= verticalInput * breakDeceleration * Time.deltaTime;
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
