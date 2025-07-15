using UnityEngine;
using System.Diagnostics;
using System.IO;

public class ECGProcessor : MonoBehaviour
{
    public int recordNum = 105;
    public string pythonExecutable = @"C:\Path\To\python.exe"; // Replace with actual Python path
    public string scriptPath = @"Assets/Data/Processor/process_ecg.py";
    public string datasetPath = @"C:\Users\Kiran\Desktop\PESticide\HEADACHE\CAVE Labs\project\unity\Heart_Simulation\Assets\Data\mit-bih-arrhythmia-database-1.0.0\mit-bih-arrhythmia-database-1.0.0";

    void Start()
    {
        RunPythonScript(recordNum);
    }

    void RunPythonScript(int recordNum)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = pythonExecutable;
        start.Arguments = $"\"{scriptPath}\" {recordNum}";
        start.WorkingDirectory = datasetPath;
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;

        using (Process process = Process.Start(start))
        {
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            UnityEngine.Debug.Log($"Python Output:\n{output}");
            if (!string.IsNullOrEmpty(error))
                UnityEngine.Debug.LogError($"Python Error:\n{error}");
        }

        UnityEngine.Debug.Log("? ECG Data Processing Complete");
    }
}
