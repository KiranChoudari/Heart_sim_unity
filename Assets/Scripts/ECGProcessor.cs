using UnityEngine;
using System.Diagnostics;
using System.IO;

public class ECGProcessor : MonoBehaviour
{
    public int recordNum = 105;  // Set in Inspector
    public string pythonPath = @"C:\Users\Kiran\AppData\Local\Programs\Python\Python310\python.exe";  // Your Python.exe
    public string pythonScriptPath = @"C:\Users\Kiran\Desktop\PESticide\HEADACHE\CAVE Labs\project\unity\Heart_Simulation\Assets\Data\Processor\process_ecg.py";

    public string workingDir = @"C:\Users\Kiran\Desktop\PESticide\HEADACHE\CAVE Labs\project\unity\Heart_Simulation\Assets\Data";  // Contains mit-bih-arrhythmia-database-1.0.0

    public HeartSimulationController simulationController;  // ✅ Assign in Inspector

    void Start()
    {
        RunECGProcessing();
    }

    void RunECGProcessing()
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = pythonPath;
        start.Arguments = $"\"{pythonScriptPath}\" {recordNum}";
        start.WorkingDirectory = workingDir;
        start.UseShellExecute = false;
        start.CreateNoWindow = true;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;

        Process process = new Process();
        process.StartInfo = start;
        process.OutputDataReceived += (sender, args) => { if (!string.IsNullOrEmpty(args.Data)) UnityEngine.Debug.Log(args.Data); };
        process.ErrorDataReceived += (sender, args) => { if (!string.IsNullOrEmpty(args.Data)) UnityEngine.Debug.LogError(args.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        UnityEngine.Debug.Log("✅ ECG Processing Done!");

        // Now Load JSON into SimulationController
        LoadGeneratedFilesIntoSimulation();
    }

    void LoadGeneratedFilesIntoSimulation()
    {
        string jsonDir = workingDir;

        string phasesPath = Path.Combine(jsonDir, $"ecg_phases{recordNum}.json");
        string plotPath = Path.Combine(jsonDir, $"ecg_plot{recordNum}.json");

        TextAsset phasesAsset = new TextAsset(File.ReadAllText(phasesPath));
        TextAsset plotAsset = new TextAsset(File.ReadAllText(plotPath));

        simulationController.phasesJsonFile = phasesAsset;
        simulationController.plotJsonFile = plotAsset;

        UnityEngine.Debug.Log("✅ JSON files injected into HeartSimulationController");
    }
}
