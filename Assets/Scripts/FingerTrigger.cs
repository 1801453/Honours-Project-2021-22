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

        collision = false;
        check = false;
        reset = false;
        sharedHold = false;
        grabTimer = 0;
        objects = GameObject.FindGameObjectsWithTag("Object");

    }

    void OnTriggerEnter(Collider other)
    {

        if (check)
        {

            collision = false;

            for (int i = 0; i < objects.Length; i++)
            {

                if (other.gameObject == objects[i])
                {

                    if (objects[i].GetComponent<Rigidbody>() != null) { grabbedObject = objects[i]; }
                    else { grabbedObject = objects[i].transform.parent.gameObject; }

                    collision = true;
                    check = false;
                    i = objects.Length;

                }

            }

        }
        else 
        {
            
            if (collision) { collision = false; }

        }

    }

    public bool isChecking() { return check; }

    public void setChecking(bool set) { check = set; }

    public bool isReset() { return reset; }

    public void setReset(bool set) { reset = set; }

    public void IsHolding()
    {

        grabTimer += Time.deltaTime;

        if (!holding && grabTimer < 1)
        {

            if (!check) { check = true; }

            if (collision)
            {


                FingerTrigger script = otherHand.GetComponent<FingerTrigger>();

                if (script.checkHolding())
                {

                    if (script.getHeld() == grabbedObject) { sharedHold = true;  }

                }

                rigidbody = grabbedObject.GetComponent<Rigidbody>();

                rigidbody.isKinematic = true;

                offset = grabbedObject.transform.position - hand.transform.position;
                startQuart = hand.transform.rotation;
                currentQuart = startQuart;

                holding = true;

            }

        }
        else if (holding)
        {

            if (sharedHold)
            {

                FingerTrigger script = otherHand.GetComponent<FingerTrigger>();

                if (script.checkHolding())
                {

                    Vector3 currentOffset = grabbedObject.transform.position - hand.transform.position;

                    grabbedObject.transform.rotation = Quaternion.FromToRotation(offset, currentOffset);

                }
                else { sharedHold = false; }

            }

            if (!sharedHold)
            {

                Vector3 position;

                currentQuart = hand.transform.rotation;
                currentQuart = startQuart * Quaternion.Inverse(currentQuart);

                position = Quaternion.Inverse(currentQuart) * offset;

                grabbedObject.transform.position = hand.transform.position + position;

                rigidbody = grabbedObject.GetComponent<Rigidbody>();

                rigidbody.isKinematic = true;

            }

        }
        else
        {

            if (check) { check = false; }

        }

    }

    public void stopHolding()
    {

        if (grabbedObject)
        {

            Rigidbody rigidbody = grabbedObject.GetComponent<Rigidbody>();

            reset = true;
            rigidbody.isKinematic = false;
            grabbedObject = null;
            sharedHold = false;

        }

        holding = false;
        grabTimer = 0;

        if (check) { check = false; }

    }

    public bool checkHolding() { return holding; }

    public GameObject getHeld() { return grabbedObject; }

}