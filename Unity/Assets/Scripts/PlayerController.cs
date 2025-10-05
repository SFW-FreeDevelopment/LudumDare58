using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

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
            float speedFactor = Mathf.Clamp01(speedX / Mathf.Max(0.01f, speed));
            float fps = Mathf.Max(1f, baseWalkFps * speedFactor);
            float frameTime = 1f / fps;

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
        rb.velocity = new Vector2(x * speed, rb.velocity.y); // maintain gravity on Y
    }

    public void SetInteractable(IInteractable ih) => current = ih;
}
