using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    [Tooltip("Extra % applied to speed and walk FPS while holding Shift.")]
    public float runBonusPercent = 25f; // 25% faster when running

    [Header("Sprites (Idle + 3 Walk Frames)")]
    public Sprite idleSprite;
    public Sprite walkSpriteA;
    public Sprite walkSpriteB;
    public Sprite walkSpriteC;
    public float baseWalkFps = 8f;       // walk speed (frames per second)
    public float moveThreshold = 0.05f;  // min movement before animating

    Rigidbody2D rb;
    SpriteRenderer sr;

    float x;
    bool canControl = true;
    bool isRunning; // Shift key

    // Animation state
    float animTimer;
    int walkIndex;       // 0–2 (A,B,C)
    int walkDir = 1;     // 1 = forward, -1 = backward (for ping-pong)
    
    // Current interact target set by triggers
    IInteractable current;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr && idleSprite) sr.sprite = idleSprite;
    }

    public void EnableControl(bool on)
    {
        canControl = on;
        if (!on)
        {
            x = 0f;
            isRunning = false;
            rb.velocity = new Vector2(0, rb.velocity.y);
            if (sr && idleSprite) sr.sprite = idleSprite; // lock to idle in POV
            animTimer = 0f;
            walkIndex = 0;
            walkDir = 1;
        }
    }

    void Update()
    {
        if (!canControl) return;

        x = Input.GetAxisRaw("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Flip facing based on movement
        if (x != 0)
            transform.localScale = new Vector3(Mathf.Sign(x), 1, 1);

        // Interact
        if (Input.GetKeyDown(KeyCode.Space) && current != null)
            current.Interact(this);

        // --- Sprite animation ---
        float speedX = Mathf.Abs(rb.velocity.x);
        if (speedX > moveThreshold)
        {
            // Effective multipliers when running
            float runMul = 1f + (runBonusPercent * 0.01f);
            float effectiveMaxSpeed = speed * (isRunning ? runMul : 1f);

            // Scale FPS with movement speed, then also boost base FPS when running
            float speedFactor = Mathf.Clamp01(speedX / Mathf.Max(0.01f, effectiveMaxSpeed));
            float effectiveFps = Mathf.Max(1f, baseWalkFps * (isRunning ? runMul : 1f) * speedFactor);
            float frameTime = 1f / effectiveFps;

            animTimer += Time.deltaTime;
            if (animTimer >= frameTime)
            {
                animTimer -= frameTime;
                walkIndex += walkDir;

                // Reverse direction at ends (ping-pong)
                if (walkIndex >= 2) { walkIndex = 2; walkDir = -1; }
                else if (walkIndex <= 0) { walkIndex = 0; walkDir = 1; }
            }

            if (sr)
            {
                switch (walkIndex)
                {
                    case 0: sr.sprite = walkSpriteA; break;
                    case 1: sr.sprite = walkSpriteB; break;
                    case 2: sr.sprite = walkSpriteC; break;
                }
            }
        }
        else
        {
            // Idle
            animTimer = 0f;
            walkIndex = 0;
            walkDir = 1;
            if (sr && idleSprite) sr.sprite = idleSprite;
        }
    }

    void FixedUpdate()
    {
        float runMul = isRunning ? (1f + runBonusPercent * 0.01f) : 1f;
        rb.velocity = new Vector2(x * speed * runMul, rb.velocity.y); // maintain gravity on Y
    }

    public void SetInteractable(IInteractable ih) => current = ih;
    
    // In PlayerController.cs
    public void SetPhysicsSimulated(bool on)
    {
        if (rb) rb.simulated = on;                // disables ALL 2D physics on the player
        // (Optional) also disable colliders if you have special triggers:
        // foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = on;
    }
}
