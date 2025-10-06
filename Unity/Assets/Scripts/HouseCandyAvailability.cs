using UnityEngine;
using System.Collections;

public class HouseCandyAvailability : MonoBehaviour
{
    [Header("State")]
    public bool availableAtStart = true;

    [Header("Porch Light")]
    public PorchLightController porchLight; // assign the light above the door

    [Header("Cooldown Seconds (after taking candy)")]
    public float minCooldown = 20f;
    public float maxCooldown = 120f;

    public System.Action<bool> OnAvailabilityChanged;

    private bool available = true;
    private Coroutine cooldownCo;

    void Awake()
    {
        available = availableAtStart;
        if (porchLight) porchLight.SetOn(available);
    }

    public bool IsAvailable => available;

    public void SetAvailable(bool on)
    {
        if (available == on) return;
        available = on;
        if (porchLight) porchLight.SetOn(available);
        OnAvailabilityChanged?.Invoke(available);
    }

    /// <summary>Call this when candy is actually awarded (minigame completes with >0 caught).</summary>
    public void ConsumeAndStartCooldown()
    {
        if (!available) return;
        SetAvailable(false);

        if (cooldownCo != null) StopCoroutine(cooldownCo);
        cooldownCo = StartCoroutine(Co_Cooldown());
    }

    IEnumerator Co_Cooldown()
    {
        float wait = Random.Range(minCooldown, maxCooldown);
        yield return new WaitForSeconds(wait);
        SetAvailable(true);
        cooldownCo = null;
    }
}
