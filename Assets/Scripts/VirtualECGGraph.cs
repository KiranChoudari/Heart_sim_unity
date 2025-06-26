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
    private float[] pWave;
    private float[] qDip;
    private float[] rSpike;
    private float[] sDip;
    private float[] tWave;

    private float sampleRate = 360f; // 360 samples per second (MIT-BIH standard)

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.widthMultiplier = 0.2f;
        lineRenderer.positionCount = pointsOnScreen;

        for (int i = 0; i < pointsOnScreen; i++)
            signalBuffer.Add(0f);

        pWave = GenerateSpike(30, 0.3f);   // subtle spike
        qDip = GenerateDip(10, 0.2f);      // shallow dip
        rSpike = GenerateSpike(20, 1.0f);  // main spike
        sDip = GenerateDip(10, 0.3f);      // shallow dip
        tWave = GenerateSpike(40, 0.5f);   // broad mild spike
    }

    void Update()
    {
        UpdateGraph();
    }

    public void InjectQRSComplex()
    {
        int delayBeforeQ = Mathf.RoundToInt(sampleRate * 0.200f); // P-wave start
        int delayBeforeR = Mathf.RoundToInt(sampleRate * 0.010f); // Q-dip before R
        int delayAfterR = Mathf.RoundToInt(sampleRate * 0.010f);  // S-dip after R
        int delayAfterS = Mathf.RoundToInt(sampleRate * 0.010f);  // T-wave after S

        InsertBlank(delayBeforeQ);
        InsertWave(pWave);
        InsertBlank(delayBeforeR - pWave.Length);
        InsertWave(qDip);
        InsertWave(rSpike);
        InsertWave(sDip);
        InsertBlank(delayAfterS);
        InsertWave(tWave);
    }

    void InsertBlank(int count)
    {
        for (int i = 0; i < count; i++)
            signalBuffer.Add(0f);
    }

    void InsertWave(float[] wave)
    {
        foreach (var point in wave)
            signalBuffer.Add(point);
    }

    void UpdateGraph()
    {
        signalBuffer.Add(0f); // continuous scroll
        if (signalBuffer.Count > pointsOnScreen)
            signalBuffer.RemoveAt(0);

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
}
