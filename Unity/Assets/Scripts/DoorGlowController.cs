using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DoorGlowController : MonoBehaviour
{
    public Color litColor = new Color(1f, 0.83f, 0.42f); // #FFD46B
    public Color unlitColor = new Color(0.26f, 0.20f, 0.12f); // #42321E
    public float fadeTime = 0.4f;

    SpriteRenderer sr;
    Coroutine fadeRoutine;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    public void SetLit(bool on)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeToColor(on ? litColor : unlitColor));
    }

    IEnumerator FadeToColor(Color target)
    {
        Color start = sr.color;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            sr.color = Color.Lerp(start, target, t / fadeTime);
            yield return null;
        }
        sr.color = target;
    }
}
