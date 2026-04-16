using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private AudioSource audioSource;
    private EnemyAudio enemyAudio;
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

    [Header("Settings")]
    public bool arrowOnlyDamage = false;

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

        enemyAudio = GetComponent<EnemyAudio>();
    }

    public bool IsInvulnerable()
    {
        if (enemyAnimator == null) return false;
        EvilEye eye = GetComponent<EvilEye>();
        if (eye != null && eye.currentState == EvilEye.EvilEyeState.Defend) return true;
        if (eye != null && eye.currentState == EvilEye.EvilEyeState.FrontIdle) return true;
        return false;
    }

    public bool IsImmuneToArrow(Arrow.ArrowType arrowType)
    {
        EvilEye eye = GetComponent<EvilEye>();
        if (eye == null) return false;

        if (eye.eyeType == EvilEye.EyeType.Ice && arrowType != Arrow.ArrowType.Fire) return true;
        if (eye.eyeType == EvilEye.EyeType.Fire && arrowType != Arrow.ArrowType.Ice) return true;

        return false;
    }

    void CancelAttack()
    {
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.InterruptAttack();
    }

    public void TakeDamage(int damage)
    {
        if (isHurt) return;
        if (IsInvulnerable()) return;

        CancelAttack();
        currentHealth -= damage;

         // play hurt sound
        SoundManager.instance.PlayWorldRandom(enemyAudio.hurtSounds, transform, 1f, 0.1f);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HurtNoKnockback());
        }
    }

    public void TakeDamageWithKnockback(int damage, Vector2 hitPosition)
    {
        if (isHurt) return;
        if (IsInvulnerable()) return;

        CancelAttack();
        currentHealth -= damage;


         // play hurt sound for arrow
        SoundManager.instance.PlayWorldRandom(enemyAudio.hurtSounds, transform, 1f, 0.1f);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            float knockbackDirection = transform.position.x > hitPosition.x ? 1f : -1f;
            StartCoroutine(Knockback(knockbackDirection));
        }
    }

    public void TakeDamageFromArrow(int damage, Vector2 hitPosition)
    {
        if (isHurt) return;
        if (IsInvulnerable()) return;

        CancelAttack();
        currentHealth -= damage;

        // play hurt sound
        SoundManager.instance.PlayWorldRandom(enemyAudio.hurtSounds, transform, 1f, 0.1f);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HurtNoKnockback());
        }
    }

    IEnumerator HurtNoKnockback()
    {
        isHurt = true;
        enemyAnimator.SetBool("IsHurt", true);


         // play hurt sound
        SoundManager.instance.PlayWorldRandom(enemyAudio.hurtSounds, transform, 1f, 0f);

        if (archerBottom != null)
            archerBottom.SetActive(false);

        yield return new WaitForSeconds(0.15f);

        if (isDead) yield break;

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
        SoundManager.instance.PlayWorldClip(enemyAudio.deathSound, transform, 1f, 0f);

        StopAllCoroutines();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        Debug.Log("Disabling " + allColliders.Length + " colliders on death");
        foreach (Collider2D col in allColliders)
        {
            Debug.Log("Disabling: " + col.gameObject.name + " " + col.GetType().Name);
            col.enabled = false;
        }

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
    EvilEye evilEye = GetComponent<EvilEye>();
    if (enemyAI != null) enemyAI.enabled = false;
    if (archerAI != null) archerAI.enabled = false;
    if (evilEye != null) evilEye.enabled = false;

    // mark permanent death enemy
    PermanentDeathEnemy pde = GetComponent<PermanentDeathEnemy>();
    if (pde != null) pde.MarkDead();

    if (archerBottom != null)
        archerBottom.SetActive(false);

    yield return new WaitForSeconds(0.5f);

    if (heartDropPrefab != null && Random.value > 0.25f)
        Instantiate(heartDropPrefab, transform.position, Quaternion.identity);

    Destroy(gameObject);
}
}