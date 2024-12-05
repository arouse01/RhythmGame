using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class EventBox : MonoBehaviour
{
    public float angle = 0;
    public float colliderSize = 5;
    public float beatZoneSize = 2;
    public SpriteShapeController safeZoneShapeController;
    public SpriteShapeController beatZoneShapeController;


    private GameObject wheel;  // parent wheel object, for getting radius
    private float circRadius;
    private Transform beatZone;
    private Transform beatMarker;

    // Start is called before the first frame update
    void Start()
    {
        safeZoneShapeController = GetComponent<SpriteShapeController>();
        wheel = this.transform.parent.gameObject;

        beatZone = transform.Find("BeatZone");
        beatZoneShapeController = beatZone.GetComponent<SpriteShapeController>();
        beatMarker = transform.Find("BeatMarker");
        circRadius = wheel.transform.localScale.x / 2;
        //circRadius = 2;
        ResetShapeWedge();

        float angleRad = angle * Mathf.Deg2Rad;
        transform.Rotate(0.0f, 0.0f, -angle, Space.Self);
        
        // the localScale gets set to 0.02 after the first round (maybe because of the wheel resizing between rounds?)
        transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ResetShapeWedge()
    {
        float h = 2 + 1.0f * 2/circRadius; // height of the wedge
        //// safeZone reshape
        // redefine spline points to be a wedge
        Spline spline = safeZoneShapeController.spline;
        spline.Clear();  // clear existing points

        float widthRad = colliderSize * Mathf.Deg2Rad;

        Vector2[] points = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(h * Mathf.Sin(widthRad/2), h * Mathf.Cos(widthRad/2)),
            new Vector2(0, (h)),
            new Vector2(h * Mathf.Sin(-widthRad/2), h * Mathf.Cos(-widthRad/2))
        };

        if (colliderSize <= 2)
        {
            // Tiny wedges don't render properly because the points are too close together, so skip the middle of the wedge top
            points = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(h * Mathf.Sin(widthRad/2), h * Mathf.Cos(widthRad/2)),
                new Vector2(h * Mathf.Sin(-widthRad/2), h * Mathf.Cos(-widthRad/2))
            };
        }
       
        // Add the new points to the spline
        for (int i = 0; i < points.Length; i++)
        {
            spline.InsertPointAt(i, points[i]);
            if (i == 2 && points.Length > 3)
            {
                float tangentLength = (2.0f * Mathf.Sin(widthRad / 2));
                spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                spline.SetLeftTangent(i, new Vector2(tangentLength, 0));   // Adjust this vector to control curvature
                spline.SetRightTangent(i, new Vector2(-tangentLength, 0));   // Adjust this vector to control curvature
            }
            else
            {
                spline.SetTangentMode(i, ShapeTangentMode.Linear);
            }
        }
        safeZoneShapeController.spline.isOpenEnded = false;
        // update the collider too
        PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
        polygonCollider.SetPath(0, points);


        //// update BeatZone
        Spline bzSpline = beatZoneShapeController.spline;
        bzSpline.Clear();  // clear existing points

        float bzWidthRad = beatZoneSize * Mathf.Deg2Rad;

        Vector2[] bzPoints = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(h * Mathf.Sin(bzWidthRad/2), h * Mathf.Cos(bzWidthRad/2)),
            new Vector2(0, h),
            new Vector2(h * Mathf.Sin(-bzWidthRad/2), h * Mathf.Cos(-bzWidthRad/2))
        };

        if (beatZoneSize <= 2)
        {
            // Tiny wedges don't render properly because the points are too close together, so skip the middle of the wedge top
            bzPoints = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(h * Mathf.Sin(bzWidthRad/2), h * Mathf.Cos(bzWidthRad/2)),
                new Vector2(h * Mathf.Sin(-bzWidthRad/2), h * Mathf.Cos(-bzWidthRad/2))
            };
        }

        // Add the new points to the spline
        for (int i = 0; i < bzPoints.Length; i++)
        {
            //Vector2 localPoint = safeZoneShapeController.transform.InverseTransformPoint(bzPoints[i]);
            bzSpline.InsertPointAt(i, bzPoints[i]);
            if (i == 2 && bzPoints.Length > 3)
            {
                float tangentLength = (2.0f * Mathf.Sin(bzWidthRad / 2));
                bzSpline.SetTangentMode(i, ShapeTangentMode.Continuous);
                bzSpline.SetLeftTangent(i, new Vector2(tangentLength, 0));   // Adjust this vector to control curvature
                bzSpline.SetRightTangent(i, new Vector2(-tangentLength, 0));   // Adjust this vector to control curvature
            }
            else
            {
                bzSpline.SetTangentMode(i, ShapeTangentMode.Linear);
            }
        }

        beatZoneShapeController.spline.isOpenEnded = false;
        // update the collider too
        PolygonCollider2D bzPolygonCollider = beatZone.GetComponent<PolygonCollider2D>();
        bzPolygonCollider.SetPath(0, bzPoints);


        //// update BeatMarker
        float parentScaleX = transform.parent != null ? transform.parent.localScale.x : 1;
        float bmWidth = 0.1f / parentScaleX;
        beatMarker.transform.localScale = new Vector3(bmWidth, h, 0.0f);
        beatMarker.transform.localPosition = new Vector3(0.0f, h/2, 0.0f);
        //// update the collider too
        //BoxCollider2D bmCollider = beatMarker.GetComponent<PolygonCollider2D>();
        //bmCollider.SetPath(0, bzPoints);
    }

    public void SelfDestruct()
    {
        Destroy(gameObject);
    }

    public void ResetColors(Color safeSpaceColor, Color beatZoneColor)
    {
        beatZone = transform.Find("BeatZone");
        transform.GetComponent<SpriteShapeRenderer>().color = safeSpaceColor;
        beatZone.GetComponent<Renderer>().material.color = beatZoneColor;
    }
}

