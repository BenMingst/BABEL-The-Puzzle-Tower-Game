using UnityEngine;

public class FloatingPlatform : MonoBehaviour
{
    public enum MovementType { Vertical, Horizontal, Circle }

    [Header("Movement")]
    public MovementType movementType = MovementType.Vertical;
    public float moveDistance = 2f;
    public float moveSpeed = 1f;

    [Header("Direction")]
    public bool startGoingUp = true;
    public bool startGoingRight = true;

    [Header("Rider Detection")]
    public LayerMask riderLayer;
    public float detectionHeight = 0.3f;
    public float detectionWidthMultiplier = 0.9f;

    private Vector3 startPosition;
    private Vector3 lastPosition;
    private float timer = 0f;
    private Rigidbody2D rb;

    void Start()
    {
        startPosition = transform.position;
        lastPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector3(col.bounds.center.x, col.bounds.max.y + detectionHeight / 2f, 0f),
            new Vector3(col.bounds.size.x * detectionWidthMultiplier, detectionHeight, 0f));
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime * moveSpeed;
        Vector3 newPosition = transform.position;

        float verticalDirection = startGoingUp ? 1f : -1f;
        float horizontalDirection = startGoingRight ? 1f : -1f;

        switch (movementType)
        {
            case MovementType.Vertical:
                newPosition = new Vector3(
                    startPosition.x,
                    startPosition.y + Mathf.Sin(timer) * moveDistance * verticalDirection,
                    startPosition.z);
                break;

            case MovementType.Horizontal:
                newPosition = new Vector3(
                    startPosition.x + Mathf.Sin(timer) * moveDistance * horizontalDirection,
                    startPosition.y,
                    startPosition.z);
                break;

            case MovementType.Circle:
                newPosition = new Vector3(
                    startPosition.x + Mathf.Cos(timer) * moveDistance,
                    startPosition.y + Mathf.Sin(timer) * moveDistance,
                    startPosition.z);
                break;
        }

        Vector3 delta = newPosition - lastPosition;
        rb.MovePosition(newPosition);
        lastPosition = newPosition;

        Collider2D platformCollider = GetComponent<Collider2D>();
        if (platformCollider == null) return;

        Vector2 boxCenter = new Vector2(
            platformCollider.bounds.center.x,
            platformCollider.bounds.max.y + detectionHeight / 2f);
        Vector2 boxSize = new Vector2(
            platformCollider.bounds.size.x * detectionWidthMultiplier,
            detectionHeight);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, riderLayer);

        if (Mathf.Abs(delta.x) > 0f)
        {
            foreach (Collider2D hit in hits)
            {
                // player
                PlayerController pc = hit.GetComponent<PlayerController>();
                if (pc == null) pc = hit.GetComponentInParent<PlayerController>();
                if (pc != null)
                {
                    pc.platformVelocity = new Vector2(delta.x / Time.fixedDeltaTime, 0f);
                    continue;
                }

                // skelly
                EnemyAI enemyAI = hit.GetComponent<EnemyAI>();
                if (enemyAI == null) enemyAI = hit.GetComponentInParent<EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.platformVelocity = new Vector2(delta.x / Time.fixedDeltaTime, 0f);
                    continue;
                }

               

                // evil eye
                EvilEye evilEye = hit.GetComponent<EvilEye>();
                if (evilEye == null) evilEye = hit.GetComponentInParent<EvilEye>();
                if (evilEye != null)
                {
                    evilEye.platformVelocity = new Vector2(delta.x / Time.fixedDeltaTime, 0f);
                    continue;
                }

                // fallback
                Rigidbody2D riderRb = hit.GetComponent<Rigidbody2D>();
                if (riderRb == null) riderRb = hit.GetComponentInParent<Rigidbody2D>();
                if (riderRb != null)
                {
                    riderRb.linearVelocity = new Vector2(
                        riderRb.linearVelocity.x + delta.x / Time.fixedDeltaTime,
                        riderRb.linearVelocity.y);
                }
            }
        }
    }
}