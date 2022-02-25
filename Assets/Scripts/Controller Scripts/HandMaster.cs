using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

public class HandMaster : MonoBehaviour
{

    public GameObject leftHand, rightHand;

    void Start()
    {
        
    }

    void Update()
    {

        handCheck(leftHand, Handedness.Left);
        handCheck(rightHand, Handedness.Right);
        
    }

    void handCheck(GameObject hand, Handedness side)
    {

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, side, out MixedRealityPose pose))
        {

            hand.SetActive(true);

            hand.transform.GetChild(0).position = pose.Position;
            hand.transform.GetChild(0).rotation = pose.Rotation;

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleMiddleJoint, side, out MixedRealityPose pose2))
            {

                hand.transform.GetChild(1).position = pose2.Position;
                hand.transform.GetChild(1).rotation = pose2.Rotation;

                if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximalJoint, side, out MixedRealityPose pose3))
                {

                    hand.transform.GetChild(2).position = pose3.Position;
                    hand.transform.GetChild(2).rotation = pose3.Rotation;

                }

            }

        }
        else { hand.SetActive(false); }

    }

}
