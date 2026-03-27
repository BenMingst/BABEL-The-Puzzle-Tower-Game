using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public enum ArrowType { Normal, Ice, Fire }
    public ArrowType arrowType = ArrowType.Normal;

    public float speed = 1.5f;
    public float maxDistance = 15f;
    public float stickDuration = 3f;
    public int damage = 1;
    public float spawnIgnoreTime = 0.1f;
    public bool isPlayerArrow = false;
    public float hitStopDuration = 0f;
    public float bounceDestroyDelay = 2f;

    private float distanceTravelled = 0f;
    private bool isStuck = false;
    private bool ignoreGround = true;
    private Vector2 travelDirection;
    private Rigidbody2D rb;
    private Collider2D[] allColliders;
    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        allColliders = GetComponents<Collider2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        rb.linearVelocity = travelDirection * speed;
        StartCoroutine(EnableGroundCollision());
    }

    IEnumerator EnableGroundCollision()
    {
        yield return new WaitForSeconds(spawnIgnoreTime);
        ignoreGround = false;
    }

    public void SetDirection(bool facingRight)
    {
        travelDirection = facingRight ? Vector2.right : Vector2.left;
    }

    void DisableAllColliders()
    {
        foreach (Collider2D col in allColliders)
            col.enabled = false;
    }

    void Update()
    {
        if (isStuck) return;

        distanceTravelled += speed * Time.deltaTime;

        if (distanceTravelled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isStuck) return;
        if (ignoreGround) return;

        if (isPlayerArrow)
        {
            if (other.CompareTag("Player")) return;

            IceWall iceWall = other.GetComponent<IceWall>();
            if (iceWall == null) iceWall = other.GetComponentInParent<IceWall>();
            if (iceWall != null && arrowType == ArrowType.Fire)
            {
                iceWall.HitByFireArrow();
                StartCoroutine(HitAndDestroy());
                return;
            }

            EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                if (enemyHealth.IsInvulnerable())
                {
                    rb.linearVelocity = new Vector2(-rb.linearVelocity.x * 0.3f, 2f);
                    rb.constraints = RigidbodyConstraints2D.None;
                    rb.angularVelocity = Random.Range(-300f, 300f);
                    rb.gravityScale = 1f;
                    DisableAllColliders();
                    StartCoroutine(BounceDestroy());
                    return;
                }

                if (enemyHealth.IsImmuneToArrow(arrowType))
                {
                    rb.linearVelocity = new Vector2(-rb.linearVelocity.x * 0.3f, 2f);
                    rb.constraints = RigidbodyConstraints2D.None;
                    rb.angularVelocity = Random.Range(-300f, 300f);
                    rb.gravityScale = 1f;
                    DisableAllColliders();
                    StartCoroutine(BounceDestroy());
                    return;
                }

                if (arrowType == ArrowType.Normal)
                    enemyHealth.TakeDamageWithKnockback(damage, transform.position);
                else
                    enemyHealth.TakeDamage(damage);
                StartCoroutine(HitAndDestroy());
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                DisableAllColliders();
                StartCoroutine(StickToWall());
            }
        }
        else
        {
            if (other.CompareTag("Player"))
            {
                if (!isStuck)
                    other.GetComponentInParent<PlayerHealth>()?.TakeDamage(damage, transform.position);
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                DisableAllColliders();
                StartCoroutine(StickToWall());
            }
        }
    }

    IEnumerator BounceDestroy()
    {
        isStuck = true;
        yield return new WaitForSeconds(bounceDestroyDelay);
        Destroy(gameObject);
    }

    IEnumerator HitAndDestroy()
    {
        isStuck = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        DisableAllColliders();

        yield return new WaitForSeconds(hitStopDuration);

        Destroy(gameObject);
    }

    IEnumerator StickToWall()
    {
        isStuck = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        DisableAllColliders();

        yield return new WaitForSeconds(stickDuration);

        Destroy(gameObject);
    }
}