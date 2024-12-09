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
    public float beatZoneSize;
    public bool pause;

    public Color safeZoneColorDefault;
    public Color beatZoneColorDefault;
    public int gameLevel;

    private float radius = 0.5f;
    public int segments = 100;

    private EventBox[] boxes;
    
    // Start is called before the first frame update
    void Start()
    {
        CreateCircleMesh();
        //ResetWheel();
        //StartSpin();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
            
        //}

        if(pause)
        {

        } 
        else
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

    public void Clear()
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
    
    public void Reset()
    {
        pause = true;
        // Place event boxes first because their size/position is linked to the wheel size/position, so resizing the wheel after will resize the event boxes
        PlaceEventBoxes();
        boxes = FindObjectsOfType<EventBox>();
        Resize();
        this.transform.Rotate(0.0f, 0.0f, 0.0f, Space.Self);  // Reset wheel position  
    }

    void CreateCircleMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // Center vertex

        float angleIncrement = 360f / segments;
        // Generate the perimeter vertices
        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleIncrement);
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
        }

        // Generate the triangles (3 indices per triangle)
        for (int i = 0; i < segments; i++)
        {
            int startIndex = i + 1; // The first vertex of the triangle (start of the segment)
            int nextIndex = (i + 1) % segments + 1;  // The second vertex of the triangle (next segment)

            // Each triangle is made by the center point (0), the start vertex, and the next vertex
            triangles[i * 3] = 0;  // Center vertex
            triangles[i * 3 + 1] = startIndex;  // Current perimeter vertex
            triangles[i * 3 + 2] = nextIndex;  // Next perimeter vertex
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Recalculate normals for correct lighting
        mesh.RecalculateNormals();

        // Apply the mesh to the MeshFilter
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void Resize(float scale=4.0f)
    {
        
        if (gameLevel == 0)
        {
            scale = 4;
        }
        else if (gameLevel == 1)
        {
            scale = 50;
        }
        else if (gameLevel == 2)
        {
            scale = 20;
        }
        else if (gameLevel == 3)
        {
            scale = 4;
        }
        float yPos = -(scale - 4) / 2;
        transform.localScale = new Vector3(scale, scale, scale);
        transform.position = new Vector3(0.0f, yPos, 0.0f);
        this.transform.Rotate(0.0f, 0.0f, 0.0f, Space.Self);  // Reset wheel position
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
        int j = 0;
        foreach (int i in eventList)
        {
            j++;
            // Calculate the position on the circle
            float angle = currAngle + i * angleStep;

            // Instantiate the prefab at the calculated position
            float wheelY = transform.position[1];
            GameObject newObject = Instantiate(eventBox, new Vector2(0, wheelY), Quaternion.identity);
            newObject.name = "EventBox_" + j.ToString();
            newObject.GetComponent<EventBox>().angle = angle;
            
            newObject.transform.parent = transform;

            currAngle = angle;

            // set eventBox collider size too
            newObject.GetComponent<EventBox>().colliderSize = colliderSize;
            newObject.GetComponent<EventBox>().beatZoneSize = beatZoneSize;

            // set colors
            newObject.GetComponent<EventBox>().ResetColors(safeZoneColorDefault, beatZoneColorDefault);
        }
        //ResizeEventBoxes();
    }

    //public void ResizeEventBoxes()
    //{
    //    boxes = FindObjectsOfType<EventBox>();
    //    foreach (EventBox box in boxes)
    //    {
    //        box.ResetShapeWedge();
    //    }
    //}

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
