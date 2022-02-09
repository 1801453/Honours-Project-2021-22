// Modified Microsoft Supplied Script for the Kinect
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

using static System.Math;

public class BodySourceView : MonoBehaviour 
{

    struct Offsets
    {

        public string name;
        public Vector3 offset, endPos;

    }
    
    public Material BoneMaterial;
    public GameObject BodySourceManager, offsetObject, camera;
    
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    private Vector3 rotationalOffset;

    ulong playerID;
    float timer = 0, leftGrabTimer = 0, rightGrabTimer = 0;
    bool leftHolding = false, rightHolding = false;

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
    
    void Update () 
    {


        timer += Time.deltaTime;

        if (timer >= 1.0f / 30.0f)
        {

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

                    if (playerID == 0)
                    {

                        playerID = body.TrackingId;

                        rotationalOffset = new Vector3(0, camera.transform.eulerAngles.y + 90, 0);

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

                    if (trackingId == playerID) { playerID = 0; }

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

        body.transform.SetParent(offsetObject.transform);

        GameObject model = new GameObject("Model:" + id);

        model.transform.SetParent(offsetObject.transform);

        CreateCapsule(model, "Head");

        offsets[0].name = "Head";
        offsets[0].offset = new Vector3(0, 0, 0);

            CreateCapsule(model, "Neck");

            offsets[1].name = "Neck";
            offsets[1].offset = new Vector3(0, 0, 0);

                CreateCapsule(model, "ShoulderRight");

                offsets[2].name = "ShoulderRight";
                offsets[2].offset = new Vector3(0, 0, 0);

                    CreateCapsule(model, "BicepRight");

                    offsets[3].name = "BicepRight";
                    offsets[3].offset = new Vector3(0, 0, 0);

                        CreateCapsule(model, "ForarmRight");

                        offsets[4].name = "ForarmRight";
                        offsets[4].offset = new Vector3(0, 0, 0);

                            CreateCapsule(model, "HandRight");

                            offsets[5].name = "HandRight";
                            offsets[5].offset = new Vector3(0, 0, 0);

                                CreateCapsule(model, "FingersRight");

                                offsets[6].name = "FingersRight";
                                offsets[6].offset = new Vector3(0, 0, 0);

                                CreateCapsule(model, "ThumbRight");

                                offsets[7].name = "ThumbRight";
                                offsets[7].offset = new Vector3(0, 0, 0);

                CreateCapsule(model, "ShoulderLeft");

                offsets[8].name = "ShoulderLeft";
                offsets[8].offset = new Vector3(0, 0, 0);

                    CreateCapsule(model, "BicepLeft");

                    offsets[9].name = "BicepLeft";
                    offsets[9].offset = new Vector3(0, 0, 0);

                        CreateCapsule(model, "ForarmLeft");

                        offsets[10].name = "ForarmLeft";
                        offsets[10].offset = new Vector3(0, 0, 0);

                            CreateCapsule(model, "HandLeft");

                            offsets[11].name = "HandLeft";
                            offsets[11].offset = new Vector3(0, 0, 0);

                                CreateCapsule(model, "FingersLeft");

                                offsets[12].name = "FingersLeft";
                                offsets[12].offset = new Vector3(0, 0, 0);

                                CreateCapsule(model, "ThumbLeft");

                                offsets[13].name = "ThumbLeft";
                                offsets[13].offset = new Vector3(0, 0, 0);

                CreateCapsule(model, "Chest");

                offsets[14].name = "Chest";
                offsets[14].offset = new Vector3(0, 0, 0);

                    CreateCapsule(model, "Stomach");

                    offsets[15].name = "Stomach";
                    offsets[15].offset = new Vector3(0, 0, 0);

                        CreateCapsule(model, "HipRight");

                        offsets[16].name = "HipRight";
                        offsets[16].offset = new Vector3(0, 0, 0);

                            CreateCapsule(model, "ThighRight");

                            offsets[17].name = "ThighRight";
                            offsets[17].offset = new Vector3(0, 0, 0);

                                CreateCapsule(model, "ShinRight");

                                offsets[18].name = "ShinRight";
                                offsets[18].offset = new Vector3(0, 0, 0);

                                    CreateCapsule(model, "FootRight");

                                    offsets[19].name = "FootRight";
                                    offsets[19].offset = new Vector3(0, 0, 0);

                        CreateCapsule(model, "HipLeft");

                        offsets[20].name = "HipLeft";
                        offsets[20].offset = new Vector3(0, 0, 0);

                            CreateCapsule(model, "ThighLeft");

                            offsets[21].name = "ThighLeft";
                            offsets[21].offset = new Vector3(0, 0, 0);

                                CreateCapsule(model, "ShinLeft");

                                offsets[22].name = "ShinLeft";
                                offsets[22].offset = new Vector3(0, 0, 0);

                                    CreateCapsule(model, "FootLeft");

                                    offsets[23].name = "FootLeft";
                                    offsets[23].offset = new Vector3(0, 0, 0);

        if (id == playerID)
        {

            GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Rigidbody rigidbody = trigger.AddComponent<Rigidbody>();
            Collider collider = trigger.GetComponent<Collider>();
            MeshRenderer renderer = trigger.GetComponent<MeshRenderer>();

            trigger.name = "FingersLeftTrigger";
            trigger.transform.parent = model.transform.Find("FingersLeft");
            rigidbody.isKinematic = true;
            collider.isTrigger = true;
            renderer.enabled = false;
            
            trigger = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rigidbody = trigger.AddComponent<Rigidbody>();
            collider = trigger.GetComponent<Collider>();
            renderer = trigger.GetComponent<MeshRenderer>();

            trigger.name = "FingersRightTrigger";
            trigger.transform.parent = model.transform.Find("FingersRight");
            rigidbody.isKinematic = true;
            collider.isTrigger = true;
            renderer.enabled = false;

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

        // Set the offset of the body
        Vector3 offset;

        offset = camera.transform.position - model.transform.Find("Head").position + offsetObject.transform.position;

        offsetObject.transform.position = offset;

        // Rotate chest and stomach to face the correct direction
        Vector3 connections, forward, rotation;

        float angle, angle2;

        forward = new Vector3(1, 0, 0);
        connections = (Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(body.Joints[Kinect.JointType.ShoulderRight])) - (Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(body.Joints[Kinect.JointType.ShoulderLeft]));
        angle = Vector3.Angle(forward, connections);
        rotation = model.transform.Find("Chest").gameObject.transform.eulerAngles;
        rotation.y -= angle;
        model.transform.Find("Chest").gameObject.transform.eulerAngles = rotation;

        connections = (Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(body.Joints[Kinect.JointType.HipRight])) - (Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(body.Joints[Kinect.JointType.HipLeft]));
        angle2 = Vector3.Angle(forward, connections);
        angle = (angle + angle2) / 2;
        rotation = model.transform.Find("Stomach").gameObject.transform.eulerAngles;
        rotation.y -= angle;
        model.transform.Find("Stomach").gameObject.transform.eulerAngles = rotation;

        if (body.TrackingId == playerID)
        {

            float scaled;

            float testValue = 0.046f;

            scaled = model.transform.Find("FingersLeft").gameObject.transform.localScale.y;

            if (scaled < testValue) {  }
            else 
            { 
                
                leftHolding = false;
                leftGrabTimer = 0;
            
            }

            scaled = model.transform.Find("FingersRight").gameObject.transform.localScale.y;

            if (scaled < testValue) {  }
            else 
            { 
                
                rightHolding = false;
                rightGrabTimer = 0;
            
            }

        }

    }
    
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X, joint.Position.Y, -joint.Position.Z);
    }

    private void CreateCapsule(GameObject parent, string name)
    {

        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Rigidbody rigidbody = capsule.AddComponent<Rigidbody>();

        capsule.name = name;
        capsule.transform.parent = parent.transform;
        rigidbody.isKinematic = true;

    }

    private void UpdateCapsule(GameObject parent, string name, Vector3 start, Vector3 end, float scaleX, float scaleZ)
    {

        GameObject capsule = parent.transform.Find(name).gameObject;
        Vector3 length;

        int id = 0;
        float lengthMag;

        for (int i = 0; i < 24; i++)
        {

            if (offsets[i].name == name)
            {

                id = i;
                i = 24;

            }

        }

        length = end - start;

        if (offsets[id].offset != new Vector3(0, 0, 0) && offsets[id].offset != length)
        {

            float difference;
            bool negative;

            difference = Vector3.Angle(offsets[id].offset, length);
            negative = false;

            if (difference < 0) 
            { 
                
                difference *= -1;
                negative = true;
            
            }

            if (difference > 180) 
            { 
                
                difference = 360 - difference;

                negative = !negative;
            
            }

            if (difference > 10) { length = Vector3.RotateTowards(offsets[id].offset, length, 10 * Mathf.Deg2Rad, 60); }

        }

        lengthMag = length.magnitude;

        capsule.transform.localPosition = end - (length / 2);
        capsule.transform.localScale = new Vector3(scaleX, (lengthMag / 4) * 2.5f, scaleZ);
        capsule.transform.rotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), length);

        offsets[id].endPos = start;

    }

    private void IsHolding(GameObject parent, string name)
    {

        bool holding;
        float grabTimer;

        float testValue = 1;

        if (name == "FingersLeft") 
        { 
            
            holding = leftHolding;

            if (leftGrabTimer < testValue) { leftGrabTimer += Time.deltaTime; }

            grabTimer = leftGrabTimer;
        
        }
        else 
        { 
            
            holding = rightHolding;

            if (rightGrabTimer < testValue) { rightGrabTimer += Time.deltaTime; }

            grabTimer = rightGrabTimer;

        }

        if (!holding && grabTimer < testValue)
        {

            GameObject[] objects;
            Collider fingerCollider;

            objects = GameObject.FindGameObjectsWithTag("Object");
            fingerCollider = parent.transform.Find(name).transform.GetChild(0).GetComponent<Collider>();

            for (int i = 0; i < objects.Length; i++)
            {

                Collider collider;

                collider = objects[i].GetComponent<Collider>();

                

            }

        }
        else if (holding)
        {



        }

    }

}
