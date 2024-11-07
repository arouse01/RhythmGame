using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class WheelLineDetector : MonoBehaviour
{
    public GameObject triangle;
    public TextMeshProUGUI scoreText;
    public int eventMax;
    public int targetScore;
    public AudioClip bridgeSound;
    public AudioClip tickSound;
    public AudioClip goodHitSound;
    public AudioClip badHitSound;
    
    Animator anim;

    private bool contact; // whether target is touching an eventBox
    private int score;
    public bool booped;  // whether the current eventBox has been hit
    private int eventCount;
    private AudioSource audioSource;
    private GameObject[] boxes;  // to change box color as needed


    void Start()
    {
        anim = triangle.GetComponent<Animator>();
        score = 0;
        eventCount = 0;
        audioSource = GetComponent<AudioSource>();
        booped = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Debug.Log("Clicked!");
            anim.SetTrigger("Active");
            if (contact)
            {
                //Debug.Log("Success!");
                booped = true;
                if (score < 0)
                {
                    score = 1;
                } else
                {
                    score++;
                }
                scoreText.text = score.ToString();
                audioSource.PlayOneShot(goodHitSound);
                if (score >= targetScore)
                {
                    audioSource.PlayOneShot(bridgeSound);
                }
            } else
            {
                //Debug.Log("Miss!");
                if (score > 0)
                {
                    score = -1;
                }
                else
                {
                    score--;
                }
                scoreText.text = score.ToString();
                audioSource.PlayOneShot(badHitSound, 0.5f);
            }

        }

                
        if(eventCount >= eventMax)
        {
            Application.Quit();
        }

    }

    // This method is called when another collider enters the trigger zone
    private void OnTriggerEnter2D(Collider2D other)
    {
        //GetComponent<Renderer>().material.color = Color.blue;
        triangle.GetComponent<Renderer>().material.color = Color.blue;
        other.GetComponent<Renderer>().material.color = Color.yellow;
        contact = true;
        eventCount++;
        audioSource.PlayOneShot(tickSound);
        // Debug.Log("Trigger triggered!");
        booped = false;
    }

    // This method is called when another collider exits the trigger zone
    private void OnTriggerExit2D(Collider2D other)
    {
        //GetComponent<Renderer>().material.color = Color.white;
        triangle.GetComponent<Renderer>().material.color = Color.white;
        other.GetComponent<Renderer>().material.SetColor("_Color", new Color(0,0,0));
        contact = false;
        if(!booped)
        {
            if (score > 0)
            {
                score = 0;
                scoreText.text = score.ToString();
            }
        }
        // Debug.Log("Trigger exited!");

    }
}

