using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I;

    [Header("Audio Sources")]
    [SerializeField] AudioSource sfxSource;   // general SFX
    [SerializeField] AudioSource loopSource;  // optional looping bgm (can stay null)

    [Header("Sound Effects")]
    public AudioClip Candy1;
    public AudioClip Candy2;
    public AudioClip Click;
    public AudioClip DoorClose;
    public AudioClip DoorOpen;
    public AudioClip GameOver;
    public AudioClip Knock;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (!sfxSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    public void Play(AudioClip clip, float volume = 1f)
    {
        if (clip && sfxSource)
            sfxSource.PlayOneShot(clip, volume);
    }

    // Convenience helpers
    public void PlayCandy()  => Play(Random.value < 0.5f ? Candy1 : Candy2);
    public void PlayClick()  => Play(Click);
    public void PlayDoorOpen() => Play(DoorOpen);
    public void PlayDoorClose() => Play(DoorClose);
    public void PlayKnock() => Play(Knock);
    public void PlayGameOver() => Play(GameOver);
}
