// Script base supplied by Microsoft, altered to fit the artefact
// Modified sections are commented
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

using static System.Math;

public class MarkerlessTesting : MonoBehaviour
{

    struct Offsets
    {

        public string name;
        public Vector3 endPos;

    }

    public Material BoneMaterial;
    public GameObject BodySourceManager, offsetObject, camera;

    Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    BodySourceManager _BodyManager;
    Vector3 rotationalOffset, leftGrabbedOffset, rightGrabbedOffset;
    GameObject leftGrabbedObject, rightGrabbedObject;

    ulong playerID;
    float timer = 0, leftGrabTimer = 0, rightGrabTimer = 0, leftResetTimer = 1.1f, rightResetTimer = 1.1f;
    bool leftHolding = false, rightHolding = false, leftReset = false, rightReset = false, leftClosed = false, rightClosed = false;

    Offsets[] offsets = new Offsets[24];

    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };

    void Update()
    {


        // Increment the refresh timer
        timer += Time.deltaTime;

        // Check if a kinect frame has elapsed
        if (timer >= 1.0f / 30.0f)
        {

            // If it has subtract the timer by 1 kinect frame
            timer -= 1.0f / 30.0f;

            if (BodySourceManager == null)
            {
                return;
            }

            _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();

            if (_BodyManager == null)
            {
                return;
            }

            Kinect.Body[] data = _BodyManager.GetData();

            if (data == null)
            {
                return;
            }

            List<ulong> trackedIds = new List<ulong>();

            foreach (var body in data)
            {

                if (body == null) { continue; }

                if (body.IsTracked)
                {

                    trackedIds.Add(body.TrackingId);

                    // Check if there is a player ID
                    if (playerID == 0)
                    {

                        // If not set the player ID to the current body ID
                        playerID = body.TrackingId;

                        // Set the global rotational offset
                        rotationalOffset = new Vector3(0, camera.transform.eulerAngles.y + 157.5f, 0);

                    }

                }

            }

            List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

            // First delete untracked bodies
            foreach (ulong trackingId in knownIds)
            {

                if (!trackedIds.Contains(trackingId))
                {

                    Destroy(_Bodies[trackingId]);

                    _Bodies.Remove(trackingId);

                    // Check if the body lost was the player, and if so signal reset the player ID to 0 so it can be reset
                    if (trackingId == playerID) { playerID = 0; }

                    // Destroy the Body and Model objects of the tracked body
                    Destroy(GameObject.Find("Body:" + trackingId));
                    Destroy(GameObject.Find("Model:" + trackingId));

                }

            }

            foreach (var body in data)
            {

                if (body == null) { continue; }

                if (body.IsTracked)
                {

                    if (!_Bodies.ContainsKey(body.TrackingId)) { _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId); }

                    RefreshBodyObject(body, _Bodies[body.TrackingId]);

                }

            }

        }

    }

    private GameObject CreateBodyObject(ulong id)
    {

        GameObject body = new GameObject("Body:" + id);

        // Set the parent of this object to be the offset object
        body.transform.SetParent(offsetObject.transform);

        // Create the empty object to store the model segments
        GameObject model = new GameObject("Model:" + id);

        // Set it to have the same parent as the body object
        model.transform.SetParent(offsetObject.transform);

        // Create each segment of the body for later alteration during refresh frames
        CreateCapsule(model, "Head");

        offsets[0].name = "Head";

        CreateCapsule(model, "Neck");

        offsets[1].name = "Neck";

        CreateCapsule(model, "ShoulderRight");

        offsets[2].name = "ShoulderRight";

        CreateCapsule(model, "BicepRight");

        offsets[3].name = "BicepRight";

        CreateCapsule(model, "ForarmRight");

        offsets[4].name = "ForarmRight";

        CreateCapsule(model, "HandRight");

        offsets[5].name = "HandRight";

        CreateCapsule(model, "FingersRight");

        offsets[6].name = "FingersRight";

        CreateCapsule(model, "ThumbRight");

        offsets[7].name = "ThumbRight";

        CreateCapsule(model, "ShoulderLeft");

        offsets[8].name = "ShoulderLeft";

        CreateCapsule(model, "BicepLeft");

        offsets[9].name = "BicepLeft";

        CreateCapsule(model, "ForarmLeft");

        offsets[10].name = "ForarmLeft";

        CreateCapsule(model, "HandLeft");

        offsets[11].name = "HandLeft";

        CreateCapsule(model, "FingersLeft");

        offsets[12].name = "FingersLeft";

        CreateCapsule(model, "ThumbLeft");

        offsets[13].name = "ThumbLeft";

        CreateCapsule(model, "Chest");

        offsets[14].name = "Chest";

        CreateCapsule(model, "Stomach");

        offsets[15].name = "Stomach";

        CreateCapsule(model, "HipRight");

        offsets[16].name = "HipRight";

        CreateCapsule(model, "ThighRight");

        offsets[17].name = "ThighRight";

        CreateCapsule(model, "ShinRight");

        offsets[18].name = "ShinRight";

        CreateCapsule(model, "FootRight");

        offsets[19].name = "FootRight";

        CreateCapsule(model, "HipLeft");

        offsets[20].name = "HipLeft";

        CreateCapsule(model, "ThighLeft");

        offsets[21].name = "ThighLeft";

        CreateCapsule(model, "ShinLeft");

        offsets[22].name = "ShinLeft";

        CreateCapsule(model, "FootLeft");

        offsets[23].name = "FootLeft";

        // Check if this is the player body
        if (id == playerID)
        {

            // If so create 2 more model objects in the fingers of both hands that will act as triggers to dect collisions when grabbing
            GameObject triggerLeft = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Rigidbody rigidbody = triggerLeft.AddComponent<Rigidbody>();
            Collider collider = triggerLeft.GetComponent<Collider>();
            MeshRenderer renderer = triggerLeft.GetComponent<MeshRenderer>();
            FingerTrigger scriptLeft = triggerLeft.AddComponent<FingerTrigger>();

            triggerLeft.name = "FingersLeftTrigger";
            triggerLeft.transform.parent = model.transform.Find("FingersLeft");
            rigidbody.isKinematic = true;
            collider.isTrigger = true;
            renderer.enabled = false;

            GameObject triggerRight = GameObject.CreatePrimitive(PrimitiveType.Capsule);

            rigidbody = triggerRight.AddComponent<Rigidbody>();
            collider = triggerRight.GetComponent<Collider>();
            renderer = triggerRight.GetComponent<MeshRenderer>();

            FingerTrigger scriptRight = triggerRight.AddComponent<FingerTrigger>();

            triggerRight.name = "FingersRightTrigger";
            triggerRight.transform.parent = model.transform.Find("FingersRight");
            rigidbody.isKinematic = true;
            collider.isTrigger = true;
            renderer.enabled = false;

            // Give the scripts of each collider the information needed to allow for proper rotation and positioning of grabbed objects
            scriptLeft.otherHand = triggerRight;
            scriptRight.otherHand = triggerLeft;

            scriptLeft.hand = model.transform.Find("HandLeft").gameObject;
            scriptRight.hand = model.transform.Find("HandRight").gameObject;

        }

        return body;

    }

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {

        // Find the model being refreshed
        GameObject model = GameObject.Find("Model:" + body.TrackingId);
        Vector3 start, end;

        Kinect.Joint sourceJoint = body.Joints[Kinect.JointType.Neck];
        Kinect.Joint targetJoint = body.Joints[_BoneMap[Kinect.JointType.Neck]];

        // Refresh the head
        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "Head", start, end, 0.05f, 0.05f);

        // Refresh the neck, child of head
        sourceJoint = body.Joints[Kinect.JointType.SpineShoulder];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[0].endPos;

        UpdateCapsule(model, "Neck", start, end, 0.05f, 0.05f);

        // Refresh the right arm
        // Refresh the shoulder, child of neck
        sourceJoint = body.Joints[Kinect.JointType.ShoulderRight];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[1].endPos;

        UpdateCapsule(model, "ShoulderRight", start, end, 0.1f, 0.05f);

        // Refresh bicep, child of shoulder
        sourceJoint = body.Joints[Kinect.JointType.ElbowRight];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[2].endPos;

        UpdateCapsule(model, "BicepRight", start, end, 0.07f, 0.05f);

        // Refresh forarm, child of bicep
        sourceJoint = body.Joints[Kinect.JointType.WristRight];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[3].endPos;

        UpdateCapsule(model, "ForarmRight", start, end, 0.07f, 0.05f);

        // Refresh hand, child of forarm
        sourceJoint = body.Joints[Kinect.JointType.HandRight];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[4].endPos;

        UpdateCapsule(model, "HandRight", start, end, 0.06f, 0.05f);

        // Refresh fingers, child of hand
        sourceJoint = body.Joints[Kinect.JointType.HandTipRight];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[5].endPos;

        UpdateCapsule(model, "FingersRight", start, end, 0.05f, 0.05f);

        // Refresh thumb, child of hand
        sourceJoint = body.Joints[Kinect.JointType.ThumbRight];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[5].endPos;

        UpdateCapsule(model, "ThumbRight", start, end, 0.05f, 0.05f);

        // Refresh left arm
        // Refresh shoulder, child of neck
        sourceJoint = body.Joints[Kinect.JointType.ShoulderLeft];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[1].endPos;

        UpdateCapsule(model, "ShoulderLeft", start, end, 0.1f, 0.05f);

        // Refresh bicep, child of shoulder
        sourceJoint = body.Joints[Kinect.JointType.ElbowLeft];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[8].endPos;

        UpdateCapsule(model, "BicepLeft", start, end, 0.07f, 0.05f);

        // Refresh forarm, child of bicep
        sourceJoint = body.Joints[Kinect.JointType.WristLeft];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[9].endPos;

        UpdateCapsule(model, "ForarmLeft", start, end, 0.07f, 0.05f);

        // Refresh hand, child of forarm
        sourceJoint = body.Joints[Kinect.JointType.HandLeft];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[10].endPos;

        UpdateCapsule(model, "HandLeft", start, end, 0.06f, 0.05f);

        // Refresh fingers, child of hand
        sourceJoint = body.Joints[Kinect.JointType.HandTipLeft];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[11].endPos;

        UpdateCapsule(model, "FingersLeft", start, end, 0.05f, 0.05f);

        // Refresh thumb, child of hand
        sourceJoint = body.Joints[Kinect.JointType.ThumbLeft];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[11].endPos;

        UpdateCapsule(model, "ThumbLeft", start, end, 0.05f, 0.05f);

        // Refresh chest, child of neck
        sourceJoint = body.Joints[Kinect.JointType.SpineMid];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[1].endPos;

        UpdateCapsule(model, "Chest", start, end, 0.3f, 0.1f);

        // Refresh stomach, child of chest
        sourceJoint = body.Joints[Kinect.JointType.SpineBase];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[14].endPos;

        UpdateCapsule(model, "Stomach", start, end, 0.3f, 0.1f);

        // Refresh right leg
        // Refresh hip, child of stomach
        sourceJoint = body.Joints[Kinect.JointType.HipRight];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[15].endPos;

        UpdateCapsule(model, "HipRight", start, end, 0.1f, 0.05f);

        // Refresh thigh, child of hip
        sourceJoint = body.Joints[Kinect.JointType.KneeRight];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[16].endPos;

        UpdateCapsule(model, "ThighRight", start, end, 0.1f, 0.07f);

        // Refresh shin, child of thigh
        sourceJoint = body.Joints[Kinect.JointType.AnkleRight];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[17].endPos;

        UpdateCapsule(model, "ShinRight", start, end, 0.1f, 0.07f);

        // Refresh foot, child of shin
        sourceJoint = body.Joints[Kinect.JointType.FootRight];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[18].endPos;

        UpdateCapsule(model, "FootRight", start, end, 0.07f, 0.05f);

        // Refresh left leg
        // Refresh hip, child of stomach
        sourceJoint = body.Joints[Kinect.JointType.HipLeft];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[15].endPos;

        UpdateCapsule(model, "HipLeft", start, end, 0.1f, 0.05f);

        // Refresh thigh, child of hip
        sourceJoint = body.Joints[Kinect.JointType.KneeLeft];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[20].endPos;

        UpdateCapsule(model, "ThighLeft", start, end, 0.1f, 0.07f);

        // Refresh shin, child of thigh
        sourceJoint = body.Joints[Kinect.JointType.AnkleLeft];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[21].endPos;

        UpdateCapsule(model, "ShinLeft", start, end, 0.1f, 0.07f);

        // Refresh foot, child of shin
        sourceJoint = body.Joints[Kinect.JointType.FootLeft];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = offsets[22].endPos;

        UpdateCapsule(model, "FootLeft", start, end, 0.07f, 0.05f);

        // Rotate chest and stomach to face the correct direction
        Vector3 connections, forward, rotation;

        float angle, angle2;

        // Rotate the chest to connect with the 2 shoulders
        forward = new Vector3(1, 0, 0);
        connections = (Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(body.Joints[Kinect.JointType.ShoulderRight])) - (Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(body.Joints[Kinect.JointType.ShoulderLeft]));
        angle = Vector3.Angle(forward, connections);
        rotation = model.transform.Find("Chest").gameObject.transform.eulerAngles;
        rotation.y -= angle;
        model.transform.Find("Chest").gameObject.transform.eulerAngles = rotation;

        // Rotate the stomach to be between the chest rotation and the hip rotation
        connections = (Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(body.Joints[Kinect.JointType.HipRight])) - (Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(body.Joints[Kinect.JointType.HipLeft]));
        angle2 = Vector3.Angle(forward, connections);
        angle = (angle + angle2) / 2;
        rotation = model.transform.Find("Stomach").gameObject.transform.eulerAngles;
        rotation.y -= angle;
        model.transform.Find("Stomach").gameObject.transform.eulerAngles = rotation;

        // Check if this is the player body
        if (body.TrackingId == playerID)
        {

            float dt = Time.deltaTime;

            Vector3 offset;

            // If so set the global offset for all bodies
            offset = camera.transform.position - model.transform.Find("Head").position + offsetObject.transform.position;

            offsetObject.transform.position = offset;

        }

    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        // Get the positions of the joint in a Vector3 format
        return new Vector3(joint.Position.X, joint.Position.Y, -joint.Position.Z);
    }

    private void CreateCapsule(GameObject parent, string name)
    {

        // Create a capsule object with the given name and parent, as well as setting it as a kinematic rigidbody
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Rigidbody rigidbody = capsule.AddComponent<Rigidbody>();

        capsule.name = name;
        capsule.transform.parent = parent.transform;
        rigidbody.isKinematic = true;

    }

    private void UpdateCapsule(GameObject parent, string name, Vector3 start, Vector3 end, float scaleX, float scaleZ)
    {

        // Get the corresponding game object to refresh
        GameObject capsule = parent.transform.Find(name).gameObject;
        Vector3 length;

        int id = 0;
        float lengthMag;

        // Find the desired object in the array of object offsets
        for (int i = 0; i < 24; i++)
        {

            if (offsets[i].name == name)
            {

                id = i;
                i = 24;

            }

        }

        // Get the distance between the joints the object connects
        length = end - start;

        // Get the lengths magnitude
        lengthMag = length.magnitude;

        // Set the segments position, rotation and scale
        capsule.transform.localPosition = end - (length / 2);
        capsule.transform.localScale = new Vector3(scaleX, (lengthMag / 4) * 2.5f, scaleZ);
        capsule.transform.rotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), length);

        // Set the end position
        offsets[id].endPos = start;

    }

}
