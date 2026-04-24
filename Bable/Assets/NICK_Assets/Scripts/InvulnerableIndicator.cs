using System.Collections;
using UnityEngine;

public class InvulnerableIndicator : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer indicatorRenderer;
    public Sprite shieldSprite;

    [Header("Settings")]
    public float displayDuration = 0.5f;
    public float fadeDuration = 0.3f;
    public float heightOffset = 1.5f;

    private Coroutine activeRoutine;

    void Awake()
    {
        if (indicatorRenderer == null)
            indicatorRenderer = GetComponent<SpriteRenderer>();

        if (indicatorRenderer != null)
        {
            if (shieldSprite != null)
                indicatorRenderer.sprite = shieldSprite;
            indicatorRenderer.enabled = false;
        }
    }

    public void Show(bool facingRight)
    {
        if (activeRoutine != null) return;
        activeRoutine = StartCoroutine(ShowSequence(facingRight));
    }

    IEnumerator ShowSequence(bool facingRight)
    {
        if (indicatorRenderer == null) { activeRoutine = null; yield break; }

        transform.localPosition = new Vector3(0f, heightOffset, 0f);

        Vector3 scale = transform.localScale;
        scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;

        indicatorRenderer.enabled = true;
        Color c = indicatorRenderer.color;
        c.a = 1f;
        indicatorRenderer.color = c;

        yield return new WaitForSeconds(displayDuration);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            c.a = alpha;
            indicatorRenderer.color = c;
            yield return null;
        }

        indicatorRenderer.enabled = false;
        activeRoutine = null;
    }
}