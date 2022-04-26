// Commented out sections of code are used for calculating shoulder points, sections of code with "//" on either side of them are used for the testing after shoulder points are calculated

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.IO;

public class MarkerbasedTesting : MonoBehaviour
{

    public GameObject head;
    public string path;
    public Vector3 rightTarget, leftTarget;

    float leftResetTimer = 5.1f, rightResetTimer = 5.1f;
    bool doneLeft, doneRight;

    Vector3 cameraPos, cameraLookAt, right, left, shoulderConnector, leftShoulderPos, rightShoulderPos;
    Quaternion rotation;

    StreamWriter writer;

    void Start()
    {

        doneLeft = false;
        doneRight = false;

        writer = new StreamWriter(path, true);

        /*
        writer.WriteLine("RightX,RightY,RightZ,LeftX,LeftY,LeftZ");
        */

        //
        writer.WriteLine("RightX,RightY,RightZ,RightOffset,LeftX,LeftY,LeftZ,LeftOffset");
        //

        writer.Close();

    }

    // Update is called once per frame
    void Update()
    {

        cameraLookAt = head.transform.forward;

        rotation.SetFromToRotation(cameraLookAt, new Vector3(1, 0, 0));

        cameraPos = rotation * head.transform.position;
        
        leftShoulderPos = rotation * head.transform.GetChild(0).transform.GetChild(0).transform.position;
        rightShoulderPos = rotation * head.transform.GetChild(0).transform.GetChild(1).transform.position;
        leftShoulderPos -= cameraPos;
        rightShoulderPos -= cameraPos;

        // Complete the hand fuunctions for both hands
        handCheck(Handedness.Right);
        handCheck(Handedness.Left);

        if (doneLeft && doneRight == doneLeft)
        {

            //
            float rightOffset, leftOffset;

            rightOffset = Vector3.Angle(rightTarget, right - rightShoulderPos);
            leftOffset = Vector3.Angle(leftTarget, left - leftShoulderPos);
            //

            writer = new StreamWriter(path, true);

            /*
            writer.WriteLine(right.ToString() + "," + left.ToString());
            */

            //
            writer.WriteLine(right.ToString() + "," + rightOffset + "," + left.ToString() + "," + leftOffset);
            //

            writer.Close();

            doneLeft = false;
            doneRight = false;

        }

    }

    void handCheck(Handedness side)
    {

        MixedRealityPose pose, pose2;

        // Check if the palm exists
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, side, out pose))
        {

            float resetTimer;
            Vector3 displacement;
            bool done;

            displacement = new Vector3(0, 0, 0);

            // If it does exist the determine which hand is currently being checked and get it's timer
            if (side == Handedness.Left)
            { 
                
                resetTimer = leftResetTimer;
                done = doneLeft;
            
            }
            else 
            { 
                
                resetTimer = rightResetTimer;
                done = doneRight;
            
            }

            // Get the middle section of the middle finger for the hand
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleMiddleJoint, side, out pose2) && !done)
            {

                float distance, dt = Time.deltaTime;

                // Get the distance between the palm and middle finger
                displacement = pose2.Position - pose.Position;
                distance = displacement.magnitude;

                // Check if it is within the range for grabbing and if the grab timer allows for grabbing
                if (distance < 0.075f && resetTimer >= 5)
                {

                    Vector3 handPos;

                    handPos = rotation * pose.Position;
                    displacement = handPos - cameraPos;

                    done = true;

                }
                else { resetTimer += dt; }

            }

            // Update the timer for the corresponding hand
            if (side == Handedness.Left) 
            { 
                
                leftResetTimer = resetTimer;

                if (done != doneLeft)
                {

                    left = displacement;
                    doneLeft = done;

                }
            
            }
            else 
            { 
                
                rightResetTimer = resetTimer;

                if (done != doneRight)
                {

                    right = displacement;
                    doneRight = done;

                }

            }

        }

    }

}
