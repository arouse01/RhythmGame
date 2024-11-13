using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class EventLogger
{
    private static string logFilePath = Application.dataPath + "/EventLog.txt";

    public static void SetLogFilePath(string path)
    {
        logFilePath = path;
    }

    public static void LogEvent(string eventType, string eventMessage)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"{timestamp}\t{eventType}\t{eventMessage}");
        }
    }
}
