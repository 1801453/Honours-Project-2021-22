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

        cameraPosition = camera.transform.position;
        myPosition = this.transform.parent.position;

        direction = cameraPosition - myPosition;
        direction.y = 0;

        angle = Quaternion.FromToRotation(new Vector3(0, 0, -1), direction);
        degrees = angle.eulerAngles.y;
        rotation = new Vector3(180 - xAngle, degrees + 180, 0);

        offset = Quaternion.Euler(rotation) * startingPos;

        this.transform.eulerAngles = rotation;
        this.transform.localPosition = offset;

        cameraPosition = camera.transform.rotation * new Vector3(0, 0, 1);

        if (Physics.Raycast(camera.transform.position, cameraPosition, out hit))
        {

            if (hit.collider.gameObject == this.gameObject || hit.collider.gameObject == this.transform.parent.gameObject)
            {

                timer += Time.deltaTime;

                if (timer > 5)
                {

                    GameObject[] objects;

                    timer = 0;
                    objects = GameObject.FindGameObjectsWithTag("Object");

                    for (int i = 0; i < objects.Length; i++)
                    {

                        BasicObject script = objects[i].transform.GetComponent<BasicObject>();

                        script.resetObject();

                    }

                }

            }

        }
        else { timer = 0; }

    }

}
