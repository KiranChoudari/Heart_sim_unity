using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class VirtualECGGraph : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [Header("Graph Settings")]
    public int pointsOnScreen = 500;
    public float graphWidth = 34f;
    public float amplitude = 10f;
    public Vector3 graphStartPosition = new Vector3(7f, 17f, -0.01f);

    private List<float> signalBuffer = new List<float>();
    private Queue<float> waveformInjectionQueue = new Queue<float>();

    private float[] pWave, qDip, rSpike, sDip, tWave;
    private float sampleRate = 360f;
    private float simulationTime = 0f;
    private float timeScale = 1f;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.widthMultiplier = 0.2f;
        lineRenderer.positionCount = pointsOnScreen;

        for (int i = 0; i < pointsOnScreen; i++)
            signalBuffer.Add(0f);

        // Generate synthetic waveform components
        pWave = GenerateSpike(30, 0.3f);   // subtle P-wave
        qDip = GenerateDip(10, 0.2f);     // Q-dip
        rSpike = GenerateSpike(20, 1.0f);  // main R-peak
        sDip = GenerateDip(10, 0.3f);     // S-dip
        tWave = GenerateSpike(40, 0.5f);   // broad T-wave
    }

    void Update()
    {
        simulationTime += Time.deltaTime * timeScale;
        float samplesToAdd = simulationTime * sampleRate;
        int intSamples = Mathf.FloorToInt(samplesToAdd);
        simulationTime -= intSamples / sampleRate;

        for (int i = 0; i < intSamples; i++)
        {
            float nextSample = waveformInjectionQueue.Count > 0 ? waveformInjectionQueue.Dequeue() : 0f;

            // Add noise for realism
            float noise = UnityEngine.Random.Range(-0.015f, 0.015f);             // random jitter
            float baseline = 0.03f * Mathf.Sin(2f * Mathf.PI * Time.time * 0.5f); // slow drift

            signalBuffer.Add(nextSample + noise + baseline);

            if (signalBuffer.Count > pointsOnScreen)
                signalBuffer.RemoveAt(0);
        }

        UpdateGraph();
    }

    public void InjectQRSComplex()
    {
        int delayBeforeQ = Mathf.RoundToInt(sampleRate * 0.200f);
        int delayBeforeR = Mathf.RoundToInt(sampleRate * 0.010f);
        int delayAfterR = Mathf.RoundToInt(sampleRate * 0.010f);
        int delayAfterS = Mathf.RoundToInt(sampleRate * 0.010f);

        EnqueueBlank(delayBeforeQ);
        EnqueueWave(pWave);
        EnqueueBlank(delayBeforeR - pWave.Length);
        EnqueueWave(qDip);
        EnqueueWave(rSpike);
        EnqueueWave(sDip);
        EnqueueBlank(delayAfterS);
        EnqueueWave(tWave);
    }

    void EnqueueBlank(int count)
    {
        for (int i = 0; i < count; i++)
            waveformInjectionQueue.Enqueue(0f);
    }

    void EnqueueWave(float[] wave)
    {
        foreach (var point in wave)
        {
            waveformInjectionQueue.Enqueue(point);
        }
    }

    void UpdateGraph()
    {
        Vector3[] positions = new Vector3[pointsOnScreen];
        for (int i = 0; i < pointsOnScreen; i++)
        {
            float x = (i / (float)(pointsOnScreen - 1)) * graphWidth;
            float y = signalBuffer[i] * amplitude;
            positions[i] = graphStartPosition + new Vector3(x, y, 0);
        }
        lineRenderer.SetPositions(positions);
    }

    float[] GenerateSpike(int length, float height)
    {
        float[] waveform = new float[length];
        for (int i = 0; i < length; i++)
        {
            float t = i / (float)(length - 1);
            waveform[i] = height * Mathf.Exp(-60f * Mathf.Pow(t - 0.5f, 2));
        }
        return waveform;
    }

    float[] GenerateDip(int length, float depth)
    {
        float[] waveform = new float[length];
        for (int i = 0; i < length; i++)
        {
            float t = i / (float)(length - 1);
            waveform[i] = -depth * Mathf.Exp(-60f * Mathf.Pow(t - 0.5f, 2));
        }
        return waveform;
    }

    /// <summary>
    /// Call this from external controller to sync simulation speed.
    /// </summary>
    public void SetTimeScale(float scale)
    {
        timeScale = scale;
    }
}
