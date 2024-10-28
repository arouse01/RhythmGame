using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class WheelLineDetector : MonoBehaviour
{
    public GameObject target;

    Animator anim;
    
    void Start()
    {
        anim = target.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Clicked!");
            anim.SetTrigger("Active");
        }

    }

    // This method is called when another collider enters the trigger zone
    private void OnTriggerEnter2D(Collider2D other)
    {
        //GetComponent<Renderer>().material.color = Color.blue;
        target.GetComponent<Renderer>().material.color = Color.blue;
        other.GetComponent<Renderer>().material.color = Color.yellow;
        // Debug.Log("Trigger triggered!");
    }

    // This method is called when another collider exits the trigger zone
    private void OnTriggerExit2D(Collider2D other)
    {
        //GetComponent<Renderer>().material.color = Color.white;
        target.GetComponent<Renderer>().material.color = Color.white;
        other.GetComponent<Renderer>().material.color = Color.white;
        // Debug.Log("Trigger exited!");

    }
}

