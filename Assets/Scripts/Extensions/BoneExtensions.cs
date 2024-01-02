using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;

namespace Extensions
{
    public static class BoneExtensions
    {
        public static List<BoneWithName> GenerateBonesWithNames(this List<Bone> bones, InputSubsystem.Extensions.MLHandTracking.KeyPointLocation keyPointLocation)
        {
            List<BoneWithName> bonesWithNames = new List<BoneWithName>();
            for(var i = 0; i < bones.Count; i++)
            {
                string boneName = InputSubsystem.Extensions.MLHandTracking.GetKeyPointName(keyPointLocation, i);
                bonesWithNames.Add(new BoneWithName
                {
                    Bone = bones[i],
                    BoneName = boneName
                });
            }

            return bonesWithNames;
        }
    }
}