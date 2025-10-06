using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PorchLightController : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite lightOnSprite;
    public Sprite lightOffSprite;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetOn(bool on)
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        if (sr) sr.sprite = on ? lightOnSprite : lightOffSprite;
    }
}
