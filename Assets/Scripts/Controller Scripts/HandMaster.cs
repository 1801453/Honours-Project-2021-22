using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

public class HandMaster : MonoBehaviour
{

    public GameObject leftHand, rightHand;

    float leftResetTimer = 1.1f, rightResetTimer = 1.1f;

    void Start()
    {
        
    }

    void Update()
    {

        handCheck(rightHand, Handedness.Right);
        handCheck(leftHand, Handedness.Left);

    }

    void handCheck(GameObject hand, Handedness side)
    {

        float resetTimer;

        if (hand.name == "Left Hand") { resetTimer = leftResetTimer; }
        else { resetTimer = rightResetTimer; }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, side, out MixedRealityPose pose))
        {

            hand.SetActive(true);

            hand.transform.GetChild(0).position = pose.Position;
            hand.transform.GetChild(0).rotation = pose.Rotation;

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleMiddleJoint, side, out MixedRealityPose pose2))
            {

                Vector3 displacement;
                float distance, dt = Time.deltaTime;

                hand.transform.GetChild(1).position = pose2.Position;
                hand.transform.GetChild(1).rotation = pose.Rotation;
                
                displacement = pose2.Position - pose.Position;
                distance = displacement.magnitude;

                FingerTrigger script = hand.transform.GetChild(1).GetChild(0).gameObject.GetComponent<FingerTrigger>();

                if (distance < 0.7f && resetTimer >= 1) { script.IsHolding(); }
                else
                {

                    script.stopHolding();

                    if (script.isReset())
                    {

                        resetTimer = 0;
                        script.setReset(false);

                    }
                    else
                    {

                        if (resetTimer < 1) { resetTimer += dt; }

                    }

                }

                if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximalJoint, side, out MixedRealityPose pose3))
                {

                    hand.transform.GetChild(2).position = pose3.Position;
                    hand.transform.GetChild(2).rotation = pose.Rotation;
                 
                    displacement = pose3.Position - pose.Position;
                    distance = displacement.magnitude;

                }

            }

        }
        else { hand.SetActive(false); }

        if (hand.name == "Left Hand") { leftResetTimer = resetTimer; }
        else { rightResetTimer = resetTimer; }

    }

}
