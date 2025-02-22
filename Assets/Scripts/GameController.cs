using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.InputSystem;

/*
Sounds:
    goodHitSound - ding.wav, Audacity
        Track 1: Generate>Risset Drum, 1760.0 Hz Frequency, Decay 0.2s, Center freq 100 Hz, Width 100 Hz, Noise mix 0, Amplitude 0.8
        Track 2: Generate>Tone, 880 Hz, amplitude 0.2, duration 0.2s
        Select all, Effect>Fade Out
        Effect>Amplify, New Peak Amplitude of -15 dB
    tickSound - tick.wav, Audacity
        Track 1: Generate>Tone, 2000 Hz sine wave, amplitude 1.0, duration 0.025s
        Track 2: Generate>Tone, 3200 Hz sine wave, amplitude 0.1, duration 0.025s long
        Track 3: Generate>Tone, 4400 Hz sine wave, amplitude 0.1, duration 0.025s long
        Track 4: Generate>Tone, 4600 Hz sine wave, amplitude 0.1, duration 0.025s long
        Mix tracks 2-4 and Effect>Amplify, New Peak Amplitude of -15 dB
        Select all tracks from 0.007s to end and Effect>Fade Out
    bridgeSound - Xilo_1_a.wav, received from Kelley Winship as preexisting bridge sound

*/

public class GameController : MonoBehaviour
{
    public WheelControl Wheel;
    public TargetControl Target;
    public Image LRSImage;
    public LevelScore LevelScore;

    public TextMeshProUGUI versionText;

    public GameObject UserInputObject;
    public GameObject gameOverPanel;
    public GameObject playerInfoField;
    public GameObject attentionField;
    public GameObject postNotesField;

    public GameObject preGamePanel;
    public GameObject AnimalField;
    public GameObject preNotesField;
    public GameObject LRSDurationField;
    public GameObject TargetWidthField;

    public GameObject InGameText;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI messageText;
    private GameObject levelScoreObject;
    private GameObject lifeMarkers;
    public ParameterLoader parameters;

    public AudioClip bridgeSound;
    public AudioClip tickSound;
    public AudioClip goodHitSound;
    public AudioClip badHitSound;
    public bool pause;

    public Color safeZoneColorDefault;
    public Color beatZoneColorDefault;
    public Color beatZoneColorFlash;
    private Color beatZoneColorFade; // fade beat zone on boop

    private bool safeZoneContact; // whether target is touching an eventBox
    private bool beatZoneContact; // whether target is touching center of eventBox
    private string sessionNumber;
    private int score;  // current score
    private int mistakeCount; // number of errors
    private int currLives; // number of lives left
    private int defaultLives; // number of errors allowed
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
    private Collider safeZoneObject;
    private bool gameOver;  // game is over, move to user input
    private bool gameOverStarted;  // gameover process started

    private TextMeshProUGUI Life1Marker;
    private TextMeshProUGUI Life2Marker;
    private TextMeshProUGUI Life3Marker;

    private static string logFilePath = Application.dataPath + "/Data/EventLog.txt";

    public InputActionAsset inputActions;
    private InputAction triggerAction;
    private InputAction cancelAction;

    void Start()
    {
        Application.targetFrameRate = 60;
        LRSImage.enabled = false; // Disable the LRS image to start
        versionText.text = "Version " + Application.version;  // Update the version number on the home screen
        //gameOver = false;
        //gameOverStarted = false;
        //gameOverPanel.SetActive(false);
        //trialIsRunning = false;
        //score = 0;
        //eventCount = 0;
        //booped = false;
        //pause = false;
        scoreText = InGameText.transform.Find("Score Text").GetComponent<TextMeshProUGUI>();
        messageText = InGameText.transform.Find("Message Text").GetComponent<TextMeshProUGUI>();
        lifeMarkers = InGameText.transform.Find("Life Markers").gameObject;
        Life1Marker = lifeMarkers.transform.Find("Life 1").GetComponent<TextMeshProUGUI>();
        Life2Marker = lifeMarkers.transform.Find("Life 2").GetComponent<TextMeshProUGUI>();
        Life3Marker = lifeMarkers.transform.Find("Life 3").GetComponent<TextMeshProUGUI>();
        levelScoreObject = InGameText.transform.Find("Level Score").gameObject;
        audioSource = GetComponent<AudioSource>();
        parameters.LoadSessionParameters("parameters.txt");
        LRSDurationField.GetComponent<TMPro.TMP_InputField>().text = parameters.LRSDuration.ToString();
        TargetWidthField.GetComponent<TMPro.TMP_InputField>().text = parameters.targetZoneWidth.ToString();
        Wheel.gameObject.SetActive(false);
        Target.gameObject.SetActive(false);

        beatZoneColorFade = beatZoneColorDefault;
        beatZoneColorFade.a = .5f;


        GameStart();

        
    }

