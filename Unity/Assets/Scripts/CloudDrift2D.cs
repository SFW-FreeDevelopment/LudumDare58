using UnityEngine;

/// <summary>
/// Gentle, visible cloud motion for 2D games (Built-in RP friendly).
/// - Horizontal drift with optional random direction and jitter
/// - Sine bobbing
/// - Wind gust modulation (subtle speed variance)
/// - Depth-based speed scaling using Z position
/// - World-space wrap, with optional camera-derived bounds
/// </summary>
public class CloudDrift2D : MonoBehaviour
{
    [Header("Drift")]
    [Tooltip("Base horizontal speed in world units/sec (positive = right, negative = left).")]
    public float speed = 3f;
    [Tooltip("Random speed offset added on Start().")]
    public Vector2 randomSpeedJitter = new Vector2(-0.5f, 0.5f);
    [Tooltip("50% chance to flip direction on Start().")]
    public bool randomReverse = true;

    [Header("Bob")]
    [Tooltip("Vertical bob amplitude in units.")]
    public float bobAmplitude = 0.2f;
    [Tooltip("Bob frequency in cycles per second.")]
    public float bobFrequency = 0.15f;

    [Header("Wind Gust (optional)")]
    public bool windGustEnabled = true;
    [Tooltip("Gust frequency in cycles per second (very low feels natural).")]
    public float gustFrequency = 0.1f;   // ~ one cycle every 10 sec
    [Tooltip("Gust amplitude as a multiplier around 1.0 (0.3 => 0.7..1.3).")]
    public float gustAmplitude = 0.3f;

    [Header("Depth-Based Speed Scaling")]
    public bool depthScaleEnabled = true;
    [Tooltip("Z range used to map a relative depth factor (nearZ -> nearMultiplier, farZ -> farMultiplier).")]
    public Vector2 zRange = new Vector2(-5f, 5f);
    [Tooltip("Speed multiplier at nearZ (usually > 1).")]
    public float nearMultiplier = 1.6f;
    [Tooltip("Speed multiplier at farZ (usually < 1).")]
    public float farMultiplier = 0.6f;

    [Header("Wrap (World Space)")]
    public bool enableWrap = true;
    [Tooltip("Left X world coordinate at which cloud wraps to right.")]
    public float leftX = -60f;
    [Tooltip("Right X world coordinate at which cloud wraps to left.")]
    public float rightX = 60f;

    [Header("Auto-Bounds From Camera")]
    [Tooltip("Compute wrap bounds from the active orthographic camera each frame (adds margins).")]
    public bool autoBoundsFromCamera = false;
    public Camera camOverride;                 // null => Camera.main
    [Tooltip("Margins (world units) from camera edges when auto-bounds is on.")]
    public float marginLeft = 5f, marginRight = 5f;

    // Runtime state
    float baseY;
    float phase;
    float actualSpeed;

    void Start()
    {
        baseY = transform.position.y;
        phase = Random.Range(0f, Mathf.PI * 2f);

        // Base speed + jitter
        actualSpeed = speed + Random.Range(randomSpeedJitter.x, randomSpeedJitter.y);
        if (randomReverse && Random.value < 0.5f) actualSpeed = -actualSpeed;

        // Depth scaling via Z
        if (depthScaleEnabled)
        {
            float t = Mathf.InverseLerp(zRange.x, zRange.y, transform.position.z);
            float depthMul = Mathf.Lerp(nearMultiplier, farMultiplier, t);
            actualSpeed *= depthMul;
        }
    }

    void Update()
    {
        // Optional auto-bounds from camera (safe for moving camera)
        if (autoBoundsFromCamera)
            ComputeBoundsFromCamera();

        Vector3 p = transform.position;

        // Wind gust modulation
        float gustMul = 1f;
        if (windGustEnabled)
        {
            // Produces ~0.7..1.3 with default amplitude=0.3
            float s = Mathf.Sin(Time.time * Mathf.PI * 2f * Mathf.Max(0.01f, gustFrequency));
            gustMul = 1f + s * gustAmplitude;
        }

        // Horizontal drift
        p.x += actualSpeed * gustMul * Time.deltaTime;

        // Vertical bob (nice and slow)
        phase += bobFrequency * Mathf.PI * 2f * Time.deltaTime;
        p.y = baseY + Mathf.Sin(phase) * bobAmplitude;

        // Wrap around
        if (enableWrap && leftX < rightX)
        {
            if (p.x > rightX) p.x = leftX;
            else if (p.x < leftX) p.x = rightX;
        }

        transform.position = p;
    }

    /// <summary>Compute left/right wrap coordinates from an orthographic camera.</summary>
    void ComputeBoundsFromCamera()
    {
        var cam = camOverride ? camOverride : Camera.main;
        if (!cam || !cam.orthographic) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float cx = cam.transform.position.x;

        leftX  = cx - halfW + marginLeft;
        rightX = cx + halfW - marginRight;
    }

    // --- Optional helpers ---

    /// <summary>Set wrap bounds at runtime.</summary>
    public void SetWrap(float left, float right)
    {
        leftX = left; rightX = right;
    }

    /// <summary>Push a new base speed (actualSpeed keeps jitter/scale; call RebuildSpeed to recompute fully).</summary>
    public void SetBaseSpeed(float newSpeed)
    {
        speed = newSpeed;
        actualSpeed = speed; // keeps previous jitter/scale out; call RebuildSpeed for full recompute
    }

    /// <summary>Recalculate actual speed including jitter, direction, and depth scaling.</summary>
    public void RebuildSpeed()
    {
        actualSpeed = speed + Random.Range(randomSpeedJitter.x, randomSpeedJitter.y);
        if (randomReverse && Random.value < 0.5f) actualSpeed = -actualSpeed;

        if (depthScaleEnabled)
        {
            float t = Mathf.InverseLerp(zRange.x, zRange.y, transform.position.z);
            float depthMul = Mathf.Lerp(nearMultiplier, farMultiplier, t);
            actualSpeed *= depthMul;
        }
    }
}
