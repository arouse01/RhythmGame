using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class EventBox : MonoBehaviour
{
    public float angle = 0;
    public float colliderSize = 5;
    public float beatZoneSize = 2;
    public float outerRadius = 2f;
    public float innerRadius = 1f;
    public int segments = 40;
    public GameObject parentObject; // The GameObject to which the segment will be attached
    public Material material; // The material to apply to the segment
    //public SpriteShapeController safeZoneShapeController;
    //public SpriteShapeController beatZoneShapeController;


    private GameObject wheel;  // parent wheel object, for getting radius
    private float circRadius;
    private Transform beatZone;
    private Transform beatMarker;

    // Start is called before the first frame update
    void Start()
    {
        //safeZoneShapeController = GetComponent<SpriteShapeController>();
        wheel = transform.parent.gameObject;
        circRadius = wheel.transform.localScale.x / 2f;

        beatZone = transform.Find("BeatZone");
        //beatZoneShapeController = beatZone.GetComponent<SpriteShapeController>();
        beatMarker = transform.Find("BeatMarker");

        float h = 2f + 1.0f * 2 / circRadius; // height of the wedge

        // create safe zone
        CreateWedge(colliderSize, h+ 1f / circRadius, innerRadius, gameObject);

        // create beat zone
        CreateWedge(beatZoneSize, h, innerRadius, beatZone.gameObject);

        //// update BeatMarker
        float parentScaleX = transform.parent != null ? transform.parent.localScale.x : 1;
        float bmWidth = 0.1f / parentScaleX;
        float markerHeight = h - innerRadius;
        float markerY = (h + innerRadius) / 2f;
        beatMarker.transform.localScale = new Vector3(bmWidth, markerHeight, 0.0f);
        beatMarker.transform.localPosition = new Vector3(markerY, 0f, 0.0f);
        //beatMarker.transform.localPosition = new Vector3(0.0f, h / 2, 0.0f);

        // set angle
        float angleRad = angle * Mathf.Deg2Rad;
        transform.Rotate(0.0f, 0.0f, angle, Space.Self);
        
        // the localScale gets set to 0.02 after the first round (maybe because of the wheel resizing between rounds?)
        transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateWedge(float wedgeWidth, float top, float bottom, GameObject targetObject)
    {
        // Create and assign mesh
        Mesh mesh = new Mesh();

        float angleStep = wedgeWidth / segments;
        float baseAngle = 90f;
        // Vertices array
        Vector3[] vertices = new Vector3[(segments + 1) * 2]; // +2 for the center points


        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = Mathf.Deg2Rad * (baseAngle + (i * angleStep) - wedgeWidth / 2);
            float x = Mathf.Sin(currentAngle);
            float y = Mathf.Cos(currentAngle);

            // Outer arc vertex
            vertices[i] = new Vector3(x * top, y * top, 0);

            // Inner arc vertex
            vertices[i + segments + 1] = new Vector3(x * bottom, y * bottom, 0);
        }

        // Triangles
        int[] triangles = new int[segments * 6]; // 2 triangles per segment
        int ti = 0;
        for (int i = 0; i < segments; i++)
        {
            int outer0 = i;
            int outer1 = i + 1;
            int inner0 = i + segments + 1;
            int inner1 = i + segments + 2;

            // First triangle
            triangles[ti++] = outer0;
            triangles[ti++] = inner0;
            triangles[ti++] = outer1;

            // Second triangle
            triangles[ti++] = outer1;
            triangles[ti++] = inner0;
            triangles[ti++] = inner1;
        }
        //// The center and outer center point for mesh creation
        //vertices[0] = Vector3.zero;
        //vertices[vertexCount + 1] = new Vector3(0f, 0f, 0f);

        //// Calculate angle in radians


        

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshCollider meshCollider = targetObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.mesh; // Assign the same mesh

        //MeshRenderer meshRenderer = targetObject.GetComponent<MeshRenderer>();
        //meshRenderer.material = material;

    }



    void AttachToParent()
    {
        // If a parent is specified, attach the segment to it and set the position and rotation.
        if (parentObject != null)
        {
            transform.SetParent(parentObject.transform);
            transform.localPosition = Vector3.zero; // Optional: adjust position
            transform.localRotation = Quaternion.identity; // Optional: adjust rotation
        }
    }

    //public void ResetShapeWedge()
    //{
        
    //    float hb = Mathf.Max(0, 1 + 1.0f * 2 / circRadius); // bottom of the wedge
    //    ////// safeZone reshape
        
    //    //// redefine spline points to be a wedge
    //    //Spline spline = safeZoneShapeController.spline;
    //    //spline.Clear();  // clear existing points

    //    //float widthRad = colliderSize * Mathf.Deg2Rad;

    //    //Vector2[] points = new Vector2[]
    //    //{
    //    //    new Vector2(hb * Mathf.Sin(-widthRad/2), hb * Mathf.Cos(-widthRad/2)),
    //    //    new Vector2(hb * Mathf.Sin(widthRad/2), hb * Mathf.Cos(widthRad/2)),
    //    //    new Vector2(h * Mathf.Sin(widthRad/2), h * Mathf.Cos(widthRad/2)),
    //    //    new Vector2(0, (h)),
    //    //    new Vector2(h * Mathf.Sin(-widthRad/2), h * Mathf.Cos(-widthRad/2))
    //    //};

    //    //if (colliderSize <= 2)
    //    //{
    //    //    // Tiny wedges don't render properly because the points are too close together, so skip the middle of the wedge top
    //    //    points = new Vector2[]
    //    //    {
    //    //        new Vector2(hb * Mathf.Sin(-widthRad/2), hb * Mathf.Cos(-widthRad/2)),
    //    //        new Vector2(hb * Mathf.Sin(widthRad/2), hb * Mathf.Cos(widthRad/2)),
    //    //        new Vector2(h * Mathf.Sin(widthRad/2), h * Mathf.Cos(widthRad/2)),
    //    //        new Vector2(h * Mathf.Sin(-widthRad/2), h * Mathf.Cos(-widthRad/2))
    //    //    };
    //    //}
       
    //    //// Add the new points to the spline
    //    //for (int i = 0; i < points.Length; i++)
    //    //{
    //    //    spline.InsertPointAt(i, points[i]);
    //    //    if (i == 3 && points.Length > 3)
    //    //    {
    //    //        float tangentLength = (2.0f * Mathf.Sin(widthRad / 2));
    //    //        spline.SetTangentMode(i, ShapeTangentMode.Continuous);
    //    //        spline.SetLeftTangent(i, new Vector2(tangentLength, 0));   // Adjust this vector to control curvature
    //    //        spline.SetRightTangent(i, new Vector2(-tangentLength, 0));   // Adjust this vector to control curvature
    //    //    }
    //    //    else
    //    //    {
    //    //        spline.SetTangentMode(i, ShapeTangentMode.Linear);
    //    //    }
    //    //}
    //    //safeZoneShapeController.spline.isOpenEnded = false;
    //    //// update the collider too
    //    //PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
    //    //polygonCollider.SetPath(0, points);


    //    //// update BeatZone
    //    Spline bzSpline = beatZoneShapeController.spline;
    //    bzSpline.Clear();  // clear existing points

    //    float bzWidthRad = beatZoneSize * Mathf.Deg2Rad;

    //    Vector2[] bzPoints = new Vector2[]
    //    {
    //        new Vector2(hb * Mathf.Sin(-bzWidthRad/2), hb * Mathf.Cos(-bzWidthRad/2)),
    //        new Vector2(hb * Mathf.Sin(bzWidthRad/2), hb * Mathf.Cos(bzWidthRad/2)),
    //        new Vector2(h * Mathf.Sin(bzWidthRad/2), h * Mathf.Cos(bzWidthRad/2)),
    //        new Vector2(0, h),
    //        new Vector2(h * Mathf.Sin(-bzWidthRad/2), h * Mathf.Cos(-bzWidthRad/2))
    //    };

    //    if (beatZoneSize <= 2)
    //    {
    //        // Tiny wedges don't render properly because the points are too close together, so skip the middle of the wedge top
    //        bzPoints = new Vector2[]
    //        {
    //            new Vector2(hb * Mathf.Sin(-bzWidthRad/2), hb * Mathf.Cos(-bzWidthRad/2)),
    //            new Vector2(hb * Mathf.Sin(bzWidthRad/2), hb * Mathf.Cos(bzWidthRad/2)),
    //            new Vector2(h * Mathf.Sin(bzWidthRad/2), h * Mathf.Cos(bzWidthRad/2)),
    //            new Vector2(h * Mathf.Sin(-bzWidthRad/2), h * Mathf.Cos(-bzWidthRad/2))
    //        };
    //    }

    //    // Add the new points to the spline
    //    for (int i = 0; i < bzPoints.Length; i++)
    //    {
    //        //Vector2 localPoint = safeZoneShapeController.transform.InverseTransformPoint(bzPoints[i]);
    //        bzSpline.InsertPointAt(i, bzPoints[i]);
    //        if (i == 3 && bzPoints.Length > 3)
    //        {
    //            float tangentLength = (2.0f * Mathf.Sin(bzWidthRad / 2));
    //            bzSpline.SetTangentMode(i, ShapeTangentMode.Continuous);
    //            bzSpline.SetLeftTangent(i, new Vector2(tangentLength, 0));   // Adjust this vector to control curvature
    //            bzSpline.SetRightTangent(i, new Vector2(-tangentLength, 0));   // Adjust this vector to control curvature
    //        }
    //        else
    //        {
    //            bzSpline.SetTangentMode(i, ShapeTangentMode.Linear);
    //        }
    //    }

    //    beatZoneShapeController.spline.isOpenEnded = false;
    //    // update the collider too
    //    PolygonCollider2D bzPolygonCollider = beatZone.GetComponent<PolygonCollider2D>();
    //    bzPolygonCollider.SetPath(0, bzPoints);


        
    //}

    public void SelfDestruct()
    {
        Destroy(gameObject);
    }

    public void ResetColors(Color safeSpaceColor, Color beatZoneColor)
    {
        beatZone = transform.Find("BeatZone");
        transform.GetComponent<MeshRenderer>().material.color = safeSpaceColor;
        beatZone.GetComponent<MeshRenderer>().material.color = beatZoneColor;
    }
    
   
}