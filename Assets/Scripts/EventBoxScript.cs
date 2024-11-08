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

    public void SetColliderSize(float newWidth)
    {
        BoxCollider2D squareCollider = GetComponent<BoxCollider2D>();
        if (squareCollider != null)
        {
            squareCollider.size = new Vector2(newWidth, 1f);
        }
    }

    public void ResetShape()
    {
        float h = 0.5f;
        // redefine spline points to be a wedge
        Spline spline = spriteShapeController.spline;
        spline.Clear();  // clear existing points

        float widthRad = colliderSize * Mathf.Deg2Rad;

        Vector2[] points = new Vector2[]
        {
            new Vector2(Mathf.Sin(-widthRad/2) * (circRadius-h), Mathf.Cos(-widthRad/2) * (circRadius-h)),
            new Vector2(Mathf.Sin(widthRad/2) * (circRadius-h), Mathf.Cos(widthRad/2) * (circRadius-h)),
            new Vector2(Mathf.Sin(widthRad/2) * (circRadius+h), Mathf.Cos(widthRad/2) * (circRadius+h)),
            new Vector2(0, (circRadius+h)),
            new Vector2(Mathf.Sin(-widthRad/2) * (circRadius+h), Mathf.Cos(-widthRad/2) * (circRadius+h))
        };

        // Add the new points to the spline
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 localPoint = spriteShapeController.transform.InverseTransformPoint(points[i]);
            spline.InsertPointAt(i, localPoint);
            if (i == 3)
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
    }

    public void SelfDestruct()
    {
        Destroy(gameObject);
    }
}
