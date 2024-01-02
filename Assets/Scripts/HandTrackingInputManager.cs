using System.Collections.Generic;
using System.Linq;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;
using Logger = LearnXR.Core.Logger;

public class HandTrackingManager : MonoBehaviour
{
    [SerializeField]
    private GameObject handPrefabForKeypoint;
    
    [SerializeField]
    [Range(0,1.0f)]
    private float boneVisibilityConfidence;

    [SerializeField] private bool boneNamesVisibility;

    [SerializeField] private bool verboseHandTrackingLog;
    
    private InputDevice leftHandDevice;
    private InputDevice rightHandDevice;
    
    private List<Bone> leftHandPinkyFingerBones = new ();
    private List<Bone> leftHandRingFingerBones = new ();
    private List<Bone> leftHandMiddleFingerBones = new ();
    private List<Bone> leftHandIndexFingerBones = new ();
    private List<Bone> leftHandThumbBones = new ();
    
    private List<Bone> rightHandPinkyFingerBones = new ();
    private List<Bone> rightHandRingFingerBones = new ();
    private List<Bone> rightHandMiddleFingerBones = new ();
    private List<Bone> rightHandIndexFingerBones = new ();
    private List<Bone> rightHandThumbBones = new ();

    private Dictionary<string, GameObject> boneIndicators = new();


    public class BoneWithName
    {
        public string BoneName { get; set; }
        public Bone Bone { get; set; }
    }
    
    private enum HandSkeletonFor
    {
        LeftHand,
        RightHand
    }
    
