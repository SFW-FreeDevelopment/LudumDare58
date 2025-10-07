using UnityEngine;
using System;
using System.Collections.Generic;

public class CandyCatchMiniGame : MonoBehaviour
{
    [Header("Prefabs & Area")]
    public GameObject bucketPrefab;
    public List<GameObject> candyPrefabs;

    [Tooltip("Auto-align the play area to the current camera view each time Run() is called.")]
    public bool autoAlignToCamera = true;
    public Camera camOverride;               // leave null to use Camera.main

    [Tooltip("Margins from camera edges (world units).")]
    public float marginLeft = 0.5f, marginRight = 0.5f, marginTop = 0.8f, marginBottom = 1.0f;

    [Tooltip("Computed/Manual area bounds (world space).")]
    public Vector2 areaMin = new Vector2(-6f, -3.5f);
    public Vector2 areaMax = new Vector2( 6f,  3.5f);

    [Header("Bucket")]
    public float bucketY = -3.0f;
    public float bucketMoveSpeed = 10f;

    [Header("Spawning")]
    public float spawnY = 3.5f;
    public float spawnInterval = 0.35f;
    public float candyCleanupY = -5.0f;

    [Header("Sorting (visual layering)")]
    public string sortingLayerName = "UI";   // make sure this exists, or use "Default"
    public int sortingOrder = 500;

    // Style the fall
    [Header("Candy Fall Style")]
    [Tooltip("Random initial rotation in degrees (e.g., -25..25).")]
    public Vector2 spawnAngleDeg = new Vector2(-25f, 25f);
    [Tooltip("Random angular velocity in deg/sec (e.g., -180..180).")]
    public Vector2 spinDegPerSec = new Vector2(-180f, 180f);
    [Tooltip("Small horizontal push so candies drift left/right a bit.")]
    public Vector2 lateralImpulse = new Vector2(-1.5f, 1.5f);
    [Tooltip("Downward impulse added on spawn (optional). Set 0 for none).")]
    public float downwardImpulse = 0.0f;
    [Tooltip("Gravity scale for candies (1 = default).")]
    public float candyGravityScale = 0.8f;

    // NEW: Fail-safe
    [Header("Fail-Safe")]
    [Tooltip("Seconds to wait after the LAST candy spawns before force-finishing if candies remain.")]
    public float afterLastSpawnTimeout = 8f; // set 5–10s to taste
    [Tooltip("Destroy candies that drift outside the play area + margin.")]
    public bool killOffscreenCandies = true;
    public float offscreenMargin = 2f;

    private int targetCount;
    private int caughtCount;
    private int spawned;
    private float spawnTimer;
    private GameObject bucketInstance;
    private readonly List<GameObject> liveCandies = new();

    private Action<int> onProgress;
    private Action<int> onComplete;
    private bool running;

    // NEW: track when last candy spawned
    private bool lastCandySpawned;
    private float lastSpawnClock;

    public void Run(int candiesToSpawn, Action<int> onProgress, Action<int> onComplete)
    {
        this.targetCount = Mathf.Max(1, candiesToSpawn);
        this.onProgress = onProgress;
        this.onComplete = onComplete;
        this.caughtCount = 0;
        this.spawned = 0;
        this.spawnTimer = 0f;
        this.running = true;

        // fail-safe init
        lastCandySpawned = false;
        lastSpawnClock = 0f;

        if (autoAlignToCamera) AlignToCamera();

        // Create bucket
        bucketInstance = Instantiate(bucketPrefab);
        bucketInstance.transform.position = new Vector3(
            Mathf.Clamp(0f, areaMin.x, areaMax.x), // center-ish
            bucketY,
            0f
        );
        var bucketCtrl = bucketInstance.GetComponent<BucketController>();
        if (bucketCtrl)
        {
            bucketCtrl.speed = bucketMoveSpeed;
            bucketCtrl.boundsMin = new Vector2(areaMin.x, bucketY);
            bucketCtrl.boundsMax = new Vector2(areaMax.x, bucketY);
            bucketCtrl.CenterBucket();
        }
        var collector = bucketInstance.GetComponentInChildren<BucketCollector>(includeInactive: true);
        if (collector) collector.Init(this);

        // Put bucket on desired sorting layer/order
        ApplySorting(bucketInstance);

        onProgress?.Invoke(caughtCount);
    }

