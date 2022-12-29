using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCollision
{
    public const float IS_GROUNDED_RAYCAST_DISTANCE = 0.1f;
    public const float IS_GROUNDED_RAYCAST_OFFSET = 0.05f;

    public static bool IsGrounded(Transform transform, BoxCollider collider, float distance = IS_GROUNDED_RAYCAST_DISTANCE, float offset = IS_GROUNDED_RAYCAST_OFFSET, Vector3? localUp = null) {
        localUp ??= Vector3.up;
        return VehicleRaycasts.GroundedRaycast(transform, collider, distance, offset, localUp).collider is not null;
    }
}
