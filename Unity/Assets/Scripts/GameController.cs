using UnityEngine;
using System.Collections;

public enum GameState { Explore, DoorPOV, MiniGame }

public class GameController : MonoBehaviour
{
    public static GameController I;

    [Header("References")]
    public Camera mainCamera;                   // Main Camera
    public MonoBehaviour followCameraScript;    // e.g., CameraFollow2D
    public Transform cameraRig;                 // Usually the camera transform
    public UIHud ui;                            // Your UI script (Approach + POV UI)
    public PlayerController player;             // Player controller

    [Header("POV Camera Settings")]
    public float camMoveTime = 0.35f;           // move duration to/from door
    public float zoomTime = 0.25f;              // zoom in/out duration
    public float exploreOrthoSize = 4.5f;       // zoom while exploring
    public float povOrthoSize = 3.2f;           // tighter zoom at door
    public Vector3 povOffset = Vector3.zero;    // XY tweak if needed

    private GameState _state = GameState.Explore;
    private Transform _lastDoorAnchor;
    private DoorController currentDoor;
    private MiniGameArrowsQTE activeMiniGame;
    
    // --- Candy Catch Minigame refs (assign in Inspector) ---
    [Header("Candy Catch Minigame")]
    public CandyCatchMiniGame candyCatchPrefab; // prefab with the scripts below
    public int minCandyToSpawn = 1;
    public int maxCandyToSpawn = 10;

    private MiniGameArrowsQTE activeQTE;
    private CandyCatchMiniGame activeCandyCatch;

    void Awake()
    {
        I = this;
        if (!cameraRig) cameraRig = mainCamera.transform;
        if (mainCamera) mainCamera.orthographicSize = exploreOrthoSize;
    }

    public GameState State => _state;

    // ========= STATE TRANSITIONS =========

    public void EnterDoorPOV(Transform doorAnchor)
    {
        if (_state != GameState.Explore || doorAnchor == null) return;

        _state = GameState.DoorPOV;
        _lastDoorAnchor = doorAnchor;

        // Find the door for this house via the anchor's parent hierarchy
        currentDoor = doorAnchor.GetComponentInParent<DoorController>();

        var anchorRef = doorAnchor.GetComponent<DoorPOVAnchor>();
        currentDoor = anchorRef ? anchorRef.door : null;
        
        // Hide approach UI to avoid overlap
        if (ui) ui.ShowApproachPrompt(false, null);

        // Freeze player & stop follow camera
        if (player) player.EnableControl(false);
        if (followCameraScript) followCameraScript.enabled = false;

        // Smooth move + zoom into POV
        StopAllCoroutines();
        StartCoroutine(Co_MoveCamAndZoom(
            toPos: GetPOVPos(doorAnchor),
            toSize: povOrthoSize,
            after: () =>
            {
                // Show POV UI with wired actions
                if (ui) ui.ShowDoorPOVUI(true, onKnock: Knock, onWalkAway: ExitDoorPOV);
            }));
    }

    public void ExitDoorPOV()
    {
        if (_state != GameState.DoorPOV) return;

        // Hide POV UI
        if (ui) ui.ShowDoorPOVUI(false, null, null);

        // Reset the door visuals/state when leaving POV (optional)
        if (currentDoor != null)
        {
            currentDoor.ResetDoor();  // <- closes + unlights
            currentDoor = null;       // <- avoid stale refs next time
        }

        // Return camera to player & restore control
        StopAllCoroutines();
        Vector3 toPos = new Vector3(player.transform.position.x, player.transform.position.y, cameraRig.position.z);

        StartCoroutine(Co_MoveCamAndZoom(
            toPos: toPos,
            toSize: exploreOrthoSize,
            after: () =>
            {
                if (followCameraScript) followCameraScript.enabled = true;
                if (player) player.EnableControl(true);
                _state = GameState.Explore;
            }));
    }

    // ========= ACTIONS =========

