using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;

public class MovementData {
    public List<string> Timestamps = new List<string>();
    public List<float> RecordingTimeSeconds = new List<float>();
    public List<Vector3> CenterEyePositions = new List<Vector3>();
    public List<Vector3> CenterEyeOrientations = new List<Vector3>();
    public List<Vector3> LeftHandPositions = new List<Vector3>();
    public List<Vector3> RightHandPositions = new List<Vector3>();
    public List<Vector3> LeftHandOrientations = new List<Vector3>();
    public List<Vector3> RightHandOrientations = new List<Vector3>();
    public List<float> LeftHandScale = new List<float>();
    public List<float> RightHandScale = new List<float>();
    public List<string> LeftHandSkeletonData = new List<string>();
    public List<string> RightHandSkeletonData = new List<string>();
    public List<bool> LeftHandTracked = new List<bool>();
    public List<bool> RightHandTracked = new List<bool>();
    public List<OVRHand.TrackingConfidence> LeftHandTrackingConfidence = new List<OVRHand.TrackingConfidence>();
    public List<OVRHand.TrackingConfidence> RightHandTrackingConfidence = new List<OVRHand.TrackingConfidence>();
    public List<Vector3> LeftBoxPositions = new List<Vector3>();
    public List<Vector3> RightBoxPositions = new List<Vector3>();
    public List<Vector3> LeftBoxOrientations = new List<Vector3>();
    public List<Vector3> RightBoxOrientations = new List<Vector3>();
    public List<bool> LeftBoxActive = new List<bool>();
    public List<bool> RightBoxActive = new List<bool>();
    public List<bool> LeftBoxHasHandInside = new List<bool>();
    public List<bool> RightBoxHasHandInside = new List<bool>();

    private static HashSet<OVRSkeleton.BoneId> bonesToSave = new HashSet<OVRSkeleton.BoneId>() {
        OVRSkeleton.BoneId.Hand_WristRoot,
        OVRSkeleton.BoneId.Hand_ForearmStub,
        OVRSkeleton.BoneId.Hand_Thumb0,
        OVRSkeleton.BoneId.Hand_Thumb1,
        OVRSkeleton.BoneId.Hand_Thumb2,
        OVRSkeleton.BoneId.Hand_Thumb3,
        OVRSkeleton.BoneId.Hand_Index1,
        OVRSkeleton.BoneId.Hand_Index2,
        OVRSkeleton.BoneId.Hand_Index3,
        OVRSkeleton.BoneId.Hand_Middle1,
        OVRSkeleton.BoneId.Hand_Middle2,
        OVRSkeleton.BoneId.Hand_Middle3,
        OVRSkeleton.BoneId.Hand_Ring1,
        OVRSkeleton.BoneId.Hand_Ring2,
        OVRSkeleton.BoneId.Hand_Ring3,
        OVRSkeleton.BoneId.Hand_Pinky0,
        OVRSkeleton.BoneId.Hand_Pinky1,
        OVRSkeleton.BoneId.Hand_Pinky2,
        OVRSkeleton.BoneId.Hand_Pinky3
    };

    public string Compress() {
        var json = JsonUtility.ToJson(this);
        var jsonBytes = Encoding.ASCII.GetBytes(json);

        string compressedData;
        using(MemoryStream ms = new MemoryStream()) {
            using(GZipStream gs = new GZipStream(ms, CompressionMode.Compress)) {
                gs.Write(jsonBytes, 0, jsonBytes.Length);
            }
            compressedData = Convert.ToBase64String(ms.ToArray());
        }

        return compressedData;
    }

    public static string OVRHandToString(IList<OVRBone> skeleton) {
        StringBuilder result = new StringBuilder();
        foreach(var bone in skeleton) {
            if(!bonesToSave.Contains(bone.Id)) {
                continue;
            }
            if(result.Length != 0) {
                result.Append("|");
            }
            result.Append((int)bone.Id);
            result.Append(",");
            result.Append(bone.Transform.localRotation.eulerAngles.x);
            result.Append(",");
            result.Append(bone.Transform.localRotation.eulerAngles.y);
            result.Append(",");
            result.Append(bone.Transform.localRotation.eulerAngles.z);
        }
        return result.ToString();
    }

    private float GetHandTrackingConfidence(List<bool> box, List<bool> hand, List<OVRHand.TrackingConfidence> tracking) {
        float tracked = 0f;
        float confidence = 0f;
        int count = 0;

        for(int i = 0; i < box.Count; ++i) {
            if(box[i] == true) {
                tracked += hand[i] ? 1f : 0f;
                confidence += tracking[i] == OVRHand.TrackingConfidence.High ? 1f : 0f;
                count++;
            }
        }

        tracked /= count;
        if(tracked < 0.6f) {
            return 0f;
        }

        confidence /= count;
        return confidence;
    }

    public float TrackingConfidence {
        get {
            var left = GetHandTrackingConfidence(LeftBoxActive, LeftHandTracked, LeftHandTrackingConfidence);
            var right = GetHandTrackingConfidence(RightBoxActive, RightHandTracked, RightHandTrackingConfidence);
            if(left == 0f || right == 0f) {
                return 0f; // if tracking dropped more than 40% of the time with either hand, confidence is just zero
            }

            // Otherwise, confidence is based on the lowest average tracking confidence per the quest
            return Math.Min(left, right);
        }
    }
}