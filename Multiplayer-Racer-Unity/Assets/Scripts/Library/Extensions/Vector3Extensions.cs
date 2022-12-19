using UnityEngine;

public static class Vector3Exensions {
    public static bool IsBasicallyEqualTo(this Vector3 value, Vector3 other) => Vector3.Distance(value, other).IsBasicallyZero();

    public static Vector3 RoundIfBasicallyZero(this Vector3 value) {
        if (value.x.IsBasicallyZero())
            value = new Vector3(0f, value.y, value.z);
        if (value.y.IsBasicallyZero())
            value = new Vector3(value.x, 0f, value.z);
        if (value.z.IsBasicallyZero())
            value = new Vector3(value.x, value.y, 0f);
        return value;
    }
}