using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class GameController : MonoBehaviour
{
    public WheelControl Wheel;
    public WheelLineDetector Target;
    public Image LRSImage;
    public TextMeshProUGUI scoreText;
    public ParameterLoader parameters;

    public AudioClip bridgeSound;
    public AudioClip tickSound;
    public AudioClip goodHitSound;
    public AudioClip badHitSound;
    public bool pause;

    private bool contact; // whether target is touching an eventBox
    private int score;  // current score
    private bool booped;  // whether the current eventBox has been hit
    private int eventCount;  // total number of contact events (beats)
    private AudioSource audioSource;
    private int numTrials;  // number of trials
    private int currTrial; // index of current trial
    private int eventMax; // max beats to present
    private int targetScore;  // target score to pass
    private bool trialIsRunning; // whether trial is running or not
    private float LRSDuration; // how long the LRS should be visible
    private int LRSThresh; // how long the LRS should be visible
    private float colliderSize;  // width of the eventBox collider

    private static string logFilePath = Application.dataPath + "/Data/EventLog.txt";


    // Start is called before the first frame update
    void Start()
    {
        DisableLRS();  // turn off the LRS image to start
        trialIsRunning = false;
        score = 0;
        eventCount = 0;
        booped = false;
        audioSource = GetComponent<AudioSource>();
        // read parameter file
        parameters.LoadTrialParameters("trials.txt");
        // Get number of trials
        numTrials = parameters.trials.Length;
        //Debug.Log("Trial count: " + numTrials);
        currTrial = 0;

        LRSDuration = parameters.LRSDuration;
        LRSThresh = parameters.LRSThresh;

        // create log file
        System.DateTime currentTime = System.DateTime.Now;

        // Format the date and time to include milliseconds
        string timeWithMilliseconds = currentTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

        string currDate = currentTime.ToString("yyyyMMdd");
        string logFileName = parameters.AnimalName + "_" + currDate + ".txt";
        string logFileFolder = Path.Combine(Application.dataPath, "Data", parameters.AnimalName);
        logFilePath = Path.Combine(logFileFolder, logFileName);
        if (!Directory.Exists(logFileFolder))
        {
            Directory.CreateDirectory(logFileFolder);
        }
        EventLogger.SetLogFilePath(logFilePath);
        EventLogger.LogEvent("Program Start", "Program Start");

        scoreText.text = "Click to start";
        Wheel.StopSpin();
    }

    // Update is called once per frame
    void Update()
    {
        if (trialIsRunning)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Debug.Log("Clicked!");
                Target.Bounce();
                if (contact)
                {
                    EventLogger.LogEvent("Response", "Hit");
                    booped = true;
                    if (score < 0)
                    {
                        score = 1;
                    }
                    else
                    {
                        score++;
                    }
                    scoreText.text = score.ToString();
                    audioSource.PlayOneShot(goodHitSound);
                    if (score >= targetScore)
                    {
                        audioSource.PlayOneShot(bridgeSound);
                    }
                }
                else
                {
                    EventLogger.LogEvent("Response", "Miss");
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

                if (score < LRSThresh)
                {
                    
                    TriggerLRS(LRSDuration);
                    score = 0;
                    scoreText.text = score.ToString();
                }
            }

            if (eventCount >= eventMax)
            {
                EndTrial();
            }
            if (score >= targetScore)
            {
                EndTrial();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartTrial();
            }
        }
    }

    void StartTrial()
    {
        // read next line in parameter file
        // initiate wheel and eventBoxes
        //TrialParameters currParams = parameters[currTrial].GetTrialParameters(currTrial);
        Wheel.wheelTempo = parameters.trials[currTrial].wheelSpeed;
        Wheel.eventList = parameters.trials[currTrial].eventList;
        eventMax = parameters.trials[currTrial].beatMax;
        targetScore = parameters.trials[currTrial].targetScore;
        colliderSize = parameters.trials[currTrial].colliderSize;
        eventCount = 0;
        score = 0;
        trialIsRunning = true;
        EventLogger.LogEvent("Trial", "Trial " + (currTrial+1) + " started");
        Wheel.colliderSize = colliderSize;
        Wheel.ResetWheel();
        Wheel.StartSpin();


    }

    void EndTrial()
    {
        // stop wheel
        Wheel.StopSpin();
        trialIsRunning = false;
        EventLogger.LogEvent("Trial", "Trial " + (currTrial + 1) + " ended");
        Wheel.ClearWheel();
        // write end of trial to log

        // if not max trial, start next trial
        if (currTrial < (numTrials - 1))
        {
            currTrial++;
            scoreText.text = "Click to start";

            // StartTrial();
        }
        else
        {
            scoreText.text = "Game Over";
        }




    }

    public void TriggerLRS(float duration)
    {
        if (LRSImage != null)
        {
            EventLogger.LogEvent("Feedback", "LRS initiated");
            LRSImage.enabled = true; // Enable the blackout image
            Wheel.StopSpin();
            Invoke("DisableLRS", duration); // Disable after the duration
        }
    }

    void DisableLRS()
    {
        if (LRSImage != null)
        {
            EventLogger.LogEvent("Feedback", "LRS ended");
            LRSImage.enabled = false; // Disable the blackout image
            Wheel.StartSpin();
        }
    }


    private void OnEnable()
    {
        //Debug.Log("Trigger triggered!");
        WheelLineDetector.OnContactStart += ContactOn;
        WheelLineDetector.OnContactEnd += ContactOff;

    }

    private void OnDisable()
    {
        //Debug.Log("Trigger off");
        WheelLineDetector.OnContactStart -= ContactOn;
        WheelLineDetector.OnContactEnd -= ContactOff;
        
    }
    
    private void ContactOn()
    {
        EventLogger.LogEvent("Beat", "Beat window start");
        eventCount++;
        audioSource.PlayOneShot(tickSound);
        contact = true;
        booped = false;
        
        //Log beat start
    }

    private void ContactOff()
    {
        EventLogger.LogEvent("Beat", "Beat window end");
        contact = false;
        if (!booped)
        {
            if (score > 0)
            {
                score = 0;
                scoreText.text = score.ToString();
            }
        }
        //Log beat end
    }

}