    // Update is called once per frame
    void Update()
    {
        if (trialIsRunning && eventCount >= eventMax)
        {
            EndTrial();
        }
        
    }

    void GameStart()
    {
        gameOver = false;
        gameOverStarted = false;
        gameOverPanel.SetActive(false);
        UserInputObject.SetActive(true);
        preGamePanel.SetActive(true);
        InGameText.SetActive(false);
    }

    public void StartSession(string sessionFile)
    {
        string AnimalName = AnimalField.GetComponent<TMP_InputField>().text;
        //string attentionText = attentionField.GetComponent<TMP_InputField>().text;
        string preNotesText = preNotesField.GetComponent<TMP_InputField>().text;

        //string AnimalName = parameters.AnimalName;

        preGamePanel.SetActive(false);
        UserInputObject.SetActive(false);
        InGameText.SetActive(true);

        Wheel.gameObject.SetActive(true);
        Target.gameObject.SetActive(true);

        // read parameter file
        parameters.LoadTrialParameters(sessionFile);
        parameters.AnimalName = AnimalName;
        // Get number of trials
        numTrials = parameters.trials.Length;

        currTrial = 0;

        // LRSDuration = parameters.LRSDuration;
        LRSDuration = float.Parse(LRSDurationField.GetComponent<TMPro.TMP_InputField>().text);
        LRSThresh = parameters.LRSThresh;
        //targetZoneWidth = float.Parse(TargetWidthField.GetComponent<TMPro.TMP_InputField>().text);
        targetZoneWidth = parameters.targetZoneWidth;

        Target.targetZoneWidth = targetZoneWidth;

        sessionNumber = System.Text.RegularExpressions.Regex.Replace(sessionFile, "[^0-9]", "");
        //Target.InitializeTarget();

        //// create log file
        System.DateTime currentTime = System.DateTime.Now;
        string currDate = currentTime.ToString("yyyyMMddHHmmss");

        //// Format the date and time to include milliseconds
        //string timeWithMilliseconds = currentTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

        string logFileName = parameters.AnimalName + "_" + currDate + ".txt";
        string logFileFolder = Path.Combine(Application.dataPath, "SessionData", parameters.AnimalName);
        logFilePath = Path.Combine(logFileFolder, logFileName);
        if (!Directory.Exists(logFileFolder))
        {
            Directory.CreateDirectory(logFileFolder);
        }
        EventLogger.SetLogFilePath(logFilePath);
        EventLogger.LogEvent("Session", "Animal: " + AnimalName);
        //EventLogger.LogEvent("Session", "Attention: " + attentionText);
        EventLogger.LogEvent("Session", "Presession Notes: " + preNotesText);
        EventLogger.LogEvent("Session", "LRS Duration: " + LRSDuration.ToString());
        EventLogger.LogEvent("Session", "Target Width: " + targetZoneWidth.ToString());

        EventLogger.LogEvent("Session", "Phase: " + sessionNumber);
        EventLogger.LogEvent("Session", "Session Start");

        score = 0;
        trialIsRunning = false;
        eventCount = 0;
        booped = false;
        pause = false;

        defaultLives = 3;

        levelScoreObject.SetActive(false);
        lifeMarkers.SetActive(false);

        messageText.SetText("Click to start<br>Phase " + sessionNumber);
        Wheel.StopSpin();

        ActivateInputs();
    }

