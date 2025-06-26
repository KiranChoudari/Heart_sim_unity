using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

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
    public TMP_Text speed;        // Label above slider
    public TMP_Text bpmDisplay;   // ✅ TextMeshPro to show BPM
    public TextAsset phasesJsonFile;
    public TextAsset plotJsonFile;
    public VirtualECGGraph virtualEcgGraph; // Assign in Inspector


    // === Simulation state ===
    private ECGData ecgData;
    private float[] ecgSignal;
    private float simulationTime = 0f;
    private float timeScale = 1f;
    private int currentPhaseIndex = 0;
    private float totalDataDuration;

    // === BPM tracking ===
    private float realBPM = 0f;
    private float runtimeStart;
    private int virtualQRSCount = 0;
    private float firstQRS = -1f;
    private float lastQRS = -1f;

    void Start()
    {
        // Load JSON
        string phasesJson = "{\"phases\":" + phasesJsonFile.text + "}";
        ecgData = JsonUtility.FromJson<ECGData>(phasesJson);

        string plotJson = "{\"plotValues\":" + plotJsonFile.text + "}";
        ECGPlotData ecgPlotData = JsonUtility.FromJson<ECGPlotData>(plotJson);
        ecgSignal = ecgPlotData.plotValues.ToArray();

        // Setup speed
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
            OnSpeedChanged(speedSlider.value);
        }
        else
        {
            timeScale = 1f;
        }

        // Estimate total duration of ECG
        if (ecgData.phases.Count > 0)
        {
            totalDataDuration = (float)ecgData.phases[ecgData.phases.Count - 1].entry;
        }

        ecgGraph.Initialize(ecgSignal);
        runtimeStart = Time.time;

        // ✅ Calculate real BPM now
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
        {
            realBPM = 60f * (realQRSCount - 1) / (lastQRS - firstQRS);
        }
    }

    void Update()
    {
        simulationTime += Time.deltaTime * timeScale;

        // Loop
        if (simulationTime > totalDataDuration)
        {
            simulationTime = 0;
            currentPhaseIndex = 0;
            virtualQRSCount = 0;
            runtimeStart = Time.time;
        }

        ecgGraph.UpdateGraph(simulationTime);

        if (currentPhaseIndex < ecgData.phases.Count)
        {
            ECGPhase nextPhase = ecgData.phases[currentPhaseIndex];
            if (simulationTime >= nextPhase.entry)
            {
                UnityEngine.Debug.Log($"Starting Phase: {nextPhase.phase} at {simulationTime:F2}s");

                if (heartAnimationController != null)
                {
                    heartAnimationController.PlayPhase(nextPhase.phase, (float)nextPhase.duration, timeScale);
                }

                if (nextPhase.phase == "QRS")
                {
                    virtualQRSCount++;
                    if (virtualEcgGraph != null)
                        virtualEcgGraph.InjectQRSComplex();                    // ✅ Inject QRS into the virtual graph
                }


                currentPhaseIndex++;
            }
        }

        UpdateBPMDisplay();
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
        if (speed != null)
        {
            speed.text = $"Speed: {value*2:F1}x";
        }
    }
}
