using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Linked Components")]
    public DoorGlowController glow;            // optional helper for fading glow
    [SerializeField] SpriteRenderer doorRenderer;      // main door sprite
    [SerializeField] SpriteRenderer entrywayRenderer;  // background behind door

    [Header("Sprites")]
    public Sprite closedDoorSprite;
    public Sprite openDoorSprite;

    [Header("Colors")]
    [Tooltip("Dark unlit entryway color (#42321E).")]
    public Color unlitColor = new Color32(0x42, 0x32, 0x1E, 255);
    [Tooltip("Warm lit entryway color (#FFD46B).")]
    public Color litColor = new Color32(0xFF, 0xD4, 0x6B, 255);
    public float glowFadeTime = 0.35f;

    [Header("Animation Settings")]
    public float knockDelay = 0.2f;   // delay between knock sounds
    public int knockCount = 2;        // how many times to knock
    public float openDelay = 0.6f;    // wait after last knock before opening

    [Header("Audio")]
    public AudioClip knockSfx;
    public AudioClip openSfx;

    AudioSource audioSource;
    bool isOpen = false;
    bool opening = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Initialize door + entryway
        if (!doorRenderer) doorRenderer = GetComponent<SpriteRenderer>();
        if (doorRenderer && closedDoorSprite)
            doorRenderer.sprite = closedDoorSprite;
        if (entrywayRenderer)
            entrywayRenderer.color = unlitColor;
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
            if (audioSource && knockSfx)
                audioSource.PlayOneShot(knockSfx);
            yield return new WaitForSeconds(knockDelay);
        }

        yield return new WaitForSeconds(openDelay);

        // --- Open door ---
        isOpen = true;
        if (audioSource && openSfx)
            audioSource.PlayOneShot(openSfx);
        if (doorRenderer && openDoorSprite)
            doorRenderer.sprite = openDoorSprite;

        // Fade the entryway from dark → warm yellow
        if (entrywayRenderer)
        {
            Color start = entrywayRenderer.color;
            Color target = litColor;
            float t = 0f;

            while (t < glowFadeTime)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / glowFadeTime);
                entrywayRenderer.color = Color.Lerp(start, target, k);
                yield return null;
            }
            entrywayRenderer.color = target;
        }

        // Optional: if using DoorGlowController, trigger it too
        if (glow) glow.SetLit(true);

        opening = false;
        Debug.Log($"[DoorController] Door '{name}' opened!");
    }

    public void ResetDoor()
    {
        isOpen = false;
        opening = false;
        StopAllCoroutines();

        if (doorRenderer && closedDoorSprite)
            doorRenderer.sprite = closedDoorSprite;

        if (entrywayRenderer)
            entrywayRenderer.color = unlitColor;

        if (glow) glow.SetLit(false);
    }

    public bool IsOpen => isOpen;
}