    void StartTrial()
    {
        // store trial info in data file
        EventLogger.LogEvent("Trial Param", "Level: " + parameters.trials[currTrial].level.ToString());
        EventLogger.LogEvent("Trial Param", "Wheel Tempo: " + parameters.trials[currTrial].wheelSpeed.ToString());
        string eventList = string.Join(", ", parameters.trials[currTrial].eventList);
        EventLogger.LogEvent("Trial Param", "Event List: " + eventList);
        EventLogger.LogEvent("Trial Param", "Max Beats: " + parameters.trials[currTrial].beatMax.ToString());
        EventLogger.LogEvent("Trial Param", "Target Score: " + parameters.trials[currTrial].targetScore.ToString());
        EventLogger.LogEvent("Trial Param", "Safe Zone Size: " + parameters.trials[currTrial].colliderSize.ToString());
        EventLogger.LogEvent("Trial Param", "Beat Zone Size: " + parameters.trials[currTrial].beatZoneSize.ToString());
        
        // initiate wheel and eventBoxes
        Wheel.wheelTempo = parameters.trials[currTrial].wheelSpeed;
        Wheel.eventList = parameters.trials[currTrial].eventList;
        eventMax = parameters.trials[currTrial].beatMax;
        targetScore = parameters.trials[currTrial].targetScore;
        colliderSize = parameters.trials[currTrial].colliderSize;
        beatZoneSize = parameters.trials[currTrial].beatZoneSize;
        level = parameters.trials[currTrial].level;
        eventCount = 0;
        score = 0;
        mistakeCount = 0;
        currLives = defaultLives;
        lifeMarkers.SetActive(true);
        UpdateLives();
        messageText.SetText("");
        UpdateScore();
        levelScoreObject.SetActive(false);
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

        lifeMarkers.SetActive(false);
        scoreText.SetText("Mistakes: " + (mistakeCount).ToString());

        // TODO: calculate level score
        if (int.Parse(sessionNumber) > 0)
        {
            UpdateLevelScore();
        }

        // if not max trial, start next trial
        if (currTrial < (numTrials - 1))
        {
            currTrial++;
            messageText.SetText("Click to start<br>Trial " + (currTrial+1).ToString());
        }
        else
        {
            messageText.SetText("Game Over");
            gameOver = true;
        }

    }
    
    void GameOver()
    {
        DeactivateInputs();
        InGameText.SetActive(false);
        gameOverStarted = true;
        UserInputObject.SetActive(true);
        gameOverPanel.SetActive(true);
        Wheel.gameObject.SetActive(false);
        Target.gameObject.SetActive(false);
    }

    public void GameOverFinish()
    {
        //GameObject playerInfoField = gameOverPanel.transform.Find("PlayerInfoField").gameObject;
        //GameObject attentionField = gameOverPanel.transform.Find("AttentionField").gameObject;
        //GameObject postNotesField = gameOverPanel.transform.Find("postNotesField").gameObject;

        string playerInfoText = playerInfoField.GetComponent<TMP_InputField>().text;
        string attentionText = attentionField.GetComponent<TMP_InputField>().text;
        string generalNotesText = postNotesField.GetComponent<TMP_InputField>().text;
        EventLogger.LogEvent("Session", "Player Information: " + playerInfoText);
        EventLogger.LogEvent("Session", "Attention: " + attentionText);
        EventLogger.LogEvent("Session", "Postsession Notes: " + generalNotesText);

        gameOverPanel.SetActive(false);
        GameStart();
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
                    // hit in beat zone, score point
                    EventLogger.LogEvent("Response", "Hit");
                    booped = true;
                    if (beatZoneObject != null)
                    {
                        beatZoneObject.GetComponent<Renderer>().material.color = beatZoneColorFlash;
                    }
                    
                    score++;
                    currLives = defaultLives;
                    UpdateLives();

                    // TODO: placeholder for calculating accuracy

                    UpdateScore();
                    audioSource.PlayOneShot(goodHitSound);
                    
                    if (score >= targetScore)
                    {
                        audioSource.PlayOneShot(bridgeSound);
                        EndTrial();
                    }

                }
                else if(safeZoneContact && !booped)
                {
                    // hit in safe zone, no score change
                    EventLogger.LogEvent("Response", "Safe");
                    booped = true;
                    currLives = defaultLives;
                    UpdateLives();  // reset lives to max
                    safeZoneObject.transform.Find("BeatZone").GetComponent<Renderer>().material.color = beatZoneColorFade;



                    //audioSource.PlayOneShot(goodHitSound);

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
                        score = 0;
                    }
                    
                    mistakeCount++;
                    UpdateScore();

                    currLives--;
                    UpdateLives();
                    
                    // TODO: placeholder for accuracy update

