using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.InteractionSubsystems;
using UnityEngine.XR.MagicLeap;
using Logger = LearnXR.Core.Logger;

public class ControllerInputManager : MonoBehaviour
{
    [SerializeField] private Material controllerAreaMaterial;
    [SerializeField] private Vector3 controllerPositionOffset;
    [SerializeField] private float displayControllerInfoFrequency = 0.5f;
    
    private MagicLeapInputs magicLeapInputs;
    private MagicLeapInputs.ControllerActions controllerActions;

    private GameObject controllerArea;
    private Color lastGeneratedRandomColor;
    private Vector3 lastControllerKnownPosition;
    private Quaternion lastControllerKnownRotation;
    
    private GestureSubsystem.Extensions.TouchpadGestureEvent gestureEvent;
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

        // controller position
        controllerArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        controllerArea.GetComponent<Renderer>().material = controllerAreaMaterial;
        controllerArea.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        StartCoroutine(DisplayControllerActions());
        
        // gestures
        // Add this after the 2nd build
        MLDevice.RegisterGestureSubsystem();

        if (MLDevice.GestureSubsystemComponent != null)
        {
            MLDevice.GestureSubsystemComponent.onTouchpadGestureChanged += GestureDetected;
        }
    }

    private void GestureDetected(GestureSubsystem.Extensions.TouchpadGestureEvent gestureEvent)
    {
        this.gestureEvent = gestureEvent;
        Debug.Log($"New gesture detected: {gestureEvent}");
        
        // Swipe up gesture
        if (this.gestureEvent is { type: InputSubsystem.Extensions.TouchpadGesture.Type.Swipe, 
                direction: InputSubsystem.Extensions.TouchpadGesture.Direction.Up, state: GestureState.Completed })
        {
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            
            // position ball initially at controller pos + an offset
            ball.transform.position = lastControllerKnownPosition + new Vector3(0, 0.1f, 0.1f);
            ball.transform.rotation = lastControllerKnownRotation;
            
            var ballRenderer = ball.GetComponent<Renderer>();
            ballRenderer.material = controllerAreaMaterial;
            ballRenderer.material.color = lastGeneratedRandomColor;
            var ballRigidbody = ball.AddComponent<Rigidbody>();
            ballRigidbody.AddForce(Vector3.forward * 350.0f);
            
            Destroy(ball, 5.0f);
        }
    }

    // Helpful input reference https://developer-docs.magicleap.cloud/docs/guides/unity/input/controller/unity-controller-api-overview/
    private IEnumerator DisplayControllerActions()
    {
        while (true)
        {
            yield return new WaitForSeconds(displayControllerInfoFrequency);
            
            if (controllerActions.IsTracked.IsPressed())
            {
                // reading the values from InputActions
                var controllerPosition = controllerActions.Position.ReadValue<Vector3>();
                var controllerRotation = controllerActions.Rotation.ReadValue<Quaternion>();
                
                Logger.Instance.LogInfo($"Controller Position: {controllerPosition}");
                Logger.Instance.LogInfo($"Controller Rotation: {controllerRotation}");
            
                Logger.Instance.LogInfo($"Controller Bumper Action: {controllerActions.Bumper.inProgress}");
                Logger.Instance.LogInfo($"Controller Trigger Action: {controllerActions.Trigger.inProgress}");
                
                // Do this after 2nd build
                Logger.Instance.LogInfo($"Controller Acceleration: {controllerActions.Acceleration.ReadValue<Vector3>()}");
                Logger.Instance.LogInfo($"Controller Touchpad Position: {controllerActions.TouchpadPosition.ReadValue<Vector2>()}");
                Logger.Instance.LogInfo($"Controller Touchpad Force: {controllerActions.TouchpadForce.ReadValue<float>()}");
            }
        }
    }

    private void Update()
    {
        if (controllerActions.IsTracked.IsPressed())
        {
            controllerArea.transform.position = controllerActions.Position.ReadValue<Vector3>() + controllerPositionOffset;
            lastControllerKnownPosition = controllerActions.Position.ReadValue<Vector3>();
            
            // Do this after the first deployed and first demo
            // Deploy and quickly show
            controllerArea.transform.rotation = controllerActions.Rotation.ReadValue<Quaternion>();
            lastControllerKnownRotation = controllerActions.Rotation.ReadValue<Quaternion>();
        }
    }

    // Callbacks for subscribed input events
    
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
        lastGeneratedRandomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        controllerArea.GetComponent<Renderer>().material.color = lastGeneratedRandomColor;
        var triggerValue = obj.ReadValue<float>();
        Logger.Instance.LogInfo($"Trigger Performed {triggerValue}");
    }
    
    // Bumper
    private void BumperPerformed(InputAction.CallbackContext obj)
    {
        var randomScale = Random.Range(0.05f, 0.25f);
        controllerArea.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        Logger.Instance.LogInfo($"Bumper Performed");
    }
    
    private void BumperCanceled(InputAction.CallbackContext obj)
    {
        Logger.Instance.LogInfo($"Bumper Canceled");
    }
    
    // Clean Up
    private void OnDestroy()
    {
        controllerActions.Trigger.started -= TriggerStarted;
        controllerActions.Trigger.performed -= TriggerPerformed;
        controllerActions.Trigger.canceled -= TriggerCanceled;

        controllerActions.Bumper.performed -= BumperPerformed;
        controllerActions.Bumper.canceled -= BumperCanceled;
        
        // gesture event removal
        if (MLDevice.GestureSubsystemComponent != null)
        {
            MLDevice.GestureSubsystemComponent.onTouchpadGestureChanged -= GestureDetected;
            MLDevice.UnregisterGestureSubsystem();
        }
    }
}