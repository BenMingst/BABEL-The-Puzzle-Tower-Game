using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float attackRange = 1.5f;
    public float sightRange = 5f;
    public Transform player;

    [Header("Movement")]
    public float walkSpeed = 2f;

    [Header("Jumping")]
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    protected bool isGrounded = false;
    private bool hitWall = false;
    private float jumpCooldown = 0.3f;
    private float lastJumpTime = -999f;

    [Header("Attack")]
    public float attackCooldown = 1f;
    public GameObject enemyHitbox;
    public float animationDuration = 1.05f;
    public float hitboxDelay = 0.45f;
    public float hitboxDuration = 0.1f;

    protected Animator animator;
    protected bool isAttacking = false;
    protected bool facingRight = true;
    protected EnemyHealth enemyHealth;
    protected Rigidbody2D rb;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;
        enemyHealth = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        if (player == null) return;

        CheckGrounded();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isAttacking && !enemyHealth.isHurt)
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

        if (distanceToPlayer <= attackRange && !isAttacking && !enemyHealth.isHurt)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("IsWalking", false);
            StartCoroutine(Attack());
        }
        else if (distanceToPlayer <= sightRange && !isAttacking && !enemyHealth.isHurt)
        {
            float direction = player.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
            animator.SetBool("IsWalking", true);

            NavigateObstacles();
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("IsWalking", false);
        }
    }

    protected void CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, groundLayer);
        isGrounded = hit.collider != null;
    }

    protected void NavigateObstacles()
    {
        if (!isGrounded || !hitWall) return;
        if (Time.time - lastJumpTime < jumpCooldown) return;

        Jump();
    }

    protected void Jump()
    {
        if (!isGrounded) return;
        if (Time.time - lastJumpTime < jumpCooldown) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        lastJumpTime = Time.time;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;
        if (collision.gameObject.CompareTag("Enemy")) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // If the contact normal is mostly horizontal, it's a wall
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                hitWall = true;
                return;
            }
        }
    }

    void LateUpdate()
    {
        hitWall = false;
    }

    protected virtual void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    protected virtual IEnumerator Attack()
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