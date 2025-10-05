using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    Rigidbody2D rb;
    float x;
    bool canControl = true;

    // Current interact target set by triggers
    IInteractable current;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void EnableControl(bool on) { canControl = on; if(!on) rb.velocity = new Vector2(0, rb.velocity.y); }

    void Update()
    {
        if (!canControl) { x = 0f; return; }

        x = Input.GetAxisRaw("Horizontal");
        if (x != 0) transform.localScale = new Vector3(Mathf.Sign(x), 1, 1);

        // Keyboard interact (Space)
        if (Input.GetKeyDown(KeyCode.Space) && current != null)
            current.Interact(this);
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(x * speed, rb.velocity.y);
    }

    public void SetInteractable(IInteractable ih) => current = ih;
}
