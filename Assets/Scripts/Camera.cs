using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class Camera : MonoBehaviour
{

    GameObject camera;

    ulong playerID;

    public GameObject BodySourceManager;
    public int type;

    private BodySourceManager _BodyManager;
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
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

    // Start is called before the first frame update
    void Start()
    {

        camera = GameObject.Find("Main Camera");
        
    }

    // Update is called once per frame
    void Update()
    {

        if (type == 0)  { camera.transform.position = Vector3.zero; }
        else if (type == 1)
        {
           
            Vector3 headPos = Vector3.zero;

            _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
            Kinect.Body[] data = _BodyManager.GetData();
            Kinect.Body tracked = null;

            if (data != null)
            {
                
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

                            tracked = body;
                    
                        }
                        else if (body.TrackingId == playerID) { tracked = body; }

                    }

                }

                if (trackedIds.Contains(playerID) && tracked != null)
                {

                    string name = "Body:";

                    name += playerID.ToString();
                    name += "/Head";
                    
                    GameObject head = GameObject.Find(name);

                    if (head != null) { headPos = head.transform.position; }

                }
                else { playerID = 0; }

            }

            camera.transform.position = headPos;

        }

    }
}
