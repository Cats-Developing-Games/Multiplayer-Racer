using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleInput
{
    static float keyboardSteeringValue = 0f;
    public const float KeyboardTimeToMaxTurn = 1f;

    /// <summary>
    /// Mimics how Input.GetAxis() worked
    /// <br/><br/>
    /// <see href="https://docs.unity3d.com/ScriptReference/Input.GetAxis.html"/>
    /// </summary>
    /// <param name="horizontalInput">The input value from an input device</param>
    /// <returns>The new direction of the steering wheel</returns>
    public static float KeyboardSteering(float horizontalInput) {
        float keyboardSteeringStep = Time.deltaTime / KeyboardTimeToMaxTurn;
        switch (horizontalInput) {
            case 1:
                keyboardSteeringValue += keyboardSteeringStep;
                break;
            case -1:
                keyboardSteeringValue += -keyboardSteeringStep;
                break;
            case 0:
                // corner case where adding/subtracting keyboardSteering will cause currentSteering to 
                // flip back and forth around 0
                if (keyboardSteeringValue > -keyboardSteeringStep && keyboardSteeringValue < keyboardSteeringStep) {
                    keyboardSteeringValue = 0f;
                } else if (keyboardSteeringValue > 0) {
                    keyboardSteeringValue += -keyboardSteeringStep;
                } else {
                    keyboardSteeringValue += keyboardSteeringStep;
                }
                break;
            default:
                break;
        }
        keyboardSteeringValue = Mathf.Clamp(keyboardSteeringValue, -1f, 1f);
        return keyboardSteeringValue;
    }

    public static float JoystickSteering(float horizontalInput) {
        return horizontalInput;
    }

    public static Func<float, float> GetSteeringMethod(PlayerInput playerInput) {
        string currentControlScheme = playerInput.currentControlScheme;
        if (currentControlScheme == "Keyboard") {
            return KeyboardSteering;
        } else {
            keyboardSteeringValue = 0f;
            return JoystickSteering;
        }
    }
}
