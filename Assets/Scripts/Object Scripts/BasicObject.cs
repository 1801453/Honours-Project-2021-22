using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicObject : MonoBehaviour
{

    float despawnTimer;

    Vector3 startLoc;

    // Start is called before the first frame update
    void Start()
    {

        startLoc = this.transform.localPosition;
        despawnTimer = 0;

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 position = this.transform.position;

        if (position.x < -1.5f)
        {

            despawnTimer += Time.deltaTime;

            if (despawnTimer >= 10)
            {

                this.transform.localPosition = startLoc;
                despawnTimer = 0;

            }

        }
        
    }

}
