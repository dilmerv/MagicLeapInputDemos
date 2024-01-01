using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using Logger = LearnXR.Core.Logger;

public class HandTrackingManager : MonoBehaviour
{
    private InputDevice leftHandDevice;
    private InputDevice rightHandDevice;
    
    void Start()
    {
        if (MLPermissions.CheckPermission(MLPermission.HandTracking).IsOk)
        {
            Logger.Instance.LogInfo($"MLPermission for hand tracking was auto granted");
        }
    }

    private void Update()
    {
        if (!leftHandDevice.isValid || !rightHandDevice.isValid)
        {
            leftHandDevice =
                InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.HandTracking |
                                                         InputDeviceCharacteristics.Left);
            rightHandDevice =
                InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.HandTracking |
                                                         InputDeviceCharacteristics.Right);
        }
        
        // If Valid let's display some info
        if(leftHandDevice.isValid) DisplayFeatures(leftHandDevice);
        if(rightHandDevice.isValid) DisplayFeatures(rightHandDevice);
        
    }

    private void DisplayFeatures(InputDevice device)
    {
        var features = new List<InputFeatureUsage>();
        if(device.TryGetFeatureUsages(features))
        {
            foreach (var feature in features)
            {
                Logger.Instance.LogInfo(feature.name);
            }
        }
    }
}
