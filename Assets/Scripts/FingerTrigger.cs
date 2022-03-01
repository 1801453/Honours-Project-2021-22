using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerTrigger : MonoBehaviour
{

    bool collision, check, holding, reset;
    float grabTimer;

    GameObject[] objects;
    GameObject grabbedObject;
    Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {

        collision = false;
        check = false;
        reset = false;
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

                    grabbedObject = objects[i];
                    collision = true;
                    check = false;
                    i = objects.Length;

                }

            }

        }
        else { collision = false; }

    }

    public bool isChecking() { return check; }

    public void setChecking(bool set) { check = set; }

    public bool isReset() { return reset; }

    public void setReset(bool set) { reset = set; }

    public void IsHolding()
    {

        if (!holding && grabTimer < 1)
        {

            if (!check) { check = true; }

            if (collision)
            {

                Rigidbody rigidbody = grabbedObject.GetComponent<Rigidbody>();

                rigidbody.isKinematic = true;

                offset = grabbedObject.transform.position - this.transform.position;

                holding = true;

            }

        }
        else if (holding)
        {

            grabbedObject.transform.position = this.transform.position + offset;

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

        }

        holding = false;
        grabTimer = 0;

        if (check) { check = false; }

    }

}