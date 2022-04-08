using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerTrigger : MonoBehaviour
{

    public GameObject otherHand, hand;

    bool collision, check, holding, reset, sharedHold;
    float grabTimer;

    GameObject[] objects;
    GameObject grabbedObject;
    Vector3 offset;
    Quaternion startQuart, currentQuart;
    Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {

        // Set the default values for the script
        collision = false;
        check = false;
        reset = false;
        sharedHold = false;
        grabTimer = 0;
        objects = GameObject.FindGameObjectsWithTag("Object");

    }

    void OnTriggerEnter(Collider other)
    {

        // Check if the hand is currently trying to grab
        if (check)
        {

            // If so then set collison to false to ensure no false positives
            collision = false;

            // Loop through all objects that are grabbable
            for (int i = 0; i < objects.Length; i++)
            {

                // Check if the collided object is a grabbable object
                if (other.gameObject == objects[i])
                {

                    // Get the rigidbody of either the object or if it is a segment of another object it's parent
                    if (objects[i].GetComponent<Rigidbody>() != null) { grabbedObject = objects[i]; }
                    else { grabbedObject = objects[i].transform.parent.gameObject; }

                    // Signal there has been a collison
                    collision = true;
                    // Stop checking for new collisions
                    check = false;
                    // And exit the loop
                    i = objects.Length;

                }

            }

        }
        else 
        {
            
            // If it isn't trying to grab thenensure that it signals no collisions
            if (collision) { collision = false; }

        }

    }

    // Return weather the collider is checking for collisons
    public bool isChecking() { return check; }

    // Set weather the collider is checking for collisons
    public void setChecking(bool set) { check = set; }

    // Return weather the hand just stopped grabbing
    public bool isReset() { return reset; }

    // Set weather the hand just stopped grabbing
    public void setReset(bool set) { reset = set; }

    public void IsHolding()
    {

        // Increment the timer for an attempt to grab
        grabTimer += Time.deltaTime;

        // Check if the hand is not grabbing something and they are still within the time limit to grab
        if (!holding && grabTimer < 1)
        {

            // If they are ensure that the collider is checking
            if (!check) { check = true; }

            // Check if a collision was detected
            if (collision)
            {


                FingerTrigger script = otherHand.GetComponent<FingerTrigger>();

                // If there was, check if the other hand is also grabbing something
                if (script.checkHolding())
                {

                    // If it is, check if it is the same object that this hand has collided with
                    // And if so signal that the hold is shared
                    if (script.getHeld() == grabbedObject) { sharedHold = true;  }

                }

                // Get the grabbed objects rigidbody
                rigidbody = grabbedObject.GetComponent<Rigidbody>();

                // Make it kinematic
                rigidbody.isKinematic = true;

                // Set the distance between the hand and object and store the hands current rotation
                offset = grabbedObject.transform.position - hand.transform.position;
                startQuart = hand.transform.rotation;
                currentQuart = startQuart;

                // Signal it is holding an object
                holding = true;

            }

        }
        // If not, check if it is holding an object
        else if (holding)
        {

            // Check if the hold is shared
            if (sharedHold)
            {

                FingerTrigger script = otherHand.GetComponent<FingerTrigger>();

                // Check if the other hand is still holding the object
                if (script.checkHolding())
                {

                    // If so get the current offset between the hand and the object
                    Vector3 currentOffset = grabbedObject.transform.position - hand.transform.position;

                    // Rotate the object so that the same point on the object is pointing towards the hand
                    grabbedObject.transform.rotation = Quaternion.FromToRotation(offset, currentOffset);

                }
                // If it isn't signal the hold is no longer shared
                else { sharedHold = false; }

            }

            // Check if the hold is shared
            if (!sharedHold)
            {

                Vector3 position;

                // Get the change in rotation since the object was first grabbed
                currentQuart = hand.transform.rotation;
                currentQuart = startQuart * Quaternion.Inverse(currentQuart);

                // Rotate the original offset by the change in rotation
                position = Quaternion.Inverse(currentQuart) * offset;

                // Set the object position to be the hand position with the rotated offset
                grabbedObject.transform.position = hand.transform.position + position;

                // Ensure the rigid body is kinematic
                rigidbody = grabbedObject.GetComponent<Rigidbody>();

                rigidbody.isKinematic = true;

            }

        }
        // If time has run out and it hasn't grabbed an object signal to stop checking for collisions
        else
        {

            if (check) { check = false; }

        }

    }

    public void stopHolding()
    {

        // Check if an object was grabbed
        if (grabbedObject)
        {

            Rigidbody rigidbody = grabbedObject.GetComponent<Rigidbody>();

            // If so then signal to reset the timer
            reset = true;
            // Set the object to no longer be kinematic
            rigidbody.isKinematic = false;
            // Set the grabbed object to null
            grabbedObject = null;
            // And ensure that share hold is false
            sharedHold = false;

        }

        // Ensure holdiong is false
        holding = false;
        // Anbd reset the grab timer
        grabTimer = 0;

        // Signal to stop checking if it hasn't already
        if (check) { check = false; }

    }

    // Dewtermine if the hand is holding anything
    public bool checkHolding() { return holding; }

    // Get the object the hand is holding
    public GameObject getHeld() { return grabbedObject; }

}