using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    // === Assign in Inspector ===
    public HeartAnimationController heartAnimationController;
    public ECGGraph ecgGraph;
    public Slider speedSlider;
    public TMP_Text speed;
    public TMP_Text bpmDisplay;
    public TMP_Text phaseDisplay; // ✅ Show current ECG phase
    public TextAsset phasesJsonFile;
    public TextAsset plotJsonFile;
    public VirtualECGGraph virtualEcgGraph;
    public HemodynamicsController hemodynamicsController;

    // === Internal state ===
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
        // === Load JSON Data ===
        string phasesJson = "{\"phases\":" + phasesJsonFile.text + "}";
        ecgData = JsonUtility.FromJson<ECGData>(phasesJson);

        string plotJson = "{\"plotValues\":" + plotJsonFile.text + "}";
        ECGPlotData ecgPlotData = JsonUtility.FromJson<ECGPlotData>(plotJson);
        ecgSignal = ecgPlotData.plotValues.ToArray();

        if (ecgData.phases.Count > 0)
            totalDataDuration = (float)ecgData.phases[ecgData.phases.Count - 1].entry;

        ecgGraph.Initialize(ecgSignal);

        // === Speed Slider Setup ===
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
            OnSpeedChanged(speedSlider.value);
        }

        // === Real BPM Calculation ===
        int realQRSCount = 0;
        foreach (var phase in ecgData.phases)
        {
            if (phase.phase == "QRS")
            {
                realQRSCount++;
                float entryTime = (float)phase.entry;
                if (firstQRS < 0) firstQRS = entryTime;
                lastQRS = entryTime;
            }
        }

        if (realQRSCount > 1)
            realBPM = 60f * (realQRSCount - 1) / (lastQRS - firstQRS);
    }

    void Update()
    {
        simulationTime += Time.deltaTime * timeScale;

        // Loop back to start if end is reached
        if (simulationTime > totalDataDuration)
        {
            simulationTime = 0;
            currentPhaseIndex = 0;
            virtualQRSCount = 0;
        }

        ecgGraph.UpdateGraph(simulationTime);
        UpdateCurrentPhaseDisplay(); // ✅ Update UI

        // === Trigger Next ECG Phase ===
        if (currentPhaseIndex < ecgData.phases.Count)
        {
            ECGPhase nextPhase = ecgData.phases[currentPhaseIndex];
            if (simulationTime >= nextPhase.entry)
            {
                UnityEngine.Debug.Log($"🫀 Triggering: {nextPhase.phase} @ {simulationTime:F2}s");

                // Trigger Heart Animation
                heartAnimationController?.PlayPhase(nextPhase.phase, (float)nextPhase.duration, timeScale);

                // Trigger Virtual ECG
                if (nextPhase.phase == "QRS")
                {
                    virtualQRSCount++;
                    virtualEcgGraph?.InjectQRSComplex();
                }

                // Trigger Blood Flow
                if (hemodynamicsController != null)
                {
                    switch (nextPhase.phase)
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

        string currentPhase = "Idle";

        foreach (var phase in ecgData.phases)
        {
            float entry = (float)phase.entry;
            float exit = entry + (float)phase.duration;
            if (simulationTime >= entry && simulationTime < exit)
            {
                currentPhase = phase.phase;
                break;
            }
        }

        phaseDisplay.text = $"Current Phase: {currentPhase}";
    }

    void UpdateBPMDisplay()
    {
        if (bpmDisplay != null)
        {
            float virtualBPM = simulationTime > 0 ? 60f * virtualQRSCount / simulationTime : 0f;
            bpmDisplay.text = $"Real BPM: {realBPM:F1} | Virtual BPM: {virtualBPM:F1}";
        }
    }

    public void OnSpeedChanged(float value)
    {
        timeScale = value;
        virtualEcgGraph?.SetTimeScale(timeScale);
        speed.text = $"Speed: {value * 2:F1}x";
    }
}
