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
        public float beatZoneSize;
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
        float beatZoneSizeOut;

        string filePath;
        #if UNITY_EDITOR
            // In the Editor, look for the file in the project root
            filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        #else
            // In a built game, look for the file in the build directory
            filePath = Path.Combine(Application.dataPath, "..", fileName);  // The ".." makes us go one folder above the data folder, which is where the application exe is
        #endif

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            trials = new TrialParameters[lines.Length-1];

            for (int i = 0; i < lines.Length; i++)
            {
                string[] splitLine = lines[i].Split('\t'); // Split the line by tabs
                if (i > 0)
                {


                    if (splitLine.Length >= 4) // Ensure there are at least 4 parameters
                    {
                        // Parse parameters into proper formats
                        if (float.TryParse(splitLine[0], out wheelSpeedOut) &&
                            int.TryParse(splitLine[2], out beatMaxOut) &&
                            int.TryParse(splitLine[3], out targetScoreOut) &&
                            float.TryParse(splitLine[4], out colliderSizeOut) &&
                            float.TryParse(splitLine[5], out beatZoneSizeOut))
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


                            trials[i-1] = new TrialParameters
                            {
                                wheelSpeed = wheelSpeedOut,
                                eventList = eventListValues,
                                beatMax = beatMaxOut,
                                targetScore = targetScoreOut,
                                colliderSize = colliderSizeOut,
                                beatZoneSize = beatZoneSizeOut,
                            };
                        }

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

    public void LoadSessionParameters(string fileName)
    {
        string filePath;

        #if UNITY_EDITOR
            // In the Editor, look for the file in the project root
            filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        #else
            // In a built game, look for the file in the build directory
            filePath = Path.Combine(Application.dataPath, "..", fileName);
        #endif
        

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
        else
        {
            Debug.Log("Session Parameter File not found");
        }
    }
}


