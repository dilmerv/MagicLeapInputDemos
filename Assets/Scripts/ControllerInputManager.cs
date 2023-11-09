using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Logger = LearnXR.Core.Logger;

public class ControllerInputManager : MonoBehaviour
{
    private MagicLeapInputs magicLeapInputs;
    private MagicLeapInputs.ControllerActions controllerActions;

    private GameObject controllerArea;
    
    void Start()
    {
        magicLeapInputs = new MagicLeapInputs();
        magicLeapInputs.Enable();
        controllerActions = new MagicLeapInputs.ControllerActions(magicLeapInputs);
        
        // event type input actions started, performed, cancelled
        controllerActions.Trigger.started += TriggerStarted;
        controllerActions.Trigger.performed += TriggerPerformed;
        controllerActions.Trigger.canceled += TriggerCanceled;

        controllerActions.Bumper.performed += BumperPerformed;
        controllerActions.Bumper.canceled += BumperCanceled;

        controllerArea = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        controllerArea.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

    private void Update()
    {
        if (controllerActions.IsTracked.IsPressed())
        {
            controllerArea.transform.position = controllerActions.Position.ReadValue<Vector3>();
        }
    }

    // Trigger
    private void TriggerStarted(InputAction.CallbackContext obj)
    {
        var triggerValue = obj.ReadValue<float>();
        Logger.Instance.LogInfo($"Trigger Started {triggerValue}");
    }
    
    private void TriggerCanceled(InputAction.CallbackContext obj)
    {
        var triggerValue = obj.ReadValue<float>();
        Logger.Instance.LogInfo($"Trigger Canceled {triggerValue}");
    }
    
    private void TriggerPerformed(InputAction.CallbackContext obj)
    {
        var triggerValue = obj.ReadValue<float>();
        Logger.Instance.LogInfo($"Trigger Performed {triggerValue}");
    }
    
    // Bumper
    private void BumperPerformed(InputAction.CallbackContext obj)
    {
        Logger.Instance.LogInfo($"Bumper Performed");
    }
    
    private void BumperCanceled(InputAction.CallbackContext obj)
    {
        Logger.Instance.LogInfo($"Bumper Canceled");
    }
}
