using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelControl : MonoBehaviour
{

    float rotSpeed;
    public float wheelTempo = 1;
    public GameObject arrow;
    public GameObject eventBox; // prefab of eventBox
    public float[] eventList;
    public float colliderSize;
    public bool pause;

    private EventBox[] boxes;

    // Start is called before the first frame update
    void Start()
    {
        //ResetWheel();
        //StartSpin();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
            
        //}

        if(pause)
        {

        } else
        {
            // Rotate at rotSpeed degrees per second
            transform.RotateAround(this.transform.position, Vector3.forward, this.rotSpeed * Time.deltaTime);
        }
        
    }

    public void StartSpin()
    {
        
        float rotationalSpeed = wheelTempo * 360.0f;
        this.rotSpeed = rotationalSpeed;

        pause = false;
    }

    public void ClearWheel()
    {
        // remove existing eventBoxes
        if (boxes != null)
        {
            foreach (EventBox box in boxes)
            {
                box.SelfDestruct();
            }
        }
    }
    
    public void ResetWheel()
    {
        // reset wheel and create new boxes
        this.transform.Rotate(0.0f, 0.0f, 0.0f, Space.Self);  // Reset wheel position
        PlaceEventBoxes();
        boxes = FindObjectsOfType<EventBox>();
        pause = true;
    }
    
    public void StopSpin()
    {
        pause = true;
    }
    
    void PlaceEventBoxes()
    {
        // Calculate total fraction of event intervals
        float intervalSum = SumArray(eventList);
        float angleStep = 360 / intervalSum;
        //// Calculate the angle between each object in radians
        //float angleStep = 2 * Mathf.PI / eventCount;
        float currAngle = 0;

        foreach (int i in eventList)
        {
            // Calculate the position on the circle
            float angle = currAngle + i * angleStep;

            // Instantiate the prefab at the calculated position
            GameObject newObject = Instantiate(eventBox, new Vector2(0, 0), Quaternion.identity);

            newObject.GetComponent<EventBox>().angle = angle;
            
            newObject.transform.parent = transform;

            
            currAngle = angle;

            // set eventBox collider size too
            newObject.GetComponent<EventBox>().colliderSize = colliderSize;
            // set color to black
            newObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0, 0, 0));
        }
        //ResizeEventBoxes();
    }

    public void ResizeEventBoxes()
    {
        boxes = FindObjectsOfType<EventBox>();
        foreach (EventBox box in boxes)
        {
            box.ResetShape();
        }
    }

    public float SumArray(float[] toBeSummed)
    {
        float sum = 0;
        foreach (float i in toBeSummed)
        {
            sum += i;
        }
        return sum;
    }
}
