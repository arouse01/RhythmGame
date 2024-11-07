using System.IO;
using UnityEngine;

public class ParameterLoader : MonoBehaviour
{
    [System.Serializable]
    public class TrialParameters
    {
        public float wheelSpeed;
        public float[] eventList;
        public int beatMax;
        public int targetScore;
        public float colliderSize;
    }

    public string AnimalName;
    public string ExperimenterName;
    public string SavePath;
    public float LRSDuration;
    public int LRSThresh;

    public TrialParameters[] trials; // Array to hold the parameters for each trial

    void Start()
    {
        // Call the function to load parameters from the file
        //Debug.Log("Starting file load!");
        LoadSessionParameters("parameters.txt");
    }

    public void LoadTrialParameters(string fileName)
    {
        float wheelSpeedOut;
        int beatMaxOut;
        int targetScoreOut;
        float colliderSizeOut;

        string filePath = Path.Combine(Application.streamingAssetsPath, fileName); // Application.streamingAssetsPath is fine for read-only files (at least as far as the game is concerned) like config files, but not log files

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            trials = new TrialParameters[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                string[] splitLine = lines[i].Split('\t'); // Split the line by tabs
                if (splitLine.Length >= 4) // Ensure there are at least 4 parameters
                {
                    // Parse parameters into proper formats
                    if (float.TryParse(splitLine[0], out wheelSpeedOut) &&
                        int.TryParse(splitLine[2], out beatMaxOut) &&
                        int.TryParse(splitLine[3], out targetScoreOut) &&
                        float.TryParse(splitLine[4], out colliderSizeOut))
                    // Parse eventList first, starting as string and converting to float
                    {
                        string[] eventListStrings = splitLine[1].Split(',');
                        float[] eventListValues = new float[eventListStrings.Length];

                        for (int j = 0; j < eventListStrings.Length; j++)
                        {
                            if (!float.TryParse(eventListStrings[j], out eventListValues[j]))
                            {
                                Debug.LogError("Invalid float value in param3: " + eventListStrings[j]);
                            }
                        }


                        trials[i] = new TrialParameters
                        {
                            wheelSpeed = wheelSpeedOut,
                            eventList = eventListValues,
                            beatMax = beatMaxOut,
                            targetScore = targetScoreOut,
                            colliderSize = colliderSizeOut
                        };
                    }

                }
            }
        }
        else
        {
            Debug.LogError("File not found at: " + filePath);
        }
    }

    public void GetTrialParameters(int trialIndex)
    {
        if (trialIndex >= 0 && trialIndex < trials.Length)
        {
            Debug.Log("Trial " + (trialIndex + 1) + " Parameters: " +
                      trials[trialIndex].wheelSpeed + ", " +
                      string.Join(", ", trials[trialIndex].eventList) + ", " +
                      trials[trialIndex].beatMax + ", " +
                      trials[trialIndex].targetScore);
            //return trials[trialIndex];
        }
        else
        {
            Debug.LogError("Invalid trial index");
        }
    }

    private void LoadSessionParameters(string fileName)
    {
        

        string filePath = Path.Combine(Application.streamingAssetsPath, fileName); // Adjust path if needed

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                //Debug.Log(line);  // To show the content of the file (for debugging)

                // Parse each line based on your expected format
                // For example, let's assume each line is in the form of "variableName=value"
                string[] keyValue = line.Split('=');
                if (keyValue.Length == 2)
                {
                    string variableName = keyValue[0].Trim();
                    string value = keyValue[1].Trim();

                    // Assign values to the corresponding starting variables
                    if (variableName == "Animal")
                    {
                        AnimalName = value;
                    }
                    else if (variableName == "Experimenter")
                    {
                        ExperimenterName = value;
                    }
                    else if (variableName == "LRS Duration")
                    {
                        float.TryParse(value, out LRSDuration) ;
                    }
                    else if (variableName == "LRS Thresh")
                    {
                        int.TryParse(value, out LRSThresh);
                    }
                }
            }

        }
    }
}


