// Modified Microsoft Supplied Script for the Kinect
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

using static System.Math;

public class BodySourceView : MonoBehaviour 
{

    public Material BoneMaterial;
    public GameObject BodySourceManager, offsetObject, camera;
    
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    private Vector3 rotationalOffset;

    ulong playerID;
    float timer = 0;
    
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
        
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {

            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);
            
            jointObj.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;

        }

        GameObject model = new GameObject("Model:" + id);

        model.transform.SetParent(offsetObject.transform);

        CreateCapsule(model, "FootlLeft");
        CreateCapsule(model, "ShinLeft");
        CreateCapsule(model, "ThighLeft");
        CreateCapsule(model, "HipLeft");
        CreateCapsule(model, "FootlRight");
        CreateCapsule(model, "ShinRight");
        CreateCapsule(model, "ThighRight");
        CreateCapsule(model, "HipRight");
        CreateCapsule(model, "ThumbLeft");
        CreateCapsule(model, "FingersLeft");
        CreateCapsule(model, "HandLeft");
        CreateCapsule(model, "ForarmLeft");
        CreateCapsule(model, "BicepLeft");
        CreateCapsule(model, "ShoulderLeft");
        CreateCapsule(model, "ThumbRight");
        CreateCapsule(model, "FingersRight");
        CreateCapsule(model, "HandRight");
        CreateCapsule(model, "ForarmRight");
        CreateCapsule(model, "BicepRight");
        CreateCapsule(model, "ShoulderRight");
        CreateCapsule(model, "Stomach");
        CreateCapsule(model, "Chest");
        CreateCapsule(model, "Neck");
        CreateCapsule(model, "Head");

        return body;

    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {

            Kinect.Joint source = body.Joints[jt];
            Kinect.Joint? target = null;
            
            if(_BoneMap.ContainsKey(jt)) { target = body.Joints[_BoneMap[jt]]; }
            
            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.localPosition = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(source);
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();

            if (body.TrackingId == playerID)
            {

                if (jt == Kinect.JointType.Head)
                {

                    Vector3 offset;

                    offset = camera.transform.position - jointObj.position + offsetObject.transform.position;
                    
                    offsetObject.transform.position = offset;

                }

            }

            if (target.HasValue)
            {

                lr.SetPosition(0, jointObj.position);
                lr.SetPosition(1, (Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(target.Value)) + offsetObject.transform.position);
                lr.SetColors(GetColorForState (source.TrackingState), GetColorForState(target.Value.TrackingState));

            }
            else { lr.enabled = false; }

        }

        // Create capsule rotations to connect tracked joints
        GameObject model = GameObject.Find("Model:" + body.TrackingId);
        Vector3 start, end;

        Kinect.Joint sourceJoint = body.Joints[Kinect.JointType.FootLeft];
        Kinect.Joint targetJoint = body.Joints[_BoneMap[Kinect.JointType.FootLeft]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "FootlLeft", start, end, 0.07f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.AnkleLeft];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.AnkleLeft]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "ShinLeft", start, end, 0.1f, 0.07f);

        sourceJoint = body.Joints[Kinect.JointType.KneeLeft];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.KneeLeft]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "ThighLeft", start, end, 0.1f, 0.07f);

        sourceJoint = body.Joints[Kinect.JointType.HipLeft];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.HipLeft]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "HipLeft", start, end, 0.1f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.FootRight];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.FootRight]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "FootlRight", start, end, 0.07f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.AnkleRight];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.AnkleRight]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "ShinRight", start, end, 0.1f, 0.07f);

        sourceJoint = body.Joints[Kinect.JointType.KneeRight];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.KneeRight]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "ThighRight", start, end, 0.1f, 0.07f);

        sourceJoint = body.Joints[Kinect.JointType.HipRight];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.HipRight]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "HipRight", start, end, 0.1f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.ThumbLeft];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.ThumbLeft]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "ThumbLeft", start, end, 0.05f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.HandTipLeft];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.HandTipLeft]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "FingersLeft", start, end, 0.05f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.HandLeft];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.HandLeft]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "HandLeft", start, end, 0.06f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.WristLeft];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.WristLeft]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "ForarmLeft", start, end, 0.07f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.ElbowLeft];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.ElbowLeft]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "BicepLeft", start, end, 0.07f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.ShoulderLeft];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.ShoulderLeft]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "ShoulderLeft", start, end, 0.1f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.ThumbRight];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.ThumbRight]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "ThumbRight", start, end, 0.05f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.HandTipRight];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.HandTipRight]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "FingersRight", start, end, 0.05f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.HandRight];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.HandRight]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "HandRight", start, end, 0.06f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.WristRight];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.WristRight]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "ForarmRight", start, end, 0.07f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.ElbowRight];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.ElbowRight]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "BicepRight", start, end, 0.07f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.ShoulderRight];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.ShoulderRight]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "ShoulderRight", start, end, 0.1f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.SpineBase];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.SpineBase]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "Stomach", start, end, 0.3f, 0.1f);

        sourceJoint = body.Joints[Kinect.JointType.SpineMid];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.SpineMid]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "Chest", start, end, 0.3f, 0.1f);

        sourceJoint = body.Joints[Kinect.JointType.SpineShoulder];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.SpineShoulder]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "Neck", start, end, 0.05f, 0.05f);

        sourceJoint = body.Joints[Kinect.JointType.Neck];
        targetJoint = body.Joints[_BoneMap[Kinect.JointType.Neck]];

        start = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(sourceJoint);
        end = Quaternion.Euler(0, -rotationalOffset.y, 0) * GetVector3FromJoint(targetJoint);

        UpdateCapsule(model, "Head", start, end, 0.05f, 0.05f);

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

        capsule.name = name;
        capsule.transform.parent = parent.transform;

    }

    private void UpdateCapsule(GameObject parent, string name, Vector3 start, Vector3 end, float scaleX, float scaleZ)
    {

        GameObject capsule = parent.transform.Find(name).gameObject;
        Vector3 length;

        length = end - start;

        capsule.transform.localPosition = start + (length / 2);
        capsule.transform.localScale = new Vector3(scaleX, (length.magnitude / 4) * 2.5f, scaleZ);
        capsule.transform.rotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), length);

    }

}
