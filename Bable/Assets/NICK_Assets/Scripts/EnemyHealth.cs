using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{

    [SerializeField] private AudioClip[] hurtSounds;
    [SerializeField] private AudioClip[] deathSounds;

    private AudioSource audioSource;
     

    public int maxHealth = 3;
    private int currentHealth;

    public float knockbackForce = 3f;
    public GameObject enemyHitbox;
    public GameObject heartDropPrefab;

    [Header("Animation")]
    public Animator enemyAnimator;
    public GameObject archerBottom;

    [Header("Physics")]
    public Rigidbody2D rb;

    public bool isHurt = false;
    public bool isDead = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    void Awake()
    {
        currentHealth = maxHealth;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = GetComponentInParent<Rigidbody2D>();

        if (enemyAnimator == null)
            enemyAnimator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // play hurt sound
        SoundFXManager.instance.PlayRandomSoundFXClip(hurtSounds, transform, 1f);

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
        enemyAnimator.SetBool("IsHurt", true);

        if (archerBottom != null)
            archerBottom.SetActive(false);

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

            if (archerBottom != null)
                archerBottom.SetActive(true);
        }
    }

    void Die()
    {
        isDead = true;

        // play death sound
        SoundFXManager.instance.PlayRandomSoundFXClip(deathSounds, transform, 1f);

        StopAllCoroutines();
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

        EnemyAI enemyAI = GetComponent<EnemyAI>();
        ArcherAI archerAI = GetComponent<ArcherAI>();
        if (enemyAI != null) enemyAI.enabled = false;
        if (archerAI != null) archerAI.enabled = false;

        if (enemyHitbox != null)
            enemyHitbox.GetComponent<Collider2D>().enabled = false;

        if (archerBottom != null)
            archerBottom.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        if (heartDropPrefab != null && Random.value > 0.25f)
        {
            Instantiate(heartDropPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}