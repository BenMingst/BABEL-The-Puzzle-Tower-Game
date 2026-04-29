using System.Collections;
using UnityEngine;

public class ArmoredSkellyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public float knockbackForce = 3f;
    public GameObject enemyHitbox;
    public GameObject heartDropPrefab;

    [Header("Animation")]
    public Animator enemyAnimator;

    [Header("Physics")]
    public Rigidbody2D rb;

    public bool isHurt = false;
    public bool isDead = false;

    private ArmoredSkellyAudio audioData;

    void Awake()
    {
        currentHealth = maxHealth;
        audioData = GetComponent<ArmoredSkellyAudio>();
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (enemyAnimator == null)
            enemyAnimator = GetComponent<Animator>();
    }

    public void TakeSwordHit(Vector2 hitPosition)
    {
        ArmoredSkellyAI ai = GetComponent<ArmoredSkellyAI>();
        if (ai != null && ai.isArmored)
        {
            ai.TakeSlashKnockback();
            return;
        }

        TakeDamageWithKnockback(1, hitPosition);
    }

    public void TakeBombDamage(int damage)
    {
        if (isHurt) return;

        ArmoredSkellyAI ai = GetComponent<ArmoredSkellyAI>();

        if (ai != null && ai.isArmored)
        {
            ai.DestroyArmor();
            currentHealth -= damage;
            if (currentHealth <= 0)
                Die();
            else
                StartCoroutine(HurtNoKnockback());
            return;
        }

        TakeDamageWithKnockback(damage, transform.position);
    }

    public void TakeDamageWithKnockback(int damage, Vector2 hitPosition)
    {
        if (isHurt) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
        else
        {
            float knockbackDirection = transform.position.x > hitPosition.x ? 1f : -1f;
            StartCoroutine(Knockback(knockbackDirection));
        }
    }

    public void TakeDamage(int damage)
    {
        if (isHurt) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
        else
            StartCoroutine(HurtNoKnockback());
    }

    IEnumerator HurtNoKnockback()
    {
        isHurt = true;
        // play hurt sound
        if (SoundManager.instance != null)
            SoundManager.instance.PlayWorldRandom(audioData.hurtSounds, transform, 1f);
        enemyAnimator.SetBool("IsHurt", true);

        yield return new WaitForSeconds(0.15f);
        if (isDead) yield break;

        yield return new WaitForSeconds(0.6f);
        if (isDead) yield break;

        if (currentHealth > 0)
        {
            enemyAnimator.SetBool("IsHurt", false);
            isHurt = false;
        }
    }

    IEnumerator Knockback(float direction)
    {
        isHurt = true;
        enemyAnimator.SetBool("IsHurt", true);
        // play hurt sound
        if (SoundManager.instance != null)
            SoundManager.instance.PlayWorldRandom(audioData.hurtSounds, transform, 1f);
        rb.linearVelocity = new Vector2(direction * knockbackForce, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.15f);
        if (isDead) yield break;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.6f);
        if (isDead) yield break;

        if (currentHealth > 0)
        {
            enemyAnimator.SetBool("IsHurt", false);
            isHurt = false;
        }
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
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
        enemyAnimator.SetBool("IsWalking", false);
        yield return null;

        enemyAnimator.enabled = false;
        enemyAnimator.enabled = true;
        enemyAnimator.Play("Death", 0, 0f);

        ArmoredSkellyAI ai = GetComponent<ArmoredSkellyAI>();
        if (ai != null) ai.enabled = false;

        if (enemyHitbox != null)
            enemyHitbox.GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(0.5f);

        if (heartDropPrefab != null && Random.value > 0.25f)
            Instantiate(heartDropPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}