using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHud : MonoBehaviour
{
    public static UIHud I; // ✅ correct singleton type

    [Header("HUD — Timer & Score")]
    [Tooltip("Ring Image with Fill Amount 0..1 (optional)")]
    public Image timerRing;
    [Tooltip("Timer text (e.g., 02:13)")]
    public TMP_Text timerText;

    [Tooltip("Candy count text")]
    public TMP_Text candyText;
    [Tooltip("Multiplier text (e.g., x3). Leave null if unused.")]
    public TMP_Text multiplierText;

    [Header("Approach Prompt")]
    public GameObject approachRoot;
    public Button approachButton;
    public TMP_Text approachText;

    [Header("Door POV UI")]
    public GameObject doorPovRoot;      // panel shown during POV
    public Button knockButton;          // "Knock"
    public Button walkAwayButton;       // "Walk Away"
    public TMP_Text doorHintText;       // e.g., "Space: Knock • Esc: Walk Away"

    [Header("MiniGame UI")]
    public GameObject miniGameRoot;     // panel shown during QTE
    public TMP_Text miniGameText;       // instructions like "Press LEFT, UP, RIGHT…"

    // Internal callbacks
    System.Action onApproachClick;
    System.Action onKnock;
    System.Action onWalkAway;
    bool povHotkeysEnabled = true;

    void Awake()
    {
        // Simple singleton; optional DontDestroyOnLoad if needed
        I = this;
    }

    void Start()
    {
        if (approachButton) approachButton.onClick.AddListener(() => onApproachClick?.Invoke());
        if (knockButton)    knockButton.onClick.AddListener(() => onKnock?.Invoke());
        if (walkAwayButton) walkAwayButton.onClick.AddListener(() => onWalkAway?.Invoke());

        // Default UI off
        ShowApproachPrompt(false, null);
        ShowDoorPOVUI(false, null, null);
        ShowMiniGame(false, "", null);

        // Initialize HUD text
        if (timerText)      timerText.text = "--:--";
        if (candyText)      candyText.text = "0";
        if (multiplierText) multiplierText.text = "x1";
        if (timerRing)      timerRing.fillAmount = 0f;
    }

    void Update()
    {
        // Keyboard shortcuts while in POV
        if (doorPovRoot && doorPovRoot.activeSelf && povHotkeysEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Space))  onKnock?.Invoke();
            if (Input.GetKeyDown(KeyCode.Escape)) onWalkAway?.Invoke();
        }
    }

    // ================= HUD API =================

    /// <summary>
    /// Set the timer visuals.
    /// </summary>
    /// <param name="normalized">0..1 progress (e.g., timeLeft / totalTime)</param>
    /// <param name="secondsLeft">Whole seconds remaining</param>
    public void SetTimer(float normalized, int secondsLeft)
    {
        if (timerRing) timerRing.fillAmount = Mathf.Clamp01(normalized);
        if (timerText)
        {
            int m = Mathf.Max(0, secondsLeft) / 60;
            int s = Mathf.Max(0, secondsLeft) % 60;
            timerText.text = $"{m:00}:{s:00}";
        }
    }

    /// <summary>
    /// Update candy count label.
    /// </summary>
    public void SetCandy(int amount)
    {
        if (candyText) candyText.text = amount.ToString();
        // Optional: punch-scale animation or sparkle FX here
    }

    /// <summary>
    /// Update multiplier label (format xN).
    /// </summary>
    public void SetMultiplier(int value)
    {
        if (multiplierText) multiplierText.text = $"x{Mathf.Max(1, value)}";
    }

    // ============== Approach Prompt ==============

    public void ShowApproachPrompt(bool show, System.Action onButton)
    {
        // Guard: do not show while in POV
        if (doorPovRoot && doorPovRoot.activeSelf) show = false;

        onApproachClick = onButton;
        if (approachRoot) approachRoot.SetActive(show);
    }

    // ============== Door POV UI ==============

    public void ShowDoorPOVUI(bool show, System.Action onKnock, System.Action onWalkAway)
    {
        this.onKnock = onKnock;
        this.onWalkAway = onWalkAway;

        if (doorPovRoot) doorPovRoot.SetActive(show);
        ShowDoorPOVButtons(show); // buttons visible by default with POV
    }

    public void ShowDoorPOVButtons(bool show)
    {
        if (!doorPovRoot) return;
        if (knockButton)    knockButton.gameObject.SetActive(show);
        if (walkAwayButton) walkAwayButton.gameObject.SetActive(show);
        if (doorHintText)   doorHintText.gameObject.SetActive(show);
    }

    // ============== MiniGame UI ==============

    public void ShowMiniGame(bool show, string instruction, System.Action<bool> onCompletePlaceholder)
    {
        if (miniGameRoot) miniGameRoot.SetActive(show);
        if (miniGameText) miniGameText.text = instruction ?? "";
        // Hide POV buttons while mini-game is up
        if (show) ShowDoorPOVButtons(false);
    }

    public void SetMiniGameInstruction(string instruction)
    {
        if (miniGameText) miniGameText.text = instruction;
    }

    // ============== Feedback Helpers ==============

    /// <summary>
    /// Optional little UI tap for feedback (e.g., on Knock).
    /// Implement however you like (DOTween, manual lerp, CrossFade, etc.)
    /// </summary>
    public void PlayKnockFeedback()
    {
        // Example: brief alpha blink on hint text
        // if (doorHintText) StartCoroutine(BlinkTMP(doorHintText, 0.05f, 0.2f));
    }

    // Example coroutine blink (not used by default)
    System.Collections.IEnumerator BlinkTMP(TMP_Text text, float fade, float hold)
    {
        if (!text) yield break;
        text.alpha = 0.5f; yield return new WaitForSeconds(fade);
        text.alpha = 1f;   yield return new WaitForSeconds(hold);
    }
    
    public void SetPOVHotkeysEnabled(bool enabled)
    {
        povHotkeysEnabled = enabled;
        // Optional: also gray out buttons instead of hiding
        if (knockButton)    knockButton.interactable = enabled;
        if (walkAwayButton) walkAwayButton.interactable = enabled;
    }
}
