using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleRaycasts 
{
    /// <summary>Assumes the cars front is facing in the Z direction</summary>
    public static Ray FrontCenterRay(Transform transform, BoxCollider collider, Vector3 direction, Vector3? localForwards = null) {
        localForwards ??= Vector3.forward;
        Ray ray = new Ray(transform.TransformPoint(collider.center + Vector3.Scale(collider.size, localForwards.Value)), direction);
        return ray;
    }

    public static HeadlightRays HeadlightRays(Transform transform, BoxCollider collider, Vector3 direction, Vector3? localForwards = null) {
        localForwards ??= Vector3.forward;
        Vector3 localLeft = Quaternion.Euler(new Vector3(0f, -90f, 0f)) * localForwards.Value;
        Vector3 localRight = Quaternion.Euler(new Vector3(0f, 90f, 0f)) * localForwards.Value;
        Ray left = new Ray(transform.TransformPoint(collider.center + Vector3.Scale(collider.size / 2, localForwards.Value + localLeft)), direction);
        Ray right = new Ray(transform.TransformPoint(collider.center + Vector3.Scale(collider.size / 2, localForwards.Value + localRight)), direction);
        return new HeadlightRays(left, right);
    }

    public static Ray CenterRay(Transform transform, BoxCollider collider, Vector3 direction) {
        Ray ray = new Ray(transform.TransformPoint(collider.center), direction);
        return ray;
    }

    public static Ray GroundedRay(Transform transform, BoxCollider collider, float offset = 0f, Vector3? localUp = null) {
        localUp ??= Vector3.up;
        return new Ray(transform.TransformPoint(collider.center + Vector3.Scale(collider.size / 2, -localUp.Value) + (localUp.Value * offset)), -transform.up); ;
    }

    public static RaycastHit GroundedRaycast(Transform transform, BoxCollider collider, float distance = VehicleCollision.IS_GROUNDED_RAYCAST_DISTANCE, float offset = VehicleCollision.IS_GROUNDED_RAYCAST_OFFSET, Vector3? localUp = null) {
        localUp ??= Vector3.up;
        Physics.Raycast(GroundedRay(transform, collider, offset, localUp), out RaycastHit raycastHit, distance);
        return raycastHit;
    }

    /// <summary>Assumes the cars front is facing in the Z direction</summary>
    public static RaycastHit FrontCenterRaycast(Transform transform, BoxCollider collider, Vector3 direction, float distance, Vector3? localForwards = null) {
        localForwards ??= Vector3.forward;
        Physics.Raycast(FrontCenterRay(transform, collider, direction, localForwards), out RaycastHit raycasthit, distance);
        return raycasthit;
    }
}

public class HeadlightRays {
    public Ray Left;
    public Ray Right;

    public HeadlightRays(Ray left, Ray right) {
        Left = left;
        Right = right;
    }
}
