using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Diagnostics;

[System.Serializable]
public class ECGPhase
{
    public double entry;
    public double duration;
    public string phase;
}

[System.Serializable]
public class ECGData
{
    public List<ECGPhase> phases;
}

[System.Serializable]
public class ECGPlotData
{
    public List<float> plotValues;
}

public class HeartSimulationController : MonoBehaviour
{
    [Header("References")]
    public HeartAnimationController heartAnimationController;
    public ECGGraph ecgGraph;
    public VirtualECGGraph virtualEcgGraph;
    public HemodynamicsController hemodynamicsController;

    [Header("UI")]
    public Slider speedSlider;
    public TMP_Text speed;
    public TMP_Text bpmDisplay;
    public TMP_Text phaseDisplay;

    [Header("Default JSON Assets")]
    public TextAsset phasesJsonFile;
    public TextAsset plotJsonFile;

    private ECGData ecgData;
    private float[] ecgSignal;
    private float simulationTime = 0f;
    private float timeScale = 1f;
    private int currentPhaseIndex = 0;
    private float totalDataDuration;

    private float realBPM = 0f;
    private int virtualQRSCount = 0;
    private float firstQRS = -1f;
    private float lastQRS = -1f;

    void Start()
    {
        // Always load default JSON on start; user uploads handled later
        UnityEngine.Debug.Log("📦 Loading default JSON...");
        string phasesJson = "{\"phases\":" + phasesJsonFile.text + "}";
        ecgData = JsonUtility.FromJson<ECGData>(phasesJson);

        string plotJson = "{\"plotValues\":" + plotJsonFile.text + "}";
        ECGPlotData ecgPlotData = JsonUtility.FromJson<ECGPlotData>(plotJson);
        ecgSignal = ecgPlotData.plotValues.ToArray();

        InitSimulation();
    }

    void InitSimulation()
    {
        simulationTime = 0f;
        currentPhaseIndex = 0;
        virtualQRSCount = 0;
        firstQRS = -1f;
        lastQRS = -1f;

        if (ecgData.phases.Count > 0)
            totalDataDuration = (float)ecgData.phases[ecgData.phases.Count - 1].entry;

        ecgGraph.Initialize(ecgSignal);

        int realQRSCount = 0;
        foreach (var phase in ecgData.phases)
        {
            if (phase.phase == "QRS")
            {
                realQRSCount++;
                float entry = (float)phase.entry;
                if (firstQRS < 0) firstQRS = entry;
                lastQRS = entry;
            }
        }

        if (realQRSCount > 1)
            realBPM = 60f * (realQRSCount - 1) / (lastQRS - firstQRS);

        if (speedSlider != null)
        {
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
            OnSpeedChanged(speedSlider.value);
        }
    }

    void Update()
    {
        simulationTime += Time.deltaTime * timeScale;

        if (simulationTime > totalDataDuration)
        {
            simulationTime = 0;
            currentPhaseIndex = 0;
            virtualQRSCount = 0;
        }

        ecgGraph.UpdateGraph(simulationTime);
        UpdateCurrentPhaseDisplay();

        if (currentPhaseIndex < ecgData.phases.Count)
        {
            ECGPhase next = ecgData.phases[currentPhaseIndex];
            if (simulationTime >= next.entry)
            {
                UnityEngine.Debug.Log($"🫀 Triggering {next.phase} @ {simulationTime:F2}s");

                heartAnimationController?.PlayPhase(next.phase, (float)next.duration, timeScale);

                if (next.phase == "QRS")
                {
                    virtualQRSCount++;
                    virtualEcgGraph?.InjectQRSComplex();
                }

                if (hemodynamicsController != null)
                {
                    switch (next.phase)
                    {
                        case "PQ":
                            hemodynamicsController.TriggerAtrialContraction();
                            break;
                        case "QRS":
                            hemodynamicsController.TriggerVentricularEjection();
                            break;
                    }
                }

                currentPhaseIndex++;
            }
        }

        UpdateBPMDisplay();
    }

    void UpdateCurrentPhaseDisplay()
    {
        if (phaseDisplay == null) return;

        string current = "Idle";

        foreach (var p in ecgData.phases)
        {
            float start = (float)p.entry;
            float end = start + (float)p.duration;
            if (simulationTime >= start && simulationTime < end)
            {
                current = p.phase;
                break;
            }
        }

        phaseDisplay.text = $"Current Phase: {current}";
    }

    void UpdateBPMDisplay()
    {
        if (bpmDisplay != null)
        {
            float virtualBPM = simulationTime > 0 ? 60f * virtualQRSCount / simulationTime : 0f;
            bpmDisplay.text = $"Real BPM: {realBPM:F1} | Virtual BPM: {virtualBPM:F1}";
        }
    }

    public void OnSpeedChanged(float val)
    {
        timeScale = val;
        virtualEcgGraph?.SetTimeScale(timeScale);
        if (speed != null)
            speed.text = $"Speed: {val * 2:F1}x";
    }

    // Called by WebGLJSONLoader.cs when user uploads JSON
    public void LoadFromText(string plotJson, string phasesJson)
    {
        ecgData = JsonUtility.FromJson<ECGData>("{\"phases\":" + phasesJson + "}");
        ECGPlotData plot = JsonUtility.FromJson<ECGPlotData>("{\"plotValues\":" + plotJson + "}");
        ecgSignal = plot.plotValues.ToArray();

        InitSimulation();
    }
}
