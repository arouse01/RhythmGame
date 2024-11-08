using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;


public class WheelLineDetector : MonoBehaviour
{
    
    //public TextMeshProUGUI scoreText;
    //public int eventMax;
    //public int targetScore;
    //public AudioClip bridgeSound;
    //public AudioClip tickSound;
    //public AudioClip goodHitSound;
    //public AudioClip badHitSound;
    
    Animation anim;

    // Events to trigger GameController
    public static event Action OnContactStart;
    public static event Action OnContactEnd;
    public static event Action OnBeatContact;

    private Transform triangle;
    //private bool contact; // whether target is touching an eventBox
    //private int score;
    //public bool booped;  // whether the current eventBox has been hit
    //private int eventCount;
    //private AudioSource audioSource;
    private GameObject[] boxes;  // to change box color as needed


    void Start()
    {
        triangle = transform.Find("Triangle");
        anim = triangle.GetComponent<Animation>();
        //score = 0;
        //eventCount = 0;
        //audioSource = GetComponent<AudioSource>();
        //booped = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    // Debug.Log("Clicked!");
        //    //anim.Play("Bounce");
        //    //    if (contact)
        //    //    {
        //    //        //Debug.Log("Success!");
        //    //        booped = true;
        //    //        if (score < 0)
        //    //        {
        //    //            score = 1;
        //    //        } else
        //    //        {
        //    //            score++;
        //    //        }
        //    //        scoreText.text = score.ToString();
        //    //        audioSource.PlayOneShot(goodHitSound);
        //    //        if (score >= targetScore)
        //    //        {
        //    //            audioSource.PlayOneShot(bridgeSound);
        //    //        }
        //    //    } else
        //    //    {
        //    //        //Debug.Log("Miss!");
        //    //        if (score > 0)
        //    //        {
        //    //            score = -1;
        //    //        }
        //    //        else
        //    //        {
        //    //            score--;
        //    //        }
        //    //        scoreText.text = score.ToString();
        //    //        audioSource.PlayOneShot(badHitSound, 0.5f);
        //    //    }

        //}


        ////if(eventCount >= eventMax)
        ////{
        ////    Application.Quit();
        ////}

    }

    // This method is called when another collider enters the trigger zone
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EventBox"))
        {
            OnContactStart?.Invoke();
            //GetComponent<Renderer>().material.color = Color.blue;
            triangle.GetComponent<Renderer>().material.color = Color.white;
            other.GetComponent<Renderer>().material.color = Color.yellow;
        }
        else if (other.CompareTag("Beat"))
        {
            OnBeatContact?.Invoke();
        }
            
        //contact = true;
        //eventCount++;
        //audioSource.PlayOneShot(tickSound);
        // Debug.Log("Trigger triggered!");
        //booped = false;
    }

    // This method is called when another collider exits the trigger zone
    private void OnTriggerExit2D(Collider2D other)
    {
        //GetComponent<Renderer>().material.color = Color.white;
        if (other.CompareTag("EventBox"))
        {
            OnContactEnd?.Invoke();
            triangle.GetComponent<Renderer>().material.color = Color.blue;
            other.GetComponent<Renderer>().material.SetColor("_Color", new Color(0, 0, 0));
        }
        else if (other.CompareTag("Beat"))
        {
            
        }

        //contact = false;
        //if(!booped)
        //{
        //    if (score > 0)
        //    {
        //        score = 0;
        //        scoreText.text = score.ToString();
        //    }
        //}
        // Debug.Log("Trigger exited!");

    }

    public void Bounce()
    {
        anim.Play("Bounce");
    }
}

