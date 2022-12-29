using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class VehicleCollisionBehaviour : MonoBehaviour
{
    [SerializeField] new BoxCollider collider;
    [SerializeField] Rigidbody rb;
    [Header("Groud Checking")]
    [SerializeField] float groundCheckRayDistance = VehicleCollision.IS_GROUNDED_RAYCAST_DISTANCE;
    [SerializeField] float groundCheckRayOffset = VehicleCollision.IS_GROUNDED_RAYCAST_OFFSET;

    [Space()]
    public bool DrawGizmos;

    public UnityEvent<bool> OnGroundedChanged;

    // raycasts
    Ray groundedRay;

    bool isGrounded = false;

    public bool IsGrounded { get => isGrounded; }
    public BoxCollider Collider { get => collider; }
    public Rigidbody Rigidbody { get => rb; }

    private void OnDrawGizmos() {
        if (!DrawGizmos) return;
        Gizmos.color = Color.red;
        groundedRay = VehicleRaycasts.GroundedRay(transform, collider, groundCheckRayOffset);
        Gizmos.DrawLine(groundedRay.origin, groundedRay.origin + (groundedRay.direction * groundCheckRayDistance));
    }

    void Start()
    {
        collider = GetComponent<BoxCollider>();
        if (rb is null) rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isGrounded != VehicleCollision.IsGrounded(transform, Collider, groundCheckRayDistance, groundCheckRayOffset)) {
            isGrounded = !isGrounded;
            OnGroundedChanged.Invoke(isGrounded);
        }
    }
}
