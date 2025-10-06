using UnityEngine;

[ExecuteAlways]
public class ParallaxLayer2D : MonoBehaviour
{
    public Camera targetCamera;                 // leave null to use Camera.main
    [Tooltip("0 = sticks to camera (UI-like), 1 = world-locked; values < 1 move slower than camera (parallax).")]
    public Vector2 parallax = new Vector2(0.2f, 0.05f);
    public Vector2 additionalOffset;            // manual nudge if needed

    Vector3 startPos;
    Vector3 camStartPos;

    void OnEnable()
    {
        if (!targetCamera) targetCamera = Camera.main;
        startPos = transform.position;
        if (targetCamera) camStartPos = targetCamera.transform.position;
        UpdateParallax();
    }

    void LateUpdate()
    {
        UpdateParallax();
    }

    void UpdateParallax()
    {
        if (!targetCamera) return;
        Vector3 camPos = targetCamera.transform.position;

        Vector3 delta = camPos - camStartPos; // how far the camera moved since start
        Vector3 newPos = startPos + new Vector3(
        delta.x * (1f - parallax.x) + additionalOffset.x,
        delta.y * (1f - parallax.y) + additionalOffset.y,
        0f
        );

        // Keep original z so draw order is preserved
        newPos.z = transform.position.z;
        transform.position = newPos;
    }
}
