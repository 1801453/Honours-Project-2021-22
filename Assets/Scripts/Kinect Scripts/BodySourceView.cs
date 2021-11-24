// Modified Microsoft Supplied Script for the Kinect
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodySourceView : MonoBehaviour 
{

    public Material BoneMaterial;
    public GameObject BodySourceManager, offsetObject, camera;
    
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;

    ulong playerID;
    
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

        foreach(var body in data)
        {

            if (body == null)  { continue;  }
                
            if(body.IsTracked) 
            {
                
                trackedIds.Add (body.TrackingId); 

                if (playerID == 0)
                { 
                    
                    playerID = body.TrackingId;

                    Vector3 offset = new Vector3(0, -camera.transform.eulerAngles.y, 0);

                    offsetObject.transform.eulerAngles = offset;
                
                }
            
            }

        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {

            if(!trackedIds.Contains(trackingId))
            {

                Destroy(_Bodies[trackingId]);

                _Bodies.Remove(trackingId);

                if (trackingId == playerID) { playerID = 0; }

            }

        }

        foreach(var body in data)
        {

            if (body == null) { continue; }
            
            if(body.IsTracked)
            {

                if(!_Bodies.ContainsKey(body.TrackingId)) {  _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId); }
                
                RefreshBodyObject(body, _Bodies[body.TrackingId]);

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
            
            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;

        }
        
        return body;

    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {

            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt)) { targetJoint = body.Joints[_BoneMap[jt]]; }
            
            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.localPosition = Quaternion.Euler(0, -offsetObject.transform.eulerAngles.y, 0) * GetVector3FromJoint(sourceJoint);
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

            if (targetJoint.HasValue)
            {

                lr.SetPosition(0, jointObj.localPosition + offsetObject.transform.position);
                lr.SetPosition(1, (Quaternion.Euler(0, -offsetObject.transform.eulerAngles.y, 0) * GetVector3FromJoint(targetJoint.Value)) + offsetObject.transform.position);
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));

            }
            else { lr.enabled = false; }

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
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, -joint.Position.Z * 10);
    }
}
