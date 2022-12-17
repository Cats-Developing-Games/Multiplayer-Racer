using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleInputHandler : MonoBehaviour
{
    string currentControlScheme;

    public Func<float, float> SteeringMethod { get; private set; }
    public float VerticalInput { get; private set; }
    public float HorizontalInput { get; private set; }
    public bool IsBreaking { get; private set; }

    void Start() {
        currentControlScheme = GetComponent<PlayerInput>().currentControlScheme;
    }

    public void OnControlsChanged() {
        SteeringMethod = VehicleInput.GetSteeringMethod(GetComponent<PlayerInput>());
    }

    public void OnAccelerate(InputValue value) {
        VerticalInput = value.Get<float>();
    }

    public void OnSteer(InputValue value) {
        HorizontalInput = value.Get<float>();
    }

    public void OnBrake(InputValue value) {
        IsBreaking = value.Get<float>() > 0;
    }
}
