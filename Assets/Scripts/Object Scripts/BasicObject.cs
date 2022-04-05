using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicObject : MonoBehaviour
{

    float despawnTimer, kinectTimer;
    bool kinematic;

    Vector3 startLoc, intermediatePos, priorPos, velocity;
    Quaternion startRot;
    Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {

        // Set starting values
        velocity = new Vector3(0, 0, 0);
        despawnTimer = 0;
        kinectTimer = 0;
        kinematic = false;

        // Determine if this object has a rigidbody or if it is a child of a rigid body
        if (this.gameObject.GetComponent<Rigidbody>() != null) 
        {

            // If it has a rigidbody get information on it's starting orientation and rigidbody
            startLoc = this.transform.localPosition;
            startRot = this.transform.rotation;
            priorPos = this.transform.position;
            rigidbody = this.gameObject.GetComponent<Rigidbody>(); 
        
        }
        else
        {

            // If it doesn't get information on it's parent's orientation and rigidbody
            startLoc = this.transform.parent.transform.localPosition;
            startRot = this.transform.parent.transform.rotation;
            priorPos = this.transform.parent.transform.position;
            rigidbody = this.transform.parent.gameObject.GetComponent<Rigidbody>();

        }

        // Set the intermediate position
        intermediatePos = priorPos;

    }

    // Update is called once per frame
    void Update()
    {

        float dt;

        // Increment the timers that happen every frame
        dt = Time.deltaTime;
        kinectTimer += dt;

        // Check if the rigidbody is kinematic
        if (rigidbody.isKinematic)
        {

            // If it is ensure the variable relating to that it is updated
            if (!kinematic) { kinematic = true; }

        }
        else
        {

            Vector3 position;

            // If it isn't kinematic get the position of the rigidbody
            if (this.gameObject.GetComponent<Rigidbody>() != null) { position = this.transform.position; }
            else { position = this.transform.parent.transform.position; }

            // Check if the position is out of bounds
            if (position.x < -1.5f || position.x > 1.8f || position.z < -1.3f || position.z > 1.3f)
            {

                // If it is increment the despawn timer
                despawnTimer += dt;

                // Check if it has been out of bounds for at least 10 seconds
                if (despawnTimer >= 10)
                {

                    // If it has reset the timer and reset the object
                    despawnTimer = 0;

                    resetObject();

                }

            }

            // Check if it has stopped being kinematic
            if (kinematic)
            {

                // Calculate the velocity based on it's movements over the last 2 kinect frames
                position = position - priorPos;
                velocity = position / (kinectTimer + (1.0f / 30.0f));
                rigidbody.velocity = velocity;
                // Signal that it is no longer kinematic
                kinematic = false;

            }

        }

        // Check if a kinect frame has elapsed
        if (kinectTimer >= 1.0f / 30.0f)
        {

            // If it has update the prior position
            priorPos = intermediatePos;

            // Update the intermediate position
            if (this.gameObject.GetComponent<Rigidbody>() != null) { intermediatePos = this.transform.position; }
            else { intermediatePos = this.transform.parent.transform.position; }

            // Reset the timer
            kinectTimer = 0;
        
        }

    }

    // Function to reset object transform
    public void resetObject()
    {

        // Reset the position and rotation of the rigidbody object with it's starting transform
        if (this.gameObject.GetComponent<Rigidbody>() != null)
        {

            this.transform.localPosition = startLoc;
            this.transform.rotation = startRot;

        }
        else
        {

            this.transform.parent.transform.localPosition = startLoc;
            this.transform.parent.transform.rotation = startRot;

        }

        // Set the velocity to 0
        rigidbody.velocity = new Vector3(0, 0, 0);

    }

}
