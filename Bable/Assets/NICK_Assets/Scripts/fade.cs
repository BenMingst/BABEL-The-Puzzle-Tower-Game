using UnityEngine;

/// <summary>
/// Fades a sprite by distance to the player: invisible when close, fully visible when farther.
/// Class name matches file name (fade.cs) for Unity.
/// </summary>
public class fade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [Tooltip("Leave empty to use SpriteRenderer on this object or children.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Distance fade")]
    [Tooltip("At this distance (or closer), alpha is 0 (invisible).")]
    [SerializeField, Range(0.05f, 15f)]
    private float fullyInvisibleAtDistance = 1f;

    [Tooltip("At this distance (or farther), alpha is 1 (fully visible).")]
    [SerializeField, Range(0.1f, 50f)]
    private float fullyVisibleAtDistance = 5f;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnValidate()
    {
        if (fullyVisibleAtDistance <= fullyInvisibleAtDistance)
            fullyVisibleAtDistance = fullyInvisibleAtDistance + 0.01f;
    }

    private void Update()
    {
        if (player == null || spriteRenderer == null)
            return;

        float dist = Vector2.Distance((Vector2)player.position, (Vector2)transform.position);
        float alpha = Mathf.InverseLerp(fullyInvisibleAtDistance, fullyVisibleAtDistance, dist);
        alpha = Mathf.Clamp01(alpha);

        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}
