using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class EventBox : MonoBehaviour
{
    public float angle = 0;
    public float colliderSize = 1;
    public SpriteShapeController spriteShapeController;

    private GameObject wheel;  // parent wheel object, for getting radius
    private float circRadius;

    // Start is called before the first frame update
    void Start()
    {
        spriteShapeController = GetComponent<SpriteShapeController>();
        wheel = this.transform.parent.gameObject;
        
        circRadius = wheel.transform.localScale.x / 2;
        
        ResetShape();

        float angleRad = angle * Mathf.Deg2Rad;
        float posX = Mathf.Sin(angleRad) * circRadius;
        float posY = Mathf.Cos(angleRad) * circRadius;
        //this.transform.position = new Vector3(posX, posY, 0.0F);
        this.transform.Rotate(0.0f, 0.0f, -angle, Space.Self);

        // update collider size

        //GameObject childBox = this.gameObject.transform.GetChild(0).gameObject;
        
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void SetColliderSize(float newWidth)
    //{
    //    BoxCollider2D squareCollider = GetComponent<BoxCollider2D>();
    //    if (squareCollider != null)
    //    {
    //        squareCollider.size = new Vector2(newWidth, 1f);
    //    }
    //}

    public void ResetShape()
    {
        float h = 1.0f;
        // redefine spline points to be a wedge
        Spline spline = spriteShapeController.spline;
        spline.Clear();  // clear existing points

        float widthRad = colliderSize * Mathf.Deg2Rad;

        Vector2[] points = new Vector2[]
        {
            new Vector2(0, 0),
            //new Vector2(Mathf.Sin(widthRad/2) * (circRadius-h), Mathf.Cos(widthRad/2) * (circRadius-h)),
            new Vector2(Mathf.Sin(widthRad/2) * (circRadius+h), Mathf.Cos(widthRad/2) * (circRadius+h)),
            new Vector2(0, (circRadius+h)),
            new Vector2(Mathf.Sin(-widthRad/2) * (circRadius+h), Mathf.Cos(-widthRad/2) * (circRadius+h))
        };

        //Vector2[] points = new Vector2[];
        if (colliderSize < 2)
        {
            // Tiny wedges don't render properly because the points are too close together, so skip the middle of the wedge top
            points = new Vector2[]
            {
                new Vector2(0, 0),
                //new Vector2(Mathf.Sin(widthRad/2) * (circRadius-h), Mathf.Cos(widthRad/2) * (circRadius-h)),
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

        

        // Optional: Make the shape closed
        spriteShapeController.spline.isOpenEnded = false;

        //// update the collider too
        PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
        polygonCollider.SetPath(0, points);

        //private void UpdateEdgeCollider()
        //{
        //    Vector2[] colliderPoints = new Vector2[spline.GetPointCount()];
        //    for (int i = 0; i < spline.GetPointCount(); i++)
        //    {
        //        colliderPoints[i] = transform.TransformPoint(spline.GetPosition(i));
        //    }
        //    edgeCollider.points = colliderPoints;
        //}
    }

    public void SelfDestruct()
    {
        Destroy(gameObject);
    }
}
