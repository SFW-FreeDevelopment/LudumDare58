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
        // Hide the POV buttons (Knock/Walk Away) and show mini-game panel
        ui.ShowDoorPOVButtons(false);
        ui.ShowMiniGame(true, "…", null); // text will be set by the minigame

        // Create a simple QTE runner under the UI (or any manager object)
        // You can also add this component to a prefab if you prefer.
        activeMiniGame = ui.gameObject.AddComponent<MiniGameArrowsQTE>();
        activeMiniGame.Run(3, // sequence length
        onTextChanged: txt => ui.SetMiniGameInstruction(txt),
        onComplete: success =>
        {
            // Clean up runner
            if (activeMiniGame) Destroy(activeMiniGame);
            activeMiniGame = null;

            ui.ShowMiniGame(false, "", null);
            ui.ShowDoorPOVButtons(true); // bring buttons back if you want

            if (success)
            {
                int reward = Random.Range(1, 11); // 1..10 inclusive
                Debug.Log($"[MiniGame] Success! Candy +{reward}");

                // Award candy using your existing GameManager if present
                if (GameManager.I != null) GameManager.I.AddCandy(reward);
                else Debug.Log($"[GameController] (No GameManager) Would have added {reward} candy.");

                // Optional: auto-exit POV on success
                ExitDoorPOV();
            }
            else
            {
                Debug.Log("[MiniGame] Failed.");
                // You could allow retry, keep POV open, etc.
            }
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
