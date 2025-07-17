using UnityEngine;
using TMPro;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class WebGLJSONLoader : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void OpenFileDialogForWebGL(string gameObjectName, string methodName);

    public TMP_Text plotPathText, phasesPathText;
    public HeartSimulationController simulationController;

    private bool waitingForPlot = false;
    private bool waitingForPhases = false;

    private string plotJsonText = "";
    private string phasesJsonText = "";

    public void SelectPlotWebGL()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        waitingForPlot = true;
        OpenFileDialogForWebGL(gameObject.name, "ReceiveJSON");
#else
        UnityEngine.Debug.LogWarning("This feature works only in WebGL build.");
#endif
    }

    public void SelectPhasesWebGL()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        waitingForPhases = true;
        OpenFileDialogForWebGL(gameObject.name, "ReceiveJSON");
#else
        UnityEngine.Debug.LogWarning("This feature works only in WebGL build.");
#endif
    }

    // Called from JS via SendMessage
    public void ReceiveJSON(string jsonContent)
    {
        if (waitingForPlot)
        {
            plotJsonText = jsonContent;
            plotPathText.text = "Plot JSON loaded ✔";
            waitingForPlot = false;
        }
        else if (waitingForPhases)
        {
            phasesJsonText = jsonContent;
            phasesPathText.text = "Phases JSON loaded ✔";
            waitingForPhases = false;
        }
    }

    public void RunSimulation()
    {
        if (string.IsNullOrEmpty(plotJsonText) || string.IsNullOrEmpty(phasesJsonText))
        {
            UnityEngine.Debug.LogError("❌ Please load both plot and phases JSON files.");
            return;
        }

        simulationController.LoadFromText(plotJsonText, phasesJsonText);
    }
}
