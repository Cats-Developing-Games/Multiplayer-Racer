using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleInput
{
    static float lastKeyboardSteerValue = 0f;
    /// <summary>
    /// Mimics how Input.GetAxis() worked
    /// <br/><br/>
    /// <see href="https://docs.unity3d.com/ScriptReference/Input.GetAxis.html"/>
    /// </summary>
    /// <param name="horizontalInput">The input value from an input device</param>
    /// <returns>The new direction of the steering wheel</returns>
    public static float KeyboardSteering(float horizontalInput) {
        float keyboardSteering = VehicleDefaults.KeyboardTimeToMaxTurn * Time.deltaTime;
        switch (horizontalInput) {
            case 1:
                lastKeyboardSteerValue += keyboardSteering;
                break;
            case -1:
                lastKeyboardSteerValue += -keyboardSteering;
                break;
            case 0:
                // corner case where adding/subtracting keyboardSteering will cause currentSteering to 
                // flip back and forth around 0
                if (lastKeyboardSteerValue > -keyboardSteering && lastKeyboardSteerValue < keyboardSteering) {
                    lastKeyboardSteerValue = 0f;
                } else if (lastKeyboardSteerValue > 0) {
                    lastKeyboardSteerValue += -keyboardSteering;
                } else {
                    lastKeyboardSteerValue += keyboardSteering;
                }
                break;
            default:
                break;
        }
        return Mathf.Clamp(lastKeyboardSteerValue, -1f, 1f);
    }

    public static float JoystickSteering(float horizontalInput) {
        return horizontalInput;
    }

    public static Func<float, float> GetSteeringMethod(PlayerInput playerInput) {
        string currentControlScheme = playerInput.currentControlScheme;
        if (currentControlScheme == "Keyboard") {
            return KeyboardSteering;
        } else {
            lastKeyboardSteerValue = 0f;
            return JoystickSteering;
        }
    }
}
