using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public float attackRange = 3f;
    public Transform player;

    [Header("Attack")]
    public float attackCooldown = 1f;
    public GameObject enemyHitbox;
    public float animationDuration = 1.05f;
    public float hitboxDelay = 0.45f;
    public float hitboxDuration = 0.6f;

    private Animator animator;
    private bool isAttacking = false;
    private bool facingRight = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
{
    if (player == null) return;

    float distanceToPlayer = Vector2.Distance(transform.position, player.position);

    // only flip when not attacking
    if (!isAttacking)
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

    if (distanceToPlayer <= attackRange && !isAttacking)
    {
        StartCoroutine(Attack());
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

        animator.SetTrigger("AttackRight");

        // wait until frame 7 to activate hitbox
        yield return new WaitForSeconds(hitboxDelay);

        // flip hitbox to correct side
        Vector3 hitboxPos = enemyHitbox.transform.localPosition;
        hitboxPos.x = facingRight ? Mathf.Abs(hitboxPos.x) : -Mathf.Abs(hitboxPos.x);
        enemyHitbox.transform.localPosition = hitboxPos;

        // activate hitbox
        enemyHitbox.GetComponent<Collider2D>().enabled = true;

        yield return new WaitForSeconds(hitboxDuration);

        // deactivate hitbox
        enemyHitbox.GetComponent<Collider2D>().enabled = false;

        // wait for cooldown
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }
}