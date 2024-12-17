using System.IO;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEngine;


public class PostProcess : IPostprocessBuildWithReport
{
    // Specify the callback order if needed; lower numbers run first
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        // Path to the folder where the build is created
        string buildFolder = Path.GetDirectoryName(report.summary.outputPath);

        // Define your source files
        string parameterFile = Path.Combine(Directory.GetCurrentDirectory(), "parameters.txt");  // The ".." makes us go one folder above the data folder, which is where the application exe is
        string trialFile = Path.Combine(Directory.GetCurrentDirectory(), "trials.txt");
        //Path.Combine(Application.dataPath, "Parameters", "file1.txt");
        // Define the target destination (build folder)
        string paramFileDest = Path.Combine(buildFolder, "parameters.txt");
        string trialFileDest = Path.Combine(buildFolder, "trials.txt");

        // Copy files if they exist
        try
        {
            if (File.Exists(parameterFile))
                File.Copy(parameterFile, paramFileDest, overwrite: true);
            else
                Debug.LogWarning($"Source file not found: {parameterFile}");

            if (File.Exists(trialFile))
                File.Copy(trialFile, trialFileDest, overwrite: true);
            else
                Debug.LogWarning($"Source file not found: {trialFile}");

            Debug.Log("Parameter files copied successfully to the build folder.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error copying parameter files: {ex.Message}");
        }
    }
}
