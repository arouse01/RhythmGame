using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBoxScript : MonoBehaviour
{
    public float angle = 0;
    public float colliderSize = 1;
    private GameObject wheel;  // parent wheel object, for getting radius

    // Start is called before the first frame update
    void Start()
    {
        wheel = this.transform.parent.gameObject;
        
        float circRadius = wheel.transform.localScale.x / 2;
        float angleRad = angle * Mathf.Deg2Rad;
        float posX = Mathf.Sin(angleRad) * circRadius;
        float posY = Mathf.Cos(angleRad) * circRadius;
        this.transform.position = new Vector3(posX, posY, 0.0F);
        this.transform.Rotate(0.0f, 0.0f, -angle, Space.Self);

        // update collider size

        //GameObject childBox = this.gameObject.transform.GetChild(0).gameObject;
        
        BoxCollider2D squareCollider = GetComponent<BoxCollider2D>();
        if (squareCollider != null)
        {
            squareCollider.size = new Vector2(colliderSize, 1f);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
