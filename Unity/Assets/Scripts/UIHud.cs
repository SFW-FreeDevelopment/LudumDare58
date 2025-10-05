using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHud : MonoBehaviour
{
    [Header("Approach Prompt")]
    public GameObject approachRoot;
    public Button approachButton;
    public TMP_Text approachText;

    [Header("Door POV UI")]
    public GameObject doorPovRoot;      // panel shown during POV
    public Button knockButton;          // "Knock"
    public Button walkAwayButton;       // "Walk Away"
    public TMP_Text doorHintText;       // e.g., "Space: Knock  •  Esc: Walk Away"

    System.Action onApproachClick;
    System.Action onKnock;
    System.Action onWalkAway;

    void Start()
    {
        if (approachButton) approachButton.onClick.AddListener(() => onApproachClick?.Invoke());

        if (knockButton) knockButton.onClick.AddListener(() => onKnock?.Invoke());
        if (walkAwayButton) walkAwayButton.onClick.AddListener(() => onWalkAway?.Invoke());

        ShowApproachPrompt(false, null);
        ShowDoorPOVUI(false, null, null);
    }

    public void ShowApproachPrompt(bool show, System.Action onButton)
    {
        if (doorPovRoot && doorPovRoot.activeSelf) show = false;

        onApproachClick = onButton;
        if (approachRoot) approachRoot.SetActive(show);
    }


    public void ShowDoorPOVUI(bool show, System.Action onKnock = null, System.Action onWalkAway = null)
    {
        this.onKnock = onKnock;
        this.onWalkAway = onWalkAway;
        if (doorPovRoot) doorPovRoot.SetActive(show);

        // Optional safety: if someone disables POV UI externally, ensure reset
        if (!show && GameController.I != null)
        {
            // Nothing else needed; GameController.ExitDoorPOV() already resets the door.
            // Just make sure all exits flow through ExitDoorPOV().
        }
    }

    void Update()
    {
        // Keyboard shortcuts while in POV
        if (doorPovRoot && doorPovRoot.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space)) onKnock?.Invoke();
            if (Input.GetKeyDown(KeyCode.Escape)) onWalkAway?.Invoke();
        }
    }

    // Optional little UI tap for feedback
    public void PlayKnockFeedback()
    {
        // e.g., brief text blink, sound effect, or button punch scale (implement as you like)
        // doorHintText?.CrossFadeAlpha(0.4f, 0.05f, true); then back, etc.
    }
}
