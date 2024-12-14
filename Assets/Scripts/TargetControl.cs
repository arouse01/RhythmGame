using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;


public class TargetControl : MonoBehaviour
{

    public Collider beatZone;

    Animation anim;

    // Events to trigger GameController
    public static event Action OnContactStart;
    public static event Action OnContactEnd;
    public static event Action OnBeatZoneStart;
    public static event Action OnBeatZoneEnd;

    public Color beatZoneColorDefault;
    private Transform triangle;
    
    private GameObject[] boxes;  // to change box color as needed

    public float width = .25f;
    public float height = 3f;
    public float targetY = 3f;

    void Start()
    {
        triangle = transform.Find("Triangle");
        
        anim = triangle.GetComponent<Animation>();
        BuildTarget();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void BuildTarget()
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        

        // Create the mesh
        Mesh mesh = new Mesh();

        // Define the vertices (corners of the rectangle)
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-width / 2, targetY-height / 2, 0), // Bottom-left
            new Vector3(width / 2, targetY-height / 2, 0),  // Bottom-right
            new Vector3(-width / 2, targetY+height / 2, 0),  // Top-left
            new Vector3(width / 2, targetY+height / 2, 0)    // Top-right
        };

        // Define the triangles (two triangles make the rectangle)
        int[] triangles = new int[]
        {
            0, 2, 1, // First triangle (Bottom-left, Top-left, Bottom-right)
            1, 2, 3  // Second triangle (Bottom-right, Top-left, Top-right)
        };

        // Assign to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Recalculate bounds and normals for rendering
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // Assign the mesh to the MeshFilter
        meshFilter.mesh = mesh;

        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.mesh; // Assign the same mesh
    }

    // This method is called when another collider enters the trigger zone
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EventBox"))
        {
            OnContactStart?.Invoke();
            //GetComponent<Renderer>().material.color = Color.blue;
            triangle.GetComponent<Renderer>().material.color = Color.white;
            //other.GetComponent<Renderer>().material.color = Color.yellow;
        }
        else if (other.CompareTag("BeatZone"))
        {
            beatZone = other; 
            OnBeatZoneStart?.Invoke();
            
        }
            
       
    }

    // This method is called when another collider exits the trigger zone
    private void OnTriggerExit(Collider other)
    {
        //GetComponent<Renderer>().material.color = Color.white;
        if (other.CompareTag("EventBox"))
        {
            OnContactEnd?.Invoke();
            triangle.GetComponent<Renderer>().material.color = Color.blue;
            //other.GetComponent<SpriteRenderer>().SetColor("_Color", Color.black);
        }
        else if (other.CompareTag("BeatZone"))
        {
            OnBeatZoneEnd?.Invoke();
            other.GetComponent<Renderer>().material.color = beatZoneColorDefault;
;
        }

    }

    public void Bounce()
    {
        anim.Play("Bounce");
    }

}

//public class DrawMeshColliderBounds : MonoBehaviour
//{
//    void OnDrawGizmos()
//    {
//        MeshCollider meshCollider = GetComponent<MeshCollider>();
//        if (meshCollider != null)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawWireCube(meshCollider.bounds.center, meshCollider.bounds.size);
//        }
//    }
//}
