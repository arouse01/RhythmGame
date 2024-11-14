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

    // Start is called before the first frame update
    void Start()
    {
        safeZoneShapeController = GetComponent<SpriteShapeController>();
        wheel = this.transform.parent.gameObject;

        beatZone = transform.Find("BeatZone");
        beatZoneShapeController = beatZone.GetComponent<SpriteShapeController>();
        circRadius = wheel.transform.localScale.x / 2;
        
        ResetShape();

        float angleRad = angle * Mathf.Deg2Rad;
        //float posX = Mathf.Sin(angleRad) * circRadius;
        //float posY = Mathf.Cos(angleRad) * circRadius;
        //this.transform.position = new Vector3(posX, posY, 0.0F);
        this.transform.Rotate(0.0f, 0.0f, -angle, Space.Self);

        // update collider size

        //GameObject childBox = this.gameObject.transform.GetChild(0).gameObject;
        
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ResetShape()
    {
        float h = 1.0f;
        //// safeZone reshape
        // redefine spline points to be a wedge
        Spline spline = safeZoneShapeController.spline;
        spline.Clear();  // clear existing points

        float widthRad = colliderSize * Mathf.Deg2Rad;

        Vector2[] points = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(Mathf.Sin(widthRad/2) * (circRadius+h), Mathf.Cos(widthRad/2) * (circRadius+h)),
            new Vector2(0, (circRadius+h)),
            new Vector2(Mathf.Sin(-widthRad/2) * (circRadius+h), Mathf.Cos(-widthRad/2) * (circRadius+h))
        };

        if (colliderSize < 2)
        {
            // Tiny wedges don't render properly because the points are too close together, so skip the middle of the wedge top
            points = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(Mathf.Sin(widthRad/2) * (circRadius+h), Mathf.Cos(widthRad/2) * (circRadius+h)),
                new Vector2(Mathf.Sin(-widthRad/2) * (circRadius+h), Mathf.Cos(-widthRad/2) * (circRadius+h))
            };
        }
       
        // Add the new points to the spline
        for (int i = 0; i < points.Length; i++)
        {
            spline.InsertPointAt(i, points[i]);
            if (i == 2 && colliderSize > 1)
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
            new Vector2(Mathf.Sin(bzWidthRad/2) * (circRadius+h), Mathf.Cos(bzWidthRad/2) * (circRadius+h)),
            new Vector2(0, (circRadius+h)),
            new Vector2(Mathf.Sin(-bzWidthRad/2) * (circRadius+h), Mathf.Cos(-bzWidthRad/2) * (circRadius+h))
        };

        if (beatZoneSize < 2)
        {
            // Tiny wedges don't render properly because the points are too close together, so skip the middle of the wedge top
            bzPoints = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(Mathf.Sin(bzWidthRad/2) * (circRadius+h), Mathf.Cos(bzWidthRad/2) * (circRadius+h)),
                new Vector2(Mathf.Sin(-bzWidthRad/2) * (circRadius+h), Mathf.Cos(-bzWidthRad/2) * (circRadius+h))
            };
        }

        // Add the new points to the spline
        for (int i = 0; i < bzPoints.Length; i++)
        {
            //Vector2 localPoint = safeZoneShapeController.transform.InverseTransformPoint(bzPoints[i]);
            bzSpline.InsertPointAt(i, bzPoints[i]);
            if (i == 2 && colliderSize > 1)
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
    }
    
    //public void Flash()
    //{
    //    this.GetComponent<Renderer>().material.color = Color.yellow;
    //}

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

