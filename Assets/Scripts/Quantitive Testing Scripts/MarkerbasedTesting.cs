using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEditor;
using System.IO;

public class MarkerbasedTesting : MonoBehaviour
{

    public GameObject ui, head;
    public TextAsset textFile;

    float leftResetTimer = 5.1f, rightResetTimer = 5.1f;
    bool doneLeft, doneRight;

    Vector3 cameraPos, cameraLookAt, right, left;
    Quaternion rotation;

    StreamWriter writer;

    void Start()
    {

        doneLeft = false;
        doneRight = false;

        StreamWriter writer = new StreamWriter(AssetDatabase.GetAssetPath(textFile), true);
        writer.WriteLine("Right:Left");
        writer.Close();

    }

    // Update is called once per frame
    void Update()
    {
        cameraLookAt = head.transform.forward;

        rotation.SetFromToRotation(cameraLookAt, new Vector3(1, 0, 0));

        cameraPos = rotation * head.transform.position;

        // Complete the hand fuunctions for both hands
        handCheck(Handedness.Right);
        handCheck(Handedness.Left);

        if (doneLeft && doneRight)
        {

            writer.WriteLine(right.ToString(), ":", left.ToString());
            writer.Close();

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

                if (done && !doneLeft)
                {

                    left = displacement;
                    doneLeft = done;

                }
            
            }
            else 
            { 
                
                rightResetTimer = resetTimer;

                if (done && !doneRight)
                {

                    right = displacement;
                    doneRight = done;

                }

            }

        }

    }

}
