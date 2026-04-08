using System.Collections;
using UnityEngine;

public class NecromancerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;

    public float knockbackForce = 2f;
    public GameObject heartDropPrefab;

    [Header("Animation")]
    public Animator enemyAnimator;

    [Header("Physics")]
    public Rigidbody2D rb;

    public bool isHurt = false;
    public bool isDead = false;

    private NecromancerAI necroAI;

    void Awake()
    {
        currentHealth = maxHealth;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (enemyAnimator == null)
            enemyAnimator = GetComponent<Animator>();

        necroAI = GetComponent<NecromancerAI>();
    }

    public void TakeDamage(int damage, Vector2 hitPosition)
    {
        if (isHurt) return;
        if (isDead) return;

        if (necroAI != null && necroAI.isStaggered)
            necroAI.NotifyStaggerHit(damage);

        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
        else
            StartCoroutine(HurtSequence(hitPosition));
    }

    IEnumerator HurtSequence(Vector2 hitPosition)
    {
        isHurt = true;
        enemyAnimator.SetBool("IsHurt", true);

        float knockbackDirection = transform.position.x > hitPosition.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(knockbackDirection * knockbackForce, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.15f);
        if (isDead) yield break;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.6f);
        if (isDead) yield break;

        enemyAnimator.SetBool("IsHurt", false);
        isHurt = false;

        if (necroAI != null && necroAI.isStaggered)
            enemyAnimator.SetTrigger("Stagger");
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();

        // kill all spawned enemies with spawn effect
        if (necroAI != null)
        {
            foreach (GameObject enemy in necroAI.spawnedEnemies)
            {
                if (enemy != null)
                {
                    // play spawn effect at enemy position
                    if (necroAI.spawnEffectPrefab != null)
                        Instantiate(necroAI.spawnEffectPrefab, enemy.transform.position, Quaternion.identity);
                    Destroy(enemy);
                }
            }
            necroAI.spawnedEnemies.Clear();
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in allColliders)
            col.enabled = false;

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        enemyAnimator.SetBool("IsHurt", true);
        yield return new WaitForSeconds(0.6f);

        enemyAnimator.SetBool("IsHurt", false);
        yield return null;

        enemyAnimator.enabled = false;
        enemyAnimator.enabled = true;
        enemyAnimator.Play("Death", 0, 0f);

        NecromancerAI ai = GetComponent<NecromancerAI>();
        if (ai != null) ai.enabled = false;

        yield return new WaitForSeconds(0.5f);

        // drop 2 hearts
        if (heartDropPrefab != null)
        {
            Instantiate(heartDropPrefab, transform.position + new Vector3(-0.3f, 0f, 0f), Quaternion.identity);
            Instantiate(heartDropPrefab, transform.position + new Vector3(0.3f, 0f, 0f), Quaternion.identity);
        }

        Destroy(gameObject);
    }
}