                    //audioSource.PlayOneShot(badHitSound, 0.5f);
                }

                //if (currLives <= 0)
                //{

                //    TriggerLRS(LRSDuration);
                //    score = 0;
                //    scoreText.SetText(score.ToString());
                //}



            }
        }

    }

    private void OnEscape(InputAction.CallbackContext context)
    {
        EndTrial();
        GameOver();
    }


    public void TriggerLRS(float duration)
    {
        if (LRSImage != null)
        {
            EventLogger.LogEvent("Feedback", "LRS initiated");
            pause = true;
            LRSImage.enabled = true; // Enable the blackout image
            Wheel.StopSpin();
            InGameText.SetActive(false);
            //scoreText.enabled = false;
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

            InGameText.SetActive(true);
            //scoreText.enabled = true;
            Wheel.StartSpin();
        }
    }

    void UpdateLives()
    {
        switch (currLives)
        {
            case 0:
                TriggerLRS(LRSDuration);
                currLives = defaultLives;
                UpdateLives();
                break;
            case 1:
                Life3Marker.color = new Color(0, 0, 0, 255);
                Life2Marker.color = new Color(0, 0, 0, 255);
                Life1Marker.color = new Color(255, 255, 255, 255);
                break;
            case 2:
                Life3Marker.color = new Color(0, 0, 0, 255);
                Life2Marker.color = new Color(255, 255, 255, 255);
                Life1Marker.color = new Color(255, 255, 255, 255);
                break;
            case 3:
                Life3Marker.color = new Color(255, 255, 255, 255); 
                Life2Marker.color = new Color(255, 255, 255, 255);
                Life1Marker.color = new Color(255, 255, 255, 255);
                break;
            default:
                break;
        }
    }

    void UpdateScore()
    {
        scoreText.SetText(score.ToString());
    }
    
    void UpdateLevelScore()
    {
        levelScoreObject.SetActive(true);
        if (mistakeCount <= 1)
        {
            LevelScore.ShowStars(3);
        }
        else if (mistakeCount <= 4)
        {
            LevelScore.ShowStars(2);
        }
        else
        {
            LevelScore.ShowStars(1);
        }


    }

    private void OnEnable()
    {
        ////Debug.Log("Trigger triggered!");
        //TargetControl.OnContactStart += WindowContactOn;
        //TargetControl.OnContactEnd += WindowContactOff;
        //TargetControl.OnBeatZoneStart += BeatZoneContactOn;
        //TargetControl.OnBeatZoneEnd += BeatZoneContactOff;
        //BeatTicker.OnBeatContact += BeatContact;

        //var gameplayActions = inputActions.FindActionMap("Rhythm");
        //triggerAction = gameplayActions.FindAction("Click");

        //triggerAction.performed += OnClick;
        //triggerAction.Enable();

    }

    void ActivateInputs()
    {
        //Debug.Log("Trigger triggered!");
        TargetControl.OnContactStart += WindowContactOn;
        TargetControl.OnContactEnd += WindowContactOff;
        TargetControl.OnBeatZoneStart += BeatZoneContactOn;
        TargetControl.OnBeatZoneEnd += BeatZoneContactOff;
        BeatTicker.OnBeatContact += BeatContact;

        var gameplayActions = inputActions.FindActionMap("Rhythm");
        triggerAction = gameplayActions.FindAction("Click");
        cancelAction = gameplayActions.FindAction("Cancel");

        triggerAction.performed += OnClick;
        triggerAction.Enable();

        cancelAction.performed += OnEscape;
        cancelAction.Enable();
    }

    void DeactivateInputs()
    {
        //Debug.Log("Trigger off");
        TargetControl.OnContactStart -= WindowContactOn;
        TargetControl.OnContactEnd -= WindowContactOff;
        TargetControl.OnBeatZoneStart -= BeatZoneContactOn;
        TargetControl.OnBeatZoneEnd -= BeatZoneContactOff;
        BeatTicker.OnBeatContact -= BeatContact;

        if (triggerAction != null)
        {
            triggerAction.performed -= OnClick;
            triggerAction.Disable();
        }
        
        if (cancelAction != null)
        {
            cancelAction.performed -= OnEscape;
            cancelAction.Disable();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void OnDisable()
    {
        //Debug.Log("Trigger off");
        TargetControl.OnContactStart -= WindowContactOn;
        TargetControl.OnContactEnd -= WindowContactOff;
        TargetControl.OnBeatZoneStart -= BeatZoneContactOn;
        TargetControl.OnBeatZoneEnd -= BeatZoneContactOff;
        BeatTicker.OnBeatContact -= BeatContact;

        if (triggerAction != null)
        {
            triggerAction.performed -= OnClick;
            triggerAction.Disable();
        }
        
        if (cancelAction != null)
        {
            cancelAction.performed -= OnEscape;
            cancelAction.Disable();
        }
        
    }
    
    private void WindowContactOn()
    {
        EventLogger.LogEvent("Beat", "Beat safe window start");
        eventCount++;

        safeZoneObject = Target.safeZone;
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
                UpdateScore();
            }
        }
    }

    private void BeatContact()
    {
        EventLogger.LogEvent("Beat", "Beat tick");
        if (int.Parse(sessionNumber) > 0)
        {
            audioSource.PlayOneShot(tickSound);
        }
            
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


