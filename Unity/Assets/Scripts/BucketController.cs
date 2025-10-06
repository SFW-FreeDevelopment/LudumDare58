using UnityEngine;

public class BucketController : MonoBehaviour
{
    public float speed = 10f;
    public Vector2 boundsMin = new Vector2(-6f, -3f);
    public Vector2 boundsMax = new Vector2( 6f, -3f);

    void Update()
    {
        float x = 0f;
        // Arrows or A/D
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))  x -= 1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) x += 1f;

        Vector3 pos = transform.position;
        pos.x += x * speed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, boundsMin.x, boundsMax.x);
        // lock Y to bounds line
        pos.y = boundsMin.y; 
        transform.position = pos;
    }
}
