using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    public System.Action OnDoorOpened;

    [Header("Renderers")]
    [SerializeField] private SpriteRenderer doorRenderer;     // main door sprite
    [SerializeField] private SpriteRenderer glowRenderer;     // background glow (behind the door)
    [SerializeField] private SpriteRenderer silhouetteRenderer; // silhouette (in front of the glow)

    [Header("Door Sprites")]
    public Sprite closedDoorSprite;
    public Sprite openDoorSprite;

    [Header("Silhouette Sprites")]
    public Sprite silhouetteManSprite;
    public Sprite silhouetteWomanSprite;

    [Header("Knock/Open Timing")]
    [Tooltip("Delay between knock sounds.")]
    public float knockDelay = 0.2f;
    [Tooltip("Number of knocks before opening.")]
    public int knockCount = 2;
    [Tooltip("Pause after last knock before door opens.")]
    public float openDelay = 0.6f;

    [Header("Audio")]
    public AudioClip knockSfx;
    public AudioClip openSfx;

    private AudioSource audioSource;
    private bool isOpen = false;
    private bool opening = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (!doorRenderer) doorRenderer = GetComponent<SpriteRenderer>();

        // Initial visual state: closed door, no glow, no silhouette
        if (doorRenderer && closedDoorSprite) doorRenderer.sprite = closedDoorSprite;
        if (glowRenderer)        glowRenderer.enabled = false;
        if (silhouetteRenderer)  silhouetteRenderer.enabled = false;
    }

    public void KnockAndOpen()
    {
        if (opening || isOpen) return;
        StopAllCoroutines();
        StartCoroutine(Co_KnockAndOpen());
    }

    private IEnumerator Co_KnockAndOpen()
    {
        opening = true;

        // --- Knock sequence ---
        for (int i = 0; i < knockCount; i++)
        {
            if (audioSource && knockSfx) audioSource.PlayOneShot(knockSfx);
            yield return new WaitForSeconds(knockDelay);
        }

        // Anticipation before reveal
        yield return new WaitForSeconds(openDelay);

        // --- Open door visuals ---
        isOpen = true;

        if (audioSource && openSfx) audioSource.PlayOneShot(openSfx);
        if (doorRenderer && openDoorSprite) doorRenderer.sprite = openDoorSprite;

        // Show glow behind the open door
        if (glowRenderer) glowRenderer.enabled = true;

        // Pick a random silhouette (man/woman) and show it
        if (silhouetteRenderer)
        {
            Sprite pick = null;

            // Choose randomly between provided sprites (fallbacks handled)
            bool chooseMan = Random.value < 0.5f;
            if (chooseMan && silhouetteManSprite) pick = silhouetteManSprite;
            else if (!chooseMan && silhouetteWomanSprite) pick = silhouetteWomanSprite;

            // Fallbacks if one/both not assigned
            if (!pick) pick = silhouetteManSprite ? silhouetteManSprite : silhouetteWomanSprite;

            silhouetteRenderer.sprite = pick;
            silhouetteRenderer.enabled = (pick != null);
        }

        opening = false;
        OnDoorOpened?.Invoke();
        Debug.Log($"[DoorController] Door '{name}' opened (silhouette shown).");
    }

    public void ResetDoor()
    {
        isOpen = false;
        opening = false;
        StopAllCoroutines();

        if (doorRenderer && closedDoorSprite) doorRenderer.sprite = closedDoorSprite;

        if (glowRenderer) glowRenderer.enabled = false;

        if (silhouetteRenderer)
        {
            silhouetteRenderer.enabled = false;
            // Optional: clear sprite reference so it re-selects next time
            // silhouetteRenderer.sprite = null;
        }
    }

    public bool IsOpen => isOpen;
}
