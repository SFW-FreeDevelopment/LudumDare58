using UnityEngine;
using System;
using System.Collections.Generic;

public class CandyCatchMiniGame : MonoBehaviour
{
    [Header("Prefabs & Area")]
    public GameObject bucketPrefab;           // prefab with BucketController + BucketCollector
    public List<GameObject> candyPrefabs;     // your different candy prefabs (must have Rigidbody2D + Collider2D + CandyMarker)
    [Tooltip("World-space min (x,y) and max (x,y) of the play area (visible on camera).")]
    public Vector2 areaMin = new Vector2(-6f, -3.5f);
    public Vector2 areaMax = new Vector2( 6f,  3.5f);

    [Header("Bucket")]
    public float bucketY = -3.0f;             // fixed Y where the bucket sits
    public float bucketMoveSpeed = 10f;       // horizontal move speed

    [Header("Spawning")]
    public float spawnY = 3.5f;               // Y from which candy drops
    public float spawnInterval = 0.35f;       // time between spawns
    public float candyCleanupY = -5.0f;       // if candies fall below this, they are removed

    private int targetCount;
    private int caughtCount;
    private int spawned;
    private float spawnTimer;
    private GameObject bucketInstance;
    private readonly List<GameObject> liveCandies = new();

    private Action<int> onProgress;
    private Action<int> onComplete;
    private bool running;

    public void Run(int candiesToSpawn, Action<int> onProgress, Action<int> onComplete)
    {
        this.targetCount = Mathf.Max(1, candiesToSpawn);
        this.onProgress = onProgress;
        this.onComplete = onComplete;
        this.caughtCount = 0;
        this.spawned = 0;
        this.spawnTimer = 0f;
        this.running = true;

        // Create bucket
        bucketInstance = Instantiate(bucketPrefab, new Vector3(0, spawnY, 0), Quaternion.identity, transform);
        var bucketCtrl = bucketInstance.GetComponent<BucketController>();
        if (bucketCtrl)
        {
            bucketCtrl.speed = bucketMoveSpeed;
            bucketCtrl.boundsMin = new Vector2(areaMin.x, bucketY);
            bucketCtrl.boundsMax = new Vector2(areaMax.x, bucketY);
        }

        var collector = bucketInstance.GetComponentInChildren<BucketCollector>(includeInactive: true);
        if (collector)
        {
            collector.Init(this);
        }

        // Ensure immediate progress text
        onProgress?.Invoke(caughtCount);
    }

    void Update()
    {
        if (!running) return;

        // Spawn candies at interval until we reach targetCount
        if (spawned < targetCount)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnCandy();
                spawned++;
                spawnTimer = spawnInterval;
            }
        }

        // Cleanup candies that fell off-screen
        for (int i = liveCandies.Count - 1; i >= 0; i--)
        {
            if (!liveCandies[i]) { liveCandies.RemoveAt(i); continue; }
            if (liveCandies[i].transform.position.y < candyCleanupY)
            {
                Destroy(liveCandies[i]);
                liveCandies.RemoveAt(i);
            }
        }

        // End when all candies are either caught or gone AND we've spawned them all
        if (spawned >= targetCount && liveCandies.Count == 0)
        {
            Finish();
        }
    }

    private void SpawnCandy()
    {
        if (candyPrefabs == null || candyPrefabs.Count == 0) return;

        // random prefab & x position
        var prefab = candyPrefabs[UnityEngine.Random.Range(0, candyPrefabs.Count)];
        float x = UnityEngine.Random.Range(areaMin.x, areaMax.x);
        Vector3 pos = new Vector3(x, spawnY, 0f);

        var go = Instantiate(prefab, pos, Quaternion.identity, transform);
        liveCandies.Add(go);

        // Ensure candy has a marker so collector can recognize it
        if (!go.GetComponent<CandyMarker>()) go.AddComponent<CandyMarker>();

        // Make sure it has a Rigidbody2D and Collider2D set up
        var rb = go.GetComponent<Rigidbody2D>();
        if (!rb) rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    // Called by BucketCollector when a candy is caught
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

        // Cleanup any remaining spawned objects
        foreach (var c in liveCandies) if (c) Destroy(c);
        liveCandies.Clear();

        if (bucketInstance) Destroy(bucketInstance);

        var cb = onComplete;
        onComplete = null;
        onProgress = null;
        cb?.Invoke(caughtCount);
    }
}
