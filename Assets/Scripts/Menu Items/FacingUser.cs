using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FacingUser : MonoBehaviour
{

    GameObject camera;

    // Start is called before the first frame update
    void Start()
    {

        camera = GameObject.Find("Main Camera");

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 cameraPosition, myPosition, direction, rotation, offset;

        float angle;

        cameraPosition = camera.transform.position;
        myPosition = this.transform.parent.position;

        direction = cameraPosition - myPosition;

        angle = (float)Math.Atan2(-direction.x, -direction.z);

        offset = new Vector3(0, 0, -0.51f);

        offset.x = (0 * (float)Math.Cos(-angle)) - (-0.51f * (float)Math.Sin(-angle));
        offset.z = (0 * (float)Math.Sin(-angle)) + (-0.51f * (float)Math.Cos(-angle));

        angle = angle * (180 / (float)Math.PI);

        rotation = new Vector3(90, angle + 180, 0);

        this.transform.eulerAngles = rotation;
        this.transform.localPosition = offset;

    }

}
