using UnityEngine;

public class MoonGlowPulse : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer moonRenderer;   // main moon sprite
    public SpriteRenderer glowRenderer;   // child halo sprite (soft circle / blurred moon)

    [Header("Pulse")]
    public float pulseSpeed = 0.5f;       // pulses per second
    public float alphaMin = 0.25f;
    public float alphaMax = 0.55f;
    public float scaleMin = 1.05f;        // slight scale bloom
    public float scaleMax = 1.12f;

    [Header("Color (optional)")]
    public Color glowColor = new Color(1f, 0.95f, 0.85f, 1f); // warm off-white

    float t;

    void Reset()
    {
        if (!moonRenderer) moonRenderer = GetComponent<SpriteRenderer>();
        if (!glowRenderer && transform.childCount > 0)
            glowRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        if (glowRenderer)
        {
            glowRenderer.enabled = true;
            glowRenderer.color = glowColor;
        }
        t = Random.Range(0f, 1f);
    }

    void Update()
    {
        if (!glowRenderer) return;

        t += Time.deltaTime * pulseSpeed;
        float s = (Mathf.Sin(t * Mathf.PI * 2f) + 1f) * 0.5f; // 0..1

        float alpha = Mathf.Lerp(alphaMin, alphaMax, s);
        var c = glowRenderer.color; c.a = alpha; glowRenderer.color = c;

        float scale = Mathf.Lerp(scaleMin, scaleMax, s);
        glowRenderer.transform.localScale = new Vector3(scale, scale, 1f);
    }
}
