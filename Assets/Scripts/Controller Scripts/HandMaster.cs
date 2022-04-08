using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

public class HandMaster : MonoBehaviour
{

    public GameObject leftHand, rightHand, ui;

    float leftResetTimer = 1.1f, rightResetTimer = 1.1f;

    void Update()
    {

        // Complete the hand fuunctions for both hands
        handCheck(rightHand, Handedness.Right);
        handCheck(leftHand, Handedness.Left);

    }

    void handCheck(GameObject hand, Handedness side)
    {

        MixedRealityPose pose, pose2;

        // Check if the palm exists
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, side, out pose))
        {

            float resetTimer;

            // If it does exist the determine which hand is currently being checked and get it's timer
            if (hand.name == "Left Hand") { resetTimer = leftResetTimer; }
            else { resetTimer = rightResetTimer; }

            // Set the hand to be active
            hand.SetActive(true);

            // Set the palm's position and rotation
            hand.transform.GetChild(0).position = pose.Position;
            hand.transform.GetChild(0).rotation = pose.Rotation;

            // Get the middle section of the middle finger for the hand
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleMiddleJoint, side, out pose2))
            {

                Vector3 displacement;
                float distance, dt = Time.deltaTime;

                // Set the finger's position and rotation
                hand.transform.GetChild(1).position = pose.Position + (pose.Rotation * (new Vector3(0, 0, 0.0442f)));
                hand.transform.GetChild(1).rotation = pose.Rotation;
                
                // Get the distance between the palm and middle finger
                displacement = pose2.Position - pose.Position;
                distance = displacement.magnitude;

                FingerTrigger script = hand.transform.GetChild(1).GetChild(0).gameObject.GetComponent<FingerTrigger>();

                // Check if it is within the range for grabbing and if the grab timer allows for grabbing
                if (distance < 0.075f && resetTimer >= 1) 
                {
                    
                    // If so then call the function signalling it is grabbing
                    script.IsHolding();

                }
                else
                {

                    // If it isn't call the function that it is not grabbing
                    script.stopHolding();

                    // Determine if it has just stopped grabbing
                    if (script.isReset())
                    {

                        // If so then reset the timer and signal the timer has been reset
                        resetTimer = 0;

                        script.setReset(false);

                    }
                    else
                    {

                        // If it hasn't just stopped grabbing then increment the grab timer
                        if (resetTimer < 1) { resetTimer += dt; }

                    }

                }

            }

            // Update the timer for the corresponding hand
            if (hand.name == "Left Hand") { leftResetTimer = resetTimer; }
            else { rightResetTimer = resetTimer; }

        }
        // If the palm doesn't exist then deactivate the hand
        else { hand.SetActive(false); }

    }

}
