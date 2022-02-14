using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerTrigger : MonoBehaviour
{

    bool collision, check;

    GameObject[] objects;
    GameObject grabbed;

    // Start is called before the first frame update
    void Start()
    {

        collision = false;
        check = false;
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

                    grabbed = objects[i];
                    collision = true;
                    check = false;
                    i = objects.Length;

                }

            }

        }
        else { collision = false; }

    }

    public void setChecking(bool isChecking) { check = isChecking; }

    public bool isChecking() { return check; }

    public bool isCollided() { return collision; }

    public GameObject getGrabbed() { return grabbed; }

}