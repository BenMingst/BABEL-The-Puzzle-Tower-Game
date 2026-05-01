using System.Collections;
using UnityEngine;

public class ArrowTypeIndicator : MonoBehaviour
{
    public static ArrowTypeIndicator Instance;

    [SerializeField] public SpriteRenderer indicatorRenderer;
    public Sprite fireArrowSprite;
    public Sprite normalArrowSprite;
    public Sprite iceArrowSprite;

    public float displayDuration = 0.3f;
    public float fadeDuration = 0.2f;

    private Coroutine currentCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (indicatorRenderer != null)
            indicatorRenderer.gameObject.SetActive(false);
    }

    public void ShowIndicator(ArrowTypeManager.ArrowType arrowType)
    {
        Sprite sprite = null;
        switch (arrowType)
        {
            case ArrowTypeManager.ArrowType.Normal:
                sprite = normalArrowSprite;
                break;
            case ArrowTypeManager.ArrowType.Ice:
                sprite = iceArrowSprite;
                break;
            case ArrowTypeManager.ArrowType.Fire:
                sprite = fireArrowSprite;
                break;
        }

        if (sprite == null) return;

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(FlashIndicator(sprite));
    }

    IEnumerator FlashIndicator(Sprite sprite)
    {
        indicatorRenderer.sprite = sprite;
        indicatorRenderer.gameObject.SetActive(true);
        indicatorRenderer.color = new Color(1f, 1f, 1f, 1f);

        yield return new WaitForSeconds(displayDuration);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            indicatorRenderer.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        indicatorRenderer.gameObject.SetActive(false);
        currentCoroutine = null;
    }
}