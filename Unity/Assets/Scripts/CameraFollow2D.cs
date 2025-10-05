using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;              // Player transform

    [Header("Follow Settings")]
    public float smoothSpeed = 5f;        // Higher = snappier camera
    public Vector2 offset;                // Offset from player (e.g. new Vector2(2f, 0))

    [Header("Bounds (optional)")]
    public bool useBounds = true;         // Toggle map clamping
    public Vector2 minBounds;             // Bottom-left world corner
    public Vector2 maxBounds;             // Top-right world corner

    private Camera cam;
    private float halfHeight;
    private float halfWidth;

    void Start()
    {
        cam = GetComponent<Camera>();
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Desired position
        Vector3 desiredPos = new Vector3(target.position.x + offset.x, 
                                         target.position.y + offset.y, 
                                         transform.position.z);

        // Smooth interpolation
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

        // Clamp to bounds if enabled
        if (useBounds)
        {
            float clampX = Mathf.Clamp(smoothedPos.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
            float clampY = Mathf.Clamp(smoothedPos.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);
            smoothedPos = new Vector3(clampX, clampY, smoothedPos.z);
        }

        transform.position = smoothedPos;
    }

    // Optional: visualize bounds in Scene view
    void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, 0),
            new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0)
        );
    }
}