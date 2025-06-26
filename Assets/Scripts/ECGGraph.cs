using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ECGGraph : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private float[] fullEcgSignal;

    [Header("Graph Settings")]
    public int pointsOnScreen = 500;          // Number of visible points
    public float graphWidth = 34f;            // Width of the graph on screen (in world units)
    public float amplitude = 5f;             // Vertical scaling of ECG waveform

    [Header("ECG Data Settings")]
    public float sampleRate = 360f;           // Samples per second (e.g. MIT-BIH is 360Hz)

    [Header("Positioning")]
    public Vector3 graphStartPosition = new Vector3(7f, 17f, -0.5f); // Bottom-left corner of ECG monitor

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = pointsOnScreen;
        lineRenderer.useWorldSpace = true;
        lineRenderer.widthMultiplier = 0.2f;

        // Assign a visible material in Inspector (e.g., Unlit/Color)
        if (lineRenderer.material == null)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = new Color(0f, 1f, 0f); 
            lineRenderer.material = mat;
        }
    }

    /// <summary>
    /// Initializes the ECG signal with raw float values and prepares for plotting.
    /// </summary>
    public void Initialize(float[] signalData)
    {
        fullEcgSignal = signalData;
        UnityEngine.Debug.Log($"✅ ECGGraph Initialized with {fullEcgSignal.Length} samples.");
    }

    /// <summary>
    /// Updates the sliding ECG graph in real-time.
    /// </summary>
    /// <param name="currentTime">Time since simulation start (seconds)</param>
    public void UpdateGraph(float currentTime)
    {
        if (fullEcgSignal == null || fullEcgSignal.Length == 0)
            return;

        int startIndex = Mathf.FloorToInt(currentTime * sampleRate);
        int maxStartIndex = Mathf.Max(0, fullEcgSignal.Length - pointsOnScreen);
        startIndex = Mathf.Clamp(startIndex, 0, maxStartIndex);

        Vector3[] positions = new Vector3[pointsOnScreen];

        for (int i = 0; i < pointsOnScreen; i++)
        {
            float x = (i / (float)(pointsOnScreen - 1)) * graphWidth;
            float y = fullEcgSignal[startIndex + i] * amplitude;
            positions[i] = graphStartPosition + new Vector3(x, y, 0);
        }

        lineRenderer.SetPositions(positions);
    }
}
