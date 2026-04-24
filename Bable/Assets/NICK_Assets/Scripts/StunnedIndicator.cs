using System.Collections;
using UnityEngine;

public class StunnedIndicator : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer indicatorRenderer;
    public Animator indicatorAnimator;

    [Header("Settings")]
    public float displayDuration = 1.5f;
    public float fadeDuration = 0.3f;
    public float heightOffset = 1.5f;

    void Awake()
    {
        if (indicatorRenderer == null)
            indicatorRenderer = GetComponent<SpriteRenderer>();
        if (indicatorAnimator == null)
            indicatorAnimator = GetComponent<Animator>();

        if (indicatorRenderer != null)
            indicatorRenderer.enabled = false;
        if (indicatorAnimator != null)
            indicatorAnimator.enabled = false;
    }

    public void Show(bool facingRight)
    {
        StopAllCoroutines();
        StartCoroutine(ShowSequence(facingRight));
    }

    IEnumerator ShowSequence(bool facingRight)
    {
        if (indicatorRenderer == null) yield break;

        transform.localPosition = new Vector3(0f, heightOffset, 0f);

        // flip based on enemy facing
        Vector3 scale = transform.localScale;
        scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;

        indicatorRenderer.enabled = true;
        Color c = indicatorRenderer.color;
        c.a = 1f;
        indicatorRenderer.color = c;

        if (indicatorAnimator != null)
        {
            indicatorAnimator.enabled = true;
            indicatorAnimator.Play(0, 0, 0f);
        }

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
        if (indicatorAnimator != null)
            indicatorAnimator.enabled = false;
    }
}