    public void Knock()
    {
        if (_state != GameState.DoorPOV) return;
        if (currentDoor == null) { Debug.LogWarning("[GameController] No DoorController."); return; }

        // Prevent double-subscribe
        currentDoor.OnDoorOpened -= HandleDoorOpened;
        currentDoor.OnDoorOpened += HandleDoorOpened;

        ui.PlayKnockFeedback();
        currentDoor.KnockAndOpen();
    }
    
    private void HandleDoorOpened()
    {
        // Stop listening after one open
        if (currentDoor != null) currentDoor.OnDoorOpened -= HandleDoorOpened;
        StartMiniGame();
    }
    
private void StartMiniGame()
{
    // hide the POV buttons and show the mini-game panel
    ui.ShowDoorPOVButtons(false);
    ui.ShowMiniGame(true, "Get ready…", null);

    // Create Arrow QTE runner on the UI (or any manager object)
    activeQTE = ui.gameObject.AddComponent<MiniGameArrowsQTE>();
    activeQTE.Run(3,
        onTextChanged: txt => ui.SetMiniGameInstruction(txt),
        onComplete: success =>
        {
            if (activeQTE) Destroy(activeQTE);
            activeQTE = null;

            if (success)
            {
                // Decide how many candies to spawn (but don't award yet)
                int toSpawn = Random.Range(minCandyToSpawn, maxCandyToSpawn + 1);
                StartCandyCatch(toSpawn);
            }
            else
            {
                // You could allow retry; for now, just leave the door open.
                ui.SetMiniGameInstruction("Try again?");
                ui.ShowDoorPOVButtons(true);
            }
        });
}

private void StartCandyCatch(int toSpawn)
{
    if (!candyCatchPrefab)
    {
        Debug.LogWarning("[GameController] No CandyCatchMiniGame prefab assigned.");
        ui.SetMiniGameInstruction("Missing candy mini-game prefab!");
        ui.ShowDoorPOVButtons(true);
        return;
    }

    // Spawn the candy catch minigame under the UI (keeps it frontmost)
    activeCandyCatch = Instantiate(candyCatchPrefab, ui.transform);
    ui.SetMiniGameInstruction($"Catch the candy! (0/{toSpawn})");

    activeCandyCatch.Run(
        candiesToSpawn: toSpawn,
        onProgress: caught => ui.SetMiniGameInstruction($"Catch the candy! ({caught}/{toSpawn})"),
        onComplete: caught =>
        {
            // Clean up the mini-game instance
            if (activeCandyCatch) Destroy(activeCandyCatch.gameObject);
            activeCandyCatch = null;

            // Award only what they caught
            if (caught > 0)
            {
                if (GameManager.I != null) GameManager.I.AddCandy(caught);
                else Debug.Log($"[CandyCatch] Would have added {caught} candy.");
            }

            // Finish up this door interaction
            ui.ShowMiniGame(false, "", null);
            ExitDoorPOV();
        });
}

    // ========= HELPERS =========

    private Vector3 GetPOVPos(Transform anchor)
    {
        float z = cameraRig.position.z;
        return new Vector3(anchor.position.x + povOffset.x, anchor.position.y + povOffset.y, z);
    }

    private IEnumerator Co_MoveCamAndZoom(Vector3 toPos, float toSize, System.Action after)
    {
        Vector3 fromPos = cameraRig.position;
        float fromSize = mainCamera.orthographicSize;

        float maxT = Mathf.Max(camMoveTime, zoomTime);
        float t = 0f;

        while (t < maxT)
        {
            t += Time.deltaTime;

            float kMove = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / camMoveTime));
            float kZoom = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / zoomTime));

            cameraRig.position = Vector3.Lerp(fromPos, toPos, kMove);
            mainCamera.orthographicSize = Mathf.Lerp(fromSize, toSize, kZoom);

            yield return null;
        }

        cameraRig.position = toPos;
        mainCamera.orthographicSize = toSize;

        after?.Invoke();
    }
}
