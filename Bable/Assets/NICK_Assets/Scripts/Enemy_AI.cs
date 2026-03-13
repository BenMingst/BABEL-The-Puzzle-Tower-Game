using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float attackRange = 1.5f;
    public float sightRange = 5f;
    public float yTolerance = 2f;
    public Transform player;

    [Header("Movement")]
    public float walkSpeed = 2f;

    [Header("Attack")]
    public float attackCooldown = 1f;
    public GameObject enemyHitbox;
    public float animationDuration = 1.05f;
    public float hitboxDelay = 0.45f;
    public float hitboxDuration = 0.1f;

    [Header("Ledge Detection")]
    public Transform ledgeCheck;
    public float ledgeCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Sight")]
    public LayerMask sightBlockLayers;

    private Animator animator;
    private bool isAttacking = false;
    private bool facingRight = true;
    private EnemyHealth enemyHealth;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;
        enemyHealth = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();
    }

bool CanSeePlayer()
{
    if (Mathf.Abs(transform.position.y - player.position.y) > yTolerance) return false;

    Vector2 origin = new Vector2(transform.position.x, transform.position.y + 1f);
    Vector2 target = new Vector2(player.position.x, player.position.y + 1f);
    Vector2 direction = (target - origin).normalized;
    float distance = Vector2.Distance(origin, target);
    RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, sightBlockLayers);

    return hit.collider == null;
}

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer();

        if (!isAttacking && !enemyHealth.isHurt && canSee)
        {
            if (player.position.x < transform.position.x && facingRight)
            {
                Flip();
            }
            else if (player.position.x > transform.position.x && !facingRight)
            {
                Flip();
            }
        }

        // check if there is ground ahead
        bool groundAhead = ledgeCheck == null || Physics2D.OverlapCircle(ledgeCheck.position, ledgeCheckRadius, groundLayer);

        if (distanceToPlayer <= attackRange && !isAttacking && !enemyHealth.isHurt && canSee)
        {
            // stop walking and attack
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("IsWalking", false);
            StartCoroutine(Attack());
        }
        else if (distanceToPlayer <= sightRange && !isAttacking && !enemyHealth.isHurt && groundAhead && canSee)
        {
            // walk towards player only if ground ahead and can see player
            float direction = player.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
            animator.SetBool("IsWalking", true);
        }
        else
        {
            // idle
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("IsWalking", false);
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        yield return null;

        if (enemyHealth.isHurt) { isAttacking = false; yield break; }

        animator.SetTrigger("AttackRight");

        yield return new WaitForSeconds(hitboxDelay);

        if (enemyHealth.isHurt)
        {
            enemyHitbox.GetComponent<Collider2D>().enabled = false;
            isAttacking = false;
            yield break;
        }

        Vector3 hitboxPos = enemyHitbox.transform.localPosition;
        hitboxPos.x = facingRight ? Mathf.Abs(hitboxPos.x) : -Mathf.Abs(hitboxPos.x);
        enemyHitbox.transform.localPosition = hitboxPos;

        enemyHitbox.GetComponent<Collider2D>().enabled = true;

        yield return new WaitForSeconds(hitboxDuration);

        enemyHitbox.GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }
}