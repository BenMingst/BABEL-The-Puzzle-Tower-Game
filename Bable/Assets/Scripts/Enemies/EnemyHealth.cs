using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public float knockbackForce = 3f;
    public GameObject enemyHitbox;
    public GameObject heartDropPrefab;
    private Rigidbody2D rb;
    private Animator animator;
    public bool isHurt = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamageWithKnockback(int damage, Vector2 hitPosition)
    {
        if (isHurt) return;

        TakeDamage(damage);

        float knockbackDirection = transform.position.x > hitPosition.x ? 1f : -1f;
        StartCoroutine(Knockback(knockbackDirection));
    }

    IEnumerator Knockback(float direction)
    {
        isHurt = true;
        animator.SetBool("IsHurt", true);

        rb.linearVelocity = new Vector2(direction * knockbackForce, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.15f);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.6f);

        animator.SetBool("IsHurt", false);
        isHurt = false;
    }

    void Die()
    {
        StartCoroutine(DeathSequence());
    }

IEnumerator DeathSequence()
{
    animator.SetTrigger("Death");
    GetComponent<EnemyAI>().enabled = false;
    if (enemyHitbox != null)
        enemyHitbox.GetComponent<Collider2D>().enabled = false;

    yield return new WaitForSeconds(0.5f);

    if (heartDropPrefab != null && Random.value > 0.5f)
    {
        Instantiate(heartDropPrefab, transform.position, Quaternion.identity);
    }

    Destroy(gameObject);
}
}