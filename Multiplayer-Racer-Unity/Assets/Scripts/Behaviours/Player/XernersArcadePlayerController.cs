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
    [Vehicle][SerializeField] VehicleSO SO;
    float forwardVelocity = 0f;
    float currentAcceleration = 0f;
    float currentTurnRadius = 0f;
    float currentSteering = 0f;
    Func<float> steeringMethod;
    [Space]
    [SerializeField] float roadFrictionCoefficient = 0.7f;

    [Header("Camera")]
    [SerializeField] bool follow = false;
    [SerializeField] bool lookAt = false;
    new CinemachineVirtualCamera camera;

    [Header("Gizmos")]
    [SerializeField] bool drawGizmos = false;
    [SerializeField] bool drawMininumTurnRadius = false;

    // Modifiers
    private List<float> accelerationModifiers;

    // Variables used in Gizmos
    Vector3 newPosition;
    Vector3 movementCircleCenter;

    // Input related
    float horizontalInput = 0f;
    float verticalInput = 0f;
    bool isBreaking = false;
    string currentControlScheme;

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
        currentControlScheme = GetComponent<PlayerInput>().currentControlScheme;
    }

    void Update() {
        if (!IsOwner) return;
        HandleMovement();
        updateWheels();
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

    void updateWheels() {
        // no wheels fr fr. no cap
    }

    #region Getters and Setters

    public void AddAccelerationModifier(float modifier) {
        accelerationModifiers.Add(modifier);
    }

    /// <returns>The index of the modifier</returns>
    public void RemoveAccelerationModifier(float modifier) {
        if (accelerationModifiers.Contains(modifier))
            accelerationModifiers.Remove(modifier);
    }

    public void SetFrictionCoefficient(float coefficient) {
        roadFrictionCoefficient = coefficient;
    }

    #endregion

    #region Updating Position and Rotation

    private void HandleMovement() {
        handleMotor();
        handleSteering();
        
        newPosition = isTurning() ? calcCircleMovement() : calcStraightMovement();
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

    Vector3 calcStraightMovement() {
        return transform.position + transform.forward * KineticPhysics.Displacement(forwardVelocity, currentAcceleration, Time.deltaTime);
    }

    Vector3 calcCircleMovement() {
        movementCircleCenter = transform.position + (transform.right * currentTurnRadius);
        float angleToMoveBy = (forwardVelocity / currentTurnRadius) * Time.deltaTime * Mathf.Rad2Deg;
        Vector3 newPositionOnCircle = Quaternion.AngleAxis(angleToMoveBy, Vector3.up) * -transform.right * currentTurnRadius;
        return newPositionOnCircle + movementCircleCenter;
    }

    #endregion

    #region Steering

    bool isTurning() => currentTurnRadius != 0;

    bool isTurningRight() => currentTurnRadius > 0;

    void handleSteering() {
        currentSteering = steeringMethod.Invoke();
        currentSteering *= calcSteeringModifier();
        currentTurnRadius = calcTurnRadius();
    }

    float calcTurnRadius() {
        if (forwardVelocity == 0 || currentSteering == 0)
            return 0f;
        else
            return (SO.MinTurnRadius / currentSteering) * (forwardVelocity / SO.MaxVelocity) * 5f;
    }

    float calcSteeringModifier() {
        return SO.SteeringModifier;
    }

    float calcKeyboardSteering() {
        float keyboardSteering = VehicleDefaults.KeyboardTimeToMaxTurn * Time.deltaTime;
        switch (horizontalInput) {
            case 1:
                currentSteering += keyboardSteering;
                break;
            case -1:
                currentSteering += -keyboardSteering;
                break;
            case 0:
                // corner case where adding/subtracting keyboardSteering will cause currentSteering to 
                // flip back and forth around 0
                if (currentSteering > -keyboardSteering && currentSteering < keyboardSteering) {
                    currentSteering = 0f;
                } else if (currentSteering > 0) {
                    currentSteering += -keyboardSteering;
                } else {
                    currentSteering += keyboardSteering;
                }
                break;
            default:
                break;
        }
        return Mathf.Clamp(currentSteering, -1f, 1f);
    }

    float calcJoystickSteering() {
        return horizontalInput;
    }

    #endregion

    #region Kinetic calculations

    void handleMotor() {
        void applyRollingDeceleration() {
            if (!isBreaking && verticalInput == 0 && forwardVelocity != 0) {
                float rollingDecel = forwardVelocity > 0 ? VehicleDefaults.RollingDeceleration : -VehicleDefaults.RollingDeceleration;
                forwardVelocity = calcVelocity(rollingDecel);
            }
        }

        currentAcceleration = calcAcceleration();
        forwardVelocity = calcVelocity(currentAcceleration);
        applyRollingDeceleration();
    }

    float calcMass() {
        return SO.Mass;
    }

    float calcEngineForce() {
        return SO.EngineForce;
    }

    float calcForwardForce() {
        if (isBreaking) {
            return 0f;
        }
        //Debug.Log("Forward Force: " + verticalInput * calcEngineForce());
        return verticalInput * calcEngineForce();
    }

    // TODO: applying traction this way does not feel right, but I am not sure how it should be done
    // Also see VehicleSO.WheelTraction
    float calcFrictionCoefficient() {
        //Debug.Log("Coefficient of friction: " + (roadFrictionCoefficient * calcTraction()).ToString());
        return roadFrictionCoefficient * calcTraction();
    }

    float calcFrictionForce() {
        if (isBreaking && forwardVelocity != 0) {
            return calcBreakForce();
        }
        if (verticalInput == 0) {
            // TODO: rolling friction (natural deceleration) is applied as a constant decel in handleMotor()
            // I'm really not sure how to properly calculate rolling friction
            return 0f;
        }
        return 0f;
    }

    float calcBreakForce() {
        //Debug.Log("Breaking Force: " + (KineticPhysics.ForceOfFriction(calcMass(), transform, calcFrictionCoefficient())).ToString());
        float force = KineticPhysics.ForceOfFriction(calcMass(), transform, calcFrictionCoefficient());
        force = forwardVelocity > 0 ? force : -force;
        return force;
    }

    float calcNetForce() {
        //Debug.Log("Net Force: " + (calcForwardForce() - calcFrictionForce()).ToString());
        return calcForwardForce() - calcFrictionForce();
    }

    float calcAcceleration() {
        //Debug.Log("Acceleration: " + KineticPhysics.Acceleration(calcNetForce(), calcMass()).ToString());
        return KineticPhysics.Acceleration(calcNetForce(), calcMass());
    }

    float calcVelocity(float acceleration) {
        return Mathf.Clamp(KineticPhysics.Velocity(forwardVelocity, acceleration, Time.deltaTime), -SO.MaxVelocity, SO.MaxVelocity);
    }

    // TODO: apply traction to turn radius somehow
    float calcTraction() {
        return SO.WheelTraction;
    }

    #endregion

    //[ServerRpc(RequireOwnership = false)]
    //void UpdatePositionServerRpc(ulong clientId, Vector3 pos) {
    //    Debug.Log("Moving client " + OwnerClientId.ToString() + " to position " + pos.ToString());
    //    NetworkManager.ConnectedClients[clientId].PlayerObject.transform.position = pos;
    //}

    public void OnCollisionEnter(Collision collision) {
        Debug.Log(collision);
    }

    #region Input Messages

    public void OnControlsChanged() {
        currentControlScheme = GetComponent<PlayerInput>().currentControlScheme;
        if (currentControlScheme == "Keyboard") {
            steeringMethod = calcKeyboardSteering;
        } else {
            steeringMethod = calcJoystickSteering;
        }
    }

    public void OnAccelerate(InputValue value) {
        verticalInput = value.Get<float>();
    }

    public void OnSteer(InputValue value) {
        horizontalInput = value.Get<float>();
    }

    public void OnBrake(InputValue value) {
        isBreaking = value.Get<float>() > 0;
    }

    #endregion
}
