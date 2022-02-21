using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicObject : MonoBehaviour
{

    float despawnTimer, kinectTimer, velocityLockTimer;
    bool kinematic;

    Vector3 startLoc, intermediatePos, priorPos, velocity;
    Rigidbody rigidbody;

    // Start is called before the first frame update
    void Start()
    {

        startLoc = this.transform.localPosition;
        priorPos = this.transform.position;
        intermediatePos = priorPos;
        velocity = new Vector3(0, 0, 0);
        rigidbody = this.gameObject.GetComponent<Rigidbody>();
        despawnTimer = 0;
        kinectTimer = 0;
        velocityLockTimer = 0;
        kinematic = false;

    }

    // Update is called once per frame
    void Update()
    {

        float dt;

        dt = Time.deltaTime;
        kinectTimer += dt;

        if (rigidbody.isKinematic)
        {

            if (!kinematic) { kinematic = true; }

        }
        else
        {

            Vector3 position = this.transform.position;

            if (position.x < -1.5f)
            {

                despawnTimer += dt;

                if (despawnTimer >= 10)
                {

                    this.transform.localPosition = startLoc;
                    despawnTimer = 0;
                    rigidbody.velocity = new Vector3(0, 0, 0);

                }

            }

            if (kinematic)
            {

                position = position - priorPos;
                velocity = position / (kinectTimer + (1.0f / 30.0f));
                rigidbody.velocity = velocity;

                velocityLockTimer = 0;
                kinematic = false;

            }

        }

        if (kinectTimer >= 1.0f / 30.0f)
        {

            priorPos = intermediatePos;
            intermediatePos = this.transform.position;
            kinectTimer = 0;
        
        }

    }

}
