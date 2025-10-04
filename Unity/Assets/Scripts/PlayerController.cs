using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    Rigidbody2D rb;
    float x;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        x = Input.GetAxisRaw("Horizontal");
        if (x != 0) transform.localScale = new Vector3(Mathf.Sign(x), 1, 1);
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(x * speed, rb.velocity.y); // keep gravity on Y
    }
}