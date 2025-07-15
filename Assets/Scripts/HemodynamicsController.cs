using UnityEngine;

public class HemodynamicsController : MonoBehaviour
{
    [Header("Fake Fill Objects")]
    public GameObject rightAtriaFill;
    public GameObject rightVentricleFill;

    [Header("Fade Settings")]
    public float pulseDuration = 0.5f;

    private Material atriaMaterial;
    private Material ventricleMaterial;
    private Color atriaColor;
    private Color ventricleColor;

    void Start()
    {
        if (rightAtriaFill != null)
        {
            atriaMaterial = rightAtriaFill.GetComponentInChildren<Renderer>().material;
            atriaColor = atriaMaterial.color;
        }

        if (rightVentricleFill != null)
        {
            ventricleMaterial = rightVentricleFill.GetComponentInChildren<Renderer>().material;
            ventricleColor = ventricleMaterial.color;
        }
    }

    public void TriggerAtrialContraction()
    {
        if (atriaMaterial != null)
            StartCoroutine(FadePulse(atriaMaterial, atriaColor));
    }

    public void TriggerVentricularEjection()
    {
        if (ventricleMaterial != null)
            StartCoroutine(FadePulse(ventricleMaterial, ventricleColor));
    }

    private System.Collections.IEnumerator FadePulse(Material mat, Color baseColor)
    {
        float time = 0f;
        while (time < pulseDuration)
        {
            float alpha = Mathf.Lerp(0f, baseColor.a, 1 - (time / pulseDuration));
            mat.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            time += Time.deltaTime;
            yield return null;
        }
        mat.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
    }
}