    void Update()
    {
        if (!running) return;

        // Spawning
        if (spawned < targetCount)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnCandy();
                spawned++;
                spawnTimer = spawnInterval;

                if (spawned >= targetCount)
                {
                    lastCandySpawned = true;
                    lastSpawnClock = 0f; // start timeout window now
                }
            }
        }

        // Per-candy maintenance
        for (int i = liveCandies.Count - 1; i >= 0; i--)
        {
            var c = liveCandies[i];
            if (!c) { liveCandies.RemoveAt(i); continue; }

            // Below bottom cleanup
            if (c.transform.position.y < candyCleanupY)
            {
                Destroy(c);
                liveCandies.RemoveAt(i);
                continue;
            }

            // Optional offscreen cleanup
            if (killOffscreenCandies)
            {
                if (c.transform.position.x < areaMin.x - offscreenMargin ||
                    c.transform.position.x > areaMax.x + offscreenMargin ||
                    c.transform.position.y > areaMax.y + offscreenMargin)
                {
                    Destroy(c);
                    liveCandies.RemoveAt(i);
                }
            }
        }

        // Normal completion
        if (spawned >= targetCount && liveCandies.Count == 0)
        {
            Finish();
            return;
        }

        // Fail-safe completion if candies linger
        if (lastCandySpawned && liveCandies.Count > 0)
        {
            lastSpawnClock += Time.deltaTime;
            if (lastSpawnClock >= afterLastSpawnTimeout)
            {
                for (int i = 0; i < liveCandies.Count; i++)
                    if (liveCandies[i]) Destroy(liveCandies[i]);
                liveCandies.Clear();
                Finish();
                return;
            }
        }
    }

    private void SpawnCandy()
    {
        if (candyPrefabs == null || candyPrefabs.Count == 0) return;

        var prefab = candyPrefabs[UnityEngine.Random.Range(0, candyPrefabs.Count)];
        float x = UnityEngine.Random.Range(areaMin.x, areaMax.x);
        Vector3 pos = new Vector3(x, spawnY, 0f);

        // Random initial rotation
        float zRot = UnityEngine.Random.Range(spawnAngleDeg.x, spawnAngleDeg.y);
        var go = Instantiate(prefab, pos, Quaternion.Euler(0f, 0f, zRot));
        liveCandies.Add(go);

        if (!go.GetComponent<CandyMarker>()) go.AddComponent<CandyMarker>();

        // Rigidbody setup
        var rb = go.GetComponent<Rigidbody2D>(); 
        if (!rb) rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = candyGravityScale;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // smoother falling/rotation

        // Spin
        rb.angularVelocity = UnityEngine.Random.Range(spinDegPerSec.x, spinDegPerSec.y);

        // Sideways push + optional downward kick
        float lateral = UnityEngine.Random.Range(lateralImpulse.x, lateralImpulse.y);
        Vector2 impulse = new Vector2(lateral, -Mathf.Abs(downwardImpulse));
        if (impulse.sqrMagnitude > 0f)
            rb.AddForce(impulse, ForceMode2D.Impulse);

        ApplySorting(go);
    }

    public void NotifyCandyCaught(GameObject candy)
    {
        if (!running) return;
        caughtCount++;
        onProgress?.Invoke(caughtCount);
        
        if (candy)
        {
            liveCandies.Remove(candy);
            Destroy(candy);
        }
    }

    private void Finish()
    {
        running = false;
        foreach (var c in liveCandies) if (c) Destroy(c);
        liveCandies.Clear();
        if (bucketInstance) Destroy(bucketInstance);

        var cb = onComplete;
        onComplete = null; onProgress = null;
        cb?.Invoke(caughtCount);
    }

    // ---- Camera alignment ----
    public void AlignToCamera()
    {
        var cam = camOverride != null ? camOverride : Camera.main;
        if (cam == null || !cam.orthographic) return;

        Vector3 cp = cam.transform.position;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        areaMin = new Vector2(cp.x - halfW + marginLeft,  cp.y - halfH + marginBottom);
        areaMax = new Vector2(cp.x + halfW - marginRight, cp.y + halfH - marginTop);

        // Derive spawn/bucket lanes from new bounds
        bucketY = areaMin.y + 0.25f;
        spawnY  = areaMax.y - 0.25f;
        candyCleanupY = areaMin.y - 1.0f;
    }

    private void ApplySorting(GameObject go)
    {
        foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (!string.IsNullOrEmpty(sortingLayerName))
                sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
        }
    }
}
