using System.Buffers;
using UnityEngine;

public class SpinTransform : MonoBehaviour
{
    [SerializeField] private float degreesPerSecond = 10f;

    private bool isRotating = true;

    // Update is called once per frame
    void Update()
    {
        if (isRotating) RotateTransform();
    }

    private void RotateTransform()
    {
        var deltaAngle = degreesPerSecond * Time.deltaTime;
        transform.rotation *= Quaternion.AngleAxis(deltaAngle, Vector3.up);
    }
}
