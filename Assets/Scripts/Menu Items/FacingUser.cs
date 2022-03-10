using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FacingUser : MonoBehaviour
{

    GameObject camera;
    Vector3 baseOffset;

    float timer;
    public string targetScene;

    // Start is called before the first frame update
    void Start()
    {

        float z;

        camera = GameObject.Find("Main Camera");
        timer = 0;
        baseOffset = this.transform.localPosition;

        z = baseOffset.z;
        baseOffset.z = baseOffset.y;
        baseOffset.x = baseOffset.x;

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
        rotation = new Vector3(90, degrees + 180, 0);

        offset = Quaternion.Euler(rotation) * new Vector3(0.51f, 0, 0);

        this.transform.eulerAngles = rotation;
        this.transform.localPosition = offset;

        cameraPosition = camera.transform.rotation * new Vector3(0, 0, 1);

        if (Physics.Raycast(camera.transform.position, cameraPosition, out hit))
        {

            if (hit.collider.gameObject == this.gameObject || hit.collider.gameObject == this.transform.parent.gameObject)
            {

                timer += Time.deltaTime;

                if (timer > 5) { SceneManager.LoadScene(targetScene); }

            }

        }
        else { timer = 0; }

    }

}
