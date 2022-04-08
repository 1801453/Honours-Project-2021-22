using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetObjects : MonoBehaviour
{

    GameObject camera;
    Vector3 startingPos;

    float timer, xAngle;

    // Start is called before the first frame update
    void Start()
    {

        float z;

        // Set the starting variables
        camera = GameObject.Find("Main Camera");
        timer = 0;
        xAngle = this.transform.eulerAngles.x;

        startingPos = this.transform.localPosition;
        startingPos.x = -startingPos.z;
        startingPos.z = 0;
        startingPos.y *= -1;

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 cameraPosition, myPosition, direction, rotation, offset;
        Quaternion angle;
        RaycastHit hit;

        float degrees;

        // Get the camera position and the position of the sphere the text orbits
        cameraPosition = camera.transform.position;
        myPosition = this.transform.parent.position;

        // Get the direcvtion vector from the sphere to the camera, ignoring the y differences
        direction = cameraPosition - myPosition;
        direction.y = 0;

        // Calculate the angle and set the rotation of the text to it
        angle = Quaternion.FromToRotation(new Vector3(0, 0, -1), direction);
        degrees = angle.eulerAngles.y;
        rotation = new Vector3(180 - xAngle, degrees + 180, 0);

        // Rotate the offset of the text from the sphere centre so that it is in the correct position
        offset = Quaternion.Euler(rotation) * startingPos;

        this.transform.eulerAngles = rotation;
        this.transform.localPosition = offset;

        // Get the direction the camera is looking in
        cameraPosition = camera.transform.rotation * new Vector3(0, 0, 1);

        // Determine if the user is looking at an object
        if (Physics.Raycast(camera.transform.position, cameraPosition, out hit))
        {

            // If they are check if it is either the text or the sphere
            if (hit.collider.gameObject == this.gameObject || hit.collider.gameObject == this.transform.parent.gameObject)
            {

                // If they are increment the timer for scene change
                timer += Time.deltaTime;

                // Check if the user has stared at the text and/or the sphere for 5 seconds
                if (timer > 5)
                {

                    GameObject[] objects;

                    // If they have reset the timer and get all the movable objects
                    timer = 0;
                    objects = GameObject.FindGameObjectsWithTag("Object");

                    // Loop through all the moveable objects and reset them to their original positions
                    for (int i = 0; i < objects.Length; i++)
                    {

                        BasicObject script = objects[i].transform.GetComponent<BasicObject>();

                        script.resetObject();

                    }

                }

            }
            // If they aren't looking at either ensure the timer is set to 0
            else { timer = 0; }

        }
        // If they aren't looking at an object ensure the timer is set to 0
        else { timer = 0; }

    }

}
