using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using TimeUtil = UnityEngine.Time;

public class EventLogger
{
    

    private static string logFilePath = Application.dataPath + "/EventLog.txt";

    public static void SetLogFilePath(string path)
    {
        logFilePath = path;
    }

    public static void LogEvent(string eventType, string eventMessage, string eventValue=null)
    {
        //string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        double time = TimeUtil.timeAsDouble;
        //var t = ReliableTime.Time;
        //var sec = t % 60f;
        //var min = Math.Floor(t) / 60f % 60f;
        //var hrs = Math.Floor(t) / 3600f % 24f;
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"{time}\t{eventType}\t{eventMessage}\t{eventValue}");
        }
    }

    
}

//// https://discussions.unity.com/t/building-an-accurate-clock-that-will-stay-accurate-over-time/917706/22
//public class ReliableTime : MonoBehaviour
//{

//    [SerializeField][Range(0f, 100f)] float _timeScale = 1f;

//    private const int SIXTY = 60;
//    private const double TOLERANCE = 1E-2;

//    static ReliableTime _instance;
//    static public ReliableTime Instance => _instance;

//    float _lastTime;  // in seconds
//    float _measTime;  // in seconds
//    int _minRegister; // in minutes

//    public float Time => SIXTY * _minRegister + _measTime;
//    public double TimeAsDouble => SIXTY * (double)_minRegister + (double)_measTime;

//    public (int h, int m, float s) GetTime()
//      => ((int)(_minRegister / (float)SIXTY),
//           _minRegister % SIXTY,
//           _measTime);

//    void Awake()
//    {
//        _instance = this;
//        setValues(TimeUtil.timeAsDouble);
//        _lastTime = _measTime;
//    }

//    void setValues(double absTime)
//    {
//        _measTime = (float)(absTime % SIXTY);
//        _minRegister = (int)(absTime / SIXTY);
//    }

//    void Update()
//    {
//        TimeUtil.timeScale = _timeScale;

//        _measTime += TimeUtil.deltaTime;

//        if (Math.Abs(_measTime - _lastTime) >= 1f)
//        {
//            _lastTime = Mathf.Floor(_measTime);

//            if (_timeScale <= 1f)
//            {
//                var time = GetTime();
//                Debug.Log($"{time.h}:{time.m}:{time.s:F3}");
//            }

//            var measured = TimeAsDouble;
//            var lapsed = TimeUtil.timeAsDouble;

//            if (Math.Abs(measured - lapsed) > TOLERANCE)
//            {
//                var next = (measured + lapsed) / 2f;
//                Debug.Log($"correction: was {measured:F4} now {next:F4}");
//                setValues(next);
//            }

//            if (_measTime >= SIXTY)
//            {
//                _measTime -= SIXTY;
//                _minRegister++;
//            }
//        }
//    }
//}