    void Start()
    {
        if (MLPermissions.CheckPermission(MLPermission.HandTracking).IsOk)
        {
            Logger.Instance.LogInfo($"MLPermission for hand tracking was auto granted");
            InputSubsystem.Extensions.MLHandTracking.StartTracking();
            
            // Gesture system initialization
            InputSubsystem.Extensions.MLGestureClassification.StartTracking();
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
        if (leftHandDevice.isValid)
        {
            DisplayFeatures($"{leftHandDevice.name}", leftHandDevice);
            BuilHandSimpleSkeleton(HandSkeletonFor.LeftHand);
            DisplayGesturesInfo(HandSkeletonFor.LeftHand);
        }

        if (rightHandDevice.isValid)
        {
            DisplayFeatures($"{leftHandDevice.name}", rightHandDevice);
            BuilHandSimpleSkeleton(HandSkeletonFor.RightHand);
            DisplayGesturesInfo(HandSkeletonFor.RightHand);
        }
    }

    private void DisplayFeatures(string handDeviceName, InputDevice device)
    {
        var features = new List<InputFeatureUsage>();
        if(device.TryGetFeatureUsages(features))
        {
            foreach (var feature in features)
            {
                if(verboseHandTrackingLog)
                    Logger.Instance.LogInfo($"{handDeviceName}: {feature.name}");
            }
        }
    }

    private void DisplayGesturesInfo(HandSkeletonFor handSkeletonFor)
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);
        InputDevice device;
        if (handSkeletonFor == HandSkeletonFor.LeftHand)
        {
            device = devices.SingleOrDefault(d =>
                d.name == InputSubsystem.Extensions.MLGestureClassification.LeftGestureInputDeviceName);
        }
        else
        {
            device = devices.SingleOrDefault(d =>
                d.name == InputSubsystem.Extensions.MLGestureClassification.RightGestureInputDeviceName);
        }
        device.TryGetFeatureValue(InputSubsystem.Extensions.DeviceFeatureUsages.HandGesture.GesturePosture, out uint gestureInt);
        InputSubsystem.Extensions.MLGestureClassification.PostureType festurePosturetype = (InputSubsystem.Extensions.MLGestureClassification.PostureType)gestureInt;
        Logger.Instance.LogInfo($"{handSkeletonFor} gesture: " + festurePosturetype);
        // Mention that you can also get additional finger data including angles with MLGestureClassification API
    }

    private void BuilHandSimpleSkeleton(HandSkeletonFor handSkeletonFor)
    {
        InputDevice device = handSkeletonFor == HandSkeletonFor.LeftHand ? leftHandDevice : rightHandDevice;
        
        device.TryGetFeatureValue(InputSubsystem.Extensions.DeviceFeatureUsages.Hand.Confidence, out float confidence);
        if (confidence >= boneVisibilityConfidence)
        {
            if (device.TryGetFeatureValue(CommonUsages.handData, out Hand hand))
            {
                hand.TryGetFingerBones(HandFinger.Index, handSkeletonFor == HandSkeletonFor.LeftHand ? leftHandIndexFingerBones : rightHandIndexFingerBones);
                hand.TryGetFingerBones(HandFinger.Middle, handSkeletonFor == HandSkeletonFor.LeftHand ? leftHandMiddleFingerBones : rightHandMiddleFingerBones);
                hand.TryGetFingerBones(HandFinger.Ring, handSkeletonFor == HandSkeletonFor.LeftHand ? leftHandRingFingerBones : rightHandRingFingerBones);
                hand.TryGetFingerBones(HandFinger.Pinky, handSkeletonFor == HandSkeletonFor.LeftHand ? leftHandPinkyFingerBones : rightHandPinkyFingerBones);
                hand.TryGetFingerBones(HandFinger.Thumb, handSkeletonFor == HandSkeletonFor.LeftHand ? leftHandThumbBones : rightHandThumbBones);

                var combinedBones = CombineBones(handSkeletonFor);
                BuildHandVisualizer(handSkeletonFor, combinedBones);
            }
        }
        else
        {
            Logger.Instance.LogInfo($"Hand Input device confidence is not sufficient to be shown");
        }
    }

    private List<BoneWithName> CombineBones(HandSkeletonFor handSkeletonFor)
    {
        List<BoneWithName> handBones;
        if (handSkeletonFor == HandSkeletonFor.LeftHand)
        {
            handBones = leftHandThumbBones.GenerateBonesWithNames(InputSubsystem.Extensions.MLHandTracking.KeyPointLocation.Thumb)
                .Concat(leftHandIndexFingerBones.GenerateBonesWithNames(InputSubsystem.Extensions.MLHandTracking.KeyPointLocation.Index))
                .Concat(leftHandMiddleFingerBones.GenerateBonesWithNames(InputSubsystem.Extensions.MLHandTracking.KeyPointLocation.Middle))
                .Concat(leftHandRingFingerBones.GenerateBonesWithNames(InputSubsystem.Extensions.MLHandTracking.KeyPointLocation.Ring))
                .Concat(leftHandPinkyFingerBones.GenerateBonesWithNames(InputSubsystem.Extensions.MLHandTracking.KeyPointLocation.Pinky))
                .ToList();
        }
        else
        {
            handBones = rightHandThumbBones.GenerateBonesWithNames(InputSubsystem.Extensions.MLHandTracking.KeyPointLocation.Thumb)
                .Concat(rightHandIndexFingerBones.GenerateBonesWithNames(InputSubsystem.Extensions.MLHandTracking.KeyPointLocation.Index))
                .Concat(rightHandMiddleFingerBones.GenerateBonesWithNames(InputSubsystem.Extensions.MLHandTracking.KeyPointLocation.Middle))
                .Concat(rightHandRingFingerBones.GenerateBonesWithNames(InputSubsystem.Extensions.MLHandTracking.KeyPointLocation.Ring))
                .Concat(rightHandPinkyFingerBones.GenerateBonesWithNames(InputSubsystem.Extensions.MLHandTracking.KeyPointLocation.Pinky))
                .ToList();
        }

        return handBones;
    }

    private void BuildHandVisualizer(HandSkeletonFor handSkeletonFor, List<BoneWithName> bones)
    {
        int boneId = 0;
        foreach (var boneInfo in bones)
        {
            var boneKeyName = $"{handSkeletonFor}_{boneInfo}_{boneId}";
            boneInfo.Bone.TryGetPosition(out Vector3 bonePosition);
            boneInfo.Bone.TryGetRotation(out Quaternion boneRotation);
            GameObject boneVisualizer = null;
            
            if (!boneIndicators.ContainsKey(boneKeyName))
            {
                boneVisualizer = Instantiate(handPrefabForKeypoint, transform, true);
                boneIndicators.Add(boneKeyName, boneVisualizer);    
            }
            
            boneVisualizer = boneIndicators[boneKeyName];
            var boneVisualizerInfo = boneVisualizer.GetComponentInChildren<TextMeshPro>(true);
            boneVisualizerInfo.text = boneInfo.BoneName;
            boneVisualizerInfo.gameObject.SetActive(boneNamesVisibility);
            boneVisualizer.transform.localPosition = bonePosition;
            boneVisualizer.transform.localRotation = boneRotation;
            boneId++;
        }
    }
}
