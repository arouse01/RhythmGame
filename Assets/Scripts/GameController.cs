using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    public WheelControl Wheel;
    public TargetControl Target;
    public Image LRSImage;
    public TextMeshProUGUI scoreText;
    public ParameterLoader parameters;

    public AudioClip bridgeSound;
    public AudioClip tickSound;
    public AudioClip goodHitSound;
    public AudioClip badHitSound;
    public bool pause;

    public Color safeZoneColorDefault;
    public Color beatZoneColorDefault;
    public Color beatZoneColorFlash;

    public GameObject playerInfoField;
    public GameObject attentionField;
    public GameObject generalNotesField;
    public GameObject gameOverPanel;

    private bool safeZoneContact; // whether target is touching an eventBox
    private bool beatZoneContact; // whether target is touching center of eventBox
    private int score;  // current score
    private int level;  // current level
    private bool booped;  // whether the current eventBox has been hit
    private int eventCount;  // total number of contact events (beats)
    private AudioSource audioSource;
    private int numTrials;  // number of trials
    private int currTrial; // index of current trial
    private int eventMax; // max beats to present
    private int targetScore;  // target score to pass
    private bool trialIsRunning; // whether trial is running or not
    private float LRSDuration = -3; // how long the LRS should be visible
    private int LRSThresh = 1; // how long the LRS should be visible
    private float targetZoneWidth = 0.25f; // width of the target zone around the avatar
    private float colliderSize;  // width of the eventBox collider
    private float beatZoneSize; // width of the beatZone collider
    private Collider beatZoneObject;
    private bool gameOver;  // game is over, move to user input
    private bool gameOverStarted;  // gameover process started

    private static string logFilePath = Application.dataPath + "/Data/EventLog.txt";

    public InputActionAsset inputActions;
    private InputAction triggerAction;

    void Start()
    {
        Application.targetFrameRate = 60;
        LRSImage.enabled = false; // Disable the LRS image to start
        gameOver = false;
        gameOverStarted = false;
        gameOverPanel.SetActive(false);
        trialIsRunning = false;
        score = 0;
        eventCount = 0;
        booped = false;
        pause = false;
        audioSource = GetComponent<AudioSource>();

        StartSession();

        scoreText.text = "Click to start";
        Wheel.StopSpin();
    }

    // Update is called once per frame
    void Update()
    {
        if (trialIsRunning && eventCount >= eventMax)
        {
            EndTrial();
        }
        
    }

    void StartSession()
    {
        // read parameter file
        parameters.LoadTrialParameters("trials.txt");
        parameters.LoadSessionParameters("parameters.txt");
        // Get number of trials
        numTrials = parameters.trials.Length;

        currTrial = 0;

        LRSDuration = parameters.LRSDuration;
        LRSThresh = parameters.LRSThresh;
        targetZoneWidth = parameters.targetZoneWidth;

        Target.targetZoneWidth = targetZoneWidth;

        //// create log file
        System.DateTime currentTime = System.DateTime.Now;
        string currDate = currentTime.ToString("yyyyMMdd");

        string AnimalName = parameters.AnimalName;

        //// Format the date and time to include milliseconds
        //string timeWithMilliseconds = currentTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

        string logFileName = parameters.AnimalName + "_" + currDate + ".txt";
        string logFileFolder = Path.Combine(Application.dataPath, "Data", parameters.AnimalName);
        logFilePath = Path.Combine(logFileFolder, logFileName);
        if (!Directory.Exists(logFileFolder))
        {
            Directory.CreateDirectory(logFileFolder);
        }
        EventLogger.SetLogFilePath(logFilePath);
        EventLogger.LogEvent("Program Start", "Program Start");
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
        beatZoneSize = parameters.trials[currTrial].beatZoneSize;
        level = parameters.trials[currTrial].level;
        eventCount = 0;
        score = 0;
        trialIsRunning = true;
        EventLogger.LogEvent("Trial", "Trial " + (currTrial+1) + " started");
        Wheel.colliderSize = colliderSize;
        Wheel.beatZoneSize = beatZoneSize;
        Wheel.safeZoneColorDefault = safeZoneColorDefault;
        Wheel.beatZoneColorDefault = beatZoneColorDefault;
        Target.beatZoneColorDefault = beatZoneColorDefault;
        Wheel.gameLevel = level;
        Wheel.Reset();
        //Debug.Break();
        Wheel.StartSpin();


    }

    void EndTrial()
    {
        Wheel.StopSpin();
        trialIsRunning = false;
        EventLogger.LogEvent("Trial", "Trial " + (currTrial + 1) + " ended");
        Wheel.Clear();
        Wheel.Resize();
        beatZoneContact = false;
        safeZoneContact = false;
        booped = false;

        // if not max trial, start next trial
        if (currTrial < (numTrials - 1))
        {
            currTrial++;
            scoreText.text = "Click to start";
        }
        else
        {
            scoreText.text = "Game Over";
            gameOver = true;
        }

    }
    
    void GameOver()
    {
        scoreText.enabled = false;
        gameOverStarted = true;
        gameOverPanel.SetActive(true);
    }

    public void GameOverFinish()
    {
        string playerInfoText = playerInfoField.GetComponent<TMP_InputField>().text;
        string attentionText = attentionField.GetComponent<TMP_InputField>().text;
        string generalNotesText = generalNotesField.GetComponent<TMP_InputField>().text;
        EventLogger.LogEvent("UserInput", "Player Information: " + playerInfoText);
        EventLogger.LogEvent("UserInput", "Attention: " + attentionText);
        EventLogger.LogEvent("UserInput", "General Notes: " + generalNotesText);

        gameOverPanel.SetActive(false);
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        //Debug.Log("Clicked!");
        if (gameOver & !gameOverStarted)
        {
            GameOver();
        }
        else if (gameOver)
        {
            
        }
        else if (!trialIsRunning & !gameOver)
        {
            StartTrial();
        }
        else
        { 
            if (!pause)
            {
                Target.Bounce();
                if (beatZoneContact && !booped)
                {
                    EventLogger.LogEvent("Response", "Hit");
                    booped = true;
                    if (beatZoneObject != null)
                    {
                        beatZoneObject.GetComponent<Renderer>().material.color = beatZoneColorFlash;
                    }
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
                    //other.GetComponent<Renderer>().material.color = Color.yellow;
                    if (score >= targetScore)
                    {
                        audioSource.PlayOneShot(bridgeSound);
                        EndTrial();
                    }

                }
                else if(safeZoneContact && !booped)
                {
                    EventLogger.LogEvent("Response", "Safe");
                    booped = true;
                    //if (score < 0)
                    //{
                    //    score = 1;
                    //}
                    //else
                    //{
                    //    score++;
                    //}
                    //scoreText.text = score.ToString();
                    audioSource.PlayOneShot(goodHitSound);
                    //if (score >= targetScore)
                    //{
                    //    audioSource.PlayOneShot(bridgeSound);
                    //}
                }
                else 
                {
                    if (safeZoneContact || beatZoneContact)
                    {
                        // in the safeZone or beatZone but not counted as hit
                        EventLogger.LogEvent("Response", "Miss (already hit)");
                    }
                    else
                    {
                        EventLogger.LogEvent("Response", "Miss");
                    }
                    
                    if (score > 0)
                    {
                        score = -1;
                    }
                    else
                    {
                        score--;
                    }
                    scoreText.text = score.ToString();
                    //audioSource.PlayOneShot(badHitSound, 0.5f);
                }

                if (score < LRSThresh)
                {

                    TriggerLRS(LRSDuration);
                    score = 0;
                    scoreText.text = score.ToString();
                }



            }
        }

    }

    public void TriggerLRS(float duration)
    {
        if (LRSImage != null)
        {
            EventLogger.LogEvent("Feedback", "LRS initiated");
            pause = true;
            LRSImage.enabled = true; // Enable the blackout image
            Wheel.StopSpin();
            scoreText.enabled = false;
            Invoke("DisableLRS", duration); // Disable after the duration
        }
    }

    void DisableLRS()
    {
        if (LRSImage != null)
        {
            EventLogger.LogEvent("Feedback", "LRS ended");
            pause = false;
            LRSImage.enabled = false; // Disable the blackout image
            scoreText.enabled = true;
            Wheel.StartSpin();
        }
    }


    private void OnEnable()
    {
        //Debug.Log("Trigger triggered!");
        TargetControl.OnContactStart += WindowContactOn;
        TargetControl.OnContactEnd += WindowContactOff;
        TargetControl.OnBeatZoneStart += BeatZoneContactOn;
        TargetControl.OnBeatZoneEnd += BeatZoneContactOff;
        BeatTicker.OnBeatContact += BeatContact;

        var gameplayActions = inputActions.FindActionMap("Rhythm");
        triggerAction = gameplayActions.FindAction("Click");

        triggerAction.performed += OnClick;
        triggerAction.Enable();

    }

    private void OnDisable()
    {
        //Debug.Log("Trigger off");
        TargetControl.OnContactStart -= WindowContactOn;
        TargetControl.OnContactEnd -= WindowContactOff;
        TargetControl.OnBeatZoneStart -= BeatZoneContactOn;
        TargetControl.OnBeatZoneEnd -= BeatZoneContactOff;
        BeatTicker.OnBeatContact -= BeatContact;

        triggerAction.performed -= OnClick;
        triggerAction.Disable();
    }
    
    private void WindowContactOn()
    {
        EventLogger.LogEvent("Beat", "Beat safe window start");
        eventCount++;
        
        safeZoneContact = true;
        booped = false;
        
    }

    private void WindowContactOff()
    {
        EventLogger.LogEvent("Beat", "Beat safe window end");
        safeZoneContact = false;
        if (!booped)
        {
            if (score > 0)
            {
                score = 0;
                scoreText.text = score.ToString();
            }
        }
    }

    private void BeatContact()
    {
        EventLogger.LogEvent("Beat", "Beat tick");
        audioSource.PlayOneShot(tickSound);
    }

    private void BeatZoneContactOn()
    {
        EventLogger.LogEvent("Beat", "Beat zone start");
        beatZoneContact = true;
        beatZoneObject = Target.beatZone;
    }
    
    private void BeatZoneContactOff()
    {
        EventLogger.LogEvent("Beat", "Beat zone end");
        beatZoneContact = false;
    }

}


