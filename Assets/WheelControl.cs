using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelControl : MonoBehaviour
{

    float rotSpeed = 0;
    public float maxSpeed = 180;
    public GameObject arrow;
    // Start is called before the first frame update
    void Start()
    {
        this.rotSpeed = maxSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
            
        //}

        transform.RotateAround(this.transform.position, Vector3.forward, this.rotSpeed * Time.deltaTime);
    }
}
