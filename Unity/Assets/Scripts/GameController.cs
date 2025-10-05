using UnityEngine;
using System.Collections;

public enum GameState { Explore, DoorPOV, MiniGame }

public class GameController : MonoBehaviour
{
    public static GameController I;

    [Header("References")]
    public Camera mainCamera;
    public MonoBehaviour followCameraScript;   // your CameraFollow2D
    public Transform cameraRig;                // optional, else leave null to use mainCamera.transform
    public UIHud ui;
    public PlayerController player;

    [Header("POV Settings")]
    public float camMoveTime = 0.35f;          // move time to door and back
    public float zoomTime = 0.25f;             // zoom in/out time
    public float exploreOrthoSize = 4.5f;      // default zoom while exploring
    public float povOrthoSize = 3.2f;          // tighter zoom at door
    public Vector3 povOffset = Vector3.zero;   // extra XY offset at door

    GameState _state = GameState.Explore;
    Transform _lastDoorAnchor;

    void Awake()
    {
        I = this;
        if (!cameraRig) cameraRig = mainCamera.transform;
        mainCamera.orthographicSize = exploreOrthoSize;
    }

    public GameState State => _state;

    // === STATE TRANSITIONS ===
    public void EnterDoorPOV(Transform doorAnchor)
    {
        if (_state != GameState.Explore) return;

        _state = GameState.DoorPOV;
        _lastDoorAnchor = doorAnchor;

        // NEW: make sure the approach prompt is hidden
        ui.ShowApproachPrompt(false, null);

        player.EnableControl(false);
        if (followCameraScript) followCameraScript.enabled = false;

        StopAllCoroutines();
        StartCoroutine(Co_MoveCamAndZoom(
        toPos: GetPOVPos(doorAnchor),
        toSize: povOrthoSize,
        after: () => ui.ShowDoorPOVUI(true, onKnock: Knock, onWalkAway: ExitDoorPOV)
        ));
    }


    public void ExitDoorPOV()
    {
        if (_state != GameState.DoorPOV) return;

        ui.ShowDoorPOVUI(false);
        StopAllCoroutines();
        StartCoroutine(Co_MoveCamAndZoom(
            toPos: new Vector3(player.transform.position.x, player.transform.position.y, cameraRig.position.z),
            toSize: exploreOrthoSize,
            after: () =>
            {
                if (followCameraScript) followCameraScript.enabled = true;
                player.EnableControl(true);
                _state = GameState.Explore;
            }
        ));
    }

    // === ACTIONS ===
    public void Knock()
    {
        if (_state != GameState.DoorPOV) return;

        // Stub for now — this is where you’ll launch your Trick/Treat or mini-game.
        // Example: MiniGameRunner.I.Play(currentHouseDef, OnMiniGameDone);
        Debug.Log("[Knock] TODO: start trick/treat or micro-puzzle here.");

        // Temporary feedback (optional): tiny camera nudge or UI blink could go here.
        ui.PlayKnockFeedback();

        // If you want to immediately return to explore for now, uncomment:
        // ExitDoorPOV();
    }

    // === HELPERS ===
    Vector3 GetPOVPos(Transform anchor)
    {
        var z = cameraRig.position.z;
        return new Vector3(anchor.position.x + povOffset.x, anchor.position.y + povOffset.y, z);
    }

    IEnumerator Co_MoveCamAndZoom(Vector3 toPos, float toSize, System.Action after)
    {
        Vector3 fromPos = cameraRig.position;
        float fromSize = mainCamera.orthographicSize;

        float maxT = Mathf.Max(camMoveTime, zoomTime);
        float t = 0f;
        while (t < maxT)
        {
            t += Time.deltaTime;
            float kMove = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / camMoveTime));
            float kZoom = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / zoomTime));

            cameraRig.position = Vector3.Lerp(fromPos, toPos, kMove);
            mainCamera.orthographicSize = Mathf.Lerp(fromSize, toSize, kZoom);
            yield return null;
        }
        cameraRig.position = toPos;
        mainCamera.orthographicSize = toSize;
        after?.Invoke();
    }
}
