using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelControl : MonoBehaviour
{

    float rotSpeed;
    public float wheelTempo = 1;
    public GameObject arrow;
    public GameObject eventBox; // prefab of eventBox
    public int eventCount;
    public int[] eventList;
    private float radius;



    // Start is called before the first frame update
    void Start()
    {
        radius = this.transform.localScale.x / 2;

        PlaceEventBoxes();

        float rotationalSpeed = wheelTempo * 360.0f;
        this.rotSpeed = rotationalSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
            
        //}

        // Rotate at rotSpeed degrees per second
        transform.RotateAround(this.transform.position, Vector3.forward, this.rotSpeed * Time.deltaTime);
    }

    void PlaceEventBoxes()
    {
        // Calculate total fraction of event intervals
        int intervalSum = SumArray(eventList);
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

            newObject.GetComponent<EventBoxScript>().angle = angle;
            


            // Optional: Set the new object as a child of the wheel GameObject
            newObject.transform.parent = transform;

            
            currAngle = angle;
        }

    }

    public int SumArray(int[] toBeSummed)
    {
        int sum = 0;
        foreach (int i in toBeSummed)
        {
            sum += i;
        }
        return sum;
    }
}
