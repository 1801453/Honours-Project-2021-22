using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

public class HandMaster : MonoBehaviour
{

    public GameObject leftHand, rightHand, ui;

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

        MixedRealityPose pose, pose2;

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, side, out pose))
        {

            float resetTimer;

            if (hand.name == "Left Hand") { resetTimer = leftResetTimer; }
            else { resetTimer = rightResetTimer; }

            hand.SetActive(true);

            hand.transform.GetChild(0).position = pose.Position;
            hand.transform.GetChild(0).rotation = pose.Rotation;

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleMiddleJoint, side, out pose2))
            {

                Vector3 displacement;
                float distance, dt = Time.deltaTime;

                hand.transform.GetChild(1).position = pose.Position + (pose.Rotation * (new Vector3(0, 0, 0.0442f)));
                hand.transform.GetChild(1).rotation = pose.Rotation;
                
                displacement = pose2.Position - pose.Position;
                distance = displacement.magnitude;

                FingerTrigger script = hand.transform.GetChild(0).GetChild(0).gameObject.GetComponent<FingerTrigger>();
                FingerTrigger script2 = hand.transform.GetChild(1).GetChild(0).gameObject.GetComponent<FingerTrigger>();

                if (distance < 0.075f && resetTimer >= 1) 
                {
                    
                    script.IsHolding();
                    script2.IsHolding();

                }
                else
                {

                    script.stopHolding();
                    script2.stopHolding();

                    if (script.isReset() && script2.isReset())
                    {

                        resetTimer = 0;
                        script.setReset(false);
                        script2.setReset(false);

                    }
                    else if (script.isReset())
                    {

                        resetTimer = 0;
                        script.setReset(false);

                    }
                    else if (script2.isReset())
                    {

                        resetTimer = 0;
                        script2.setReset(false);

                    }
                    else
                    {

                        if (resetTimer < 1) { resetTimer += dt; }

                    }

                }

            }

            if (hand.name == "Left Hand") { leftResetTimer = resetTimer; }
            else { rightResetTimer = resetTimer; }

        }
        else { hand.SetActive(false); }

    }

}
