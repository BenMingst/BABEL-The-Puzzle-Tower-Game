using System.Collections;
using UnityEngine;

public class SerpentHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;

    public GameObject heartDropPrefab;

    [Header("Animation")]
    public Animator enemyAnimator;

    [Header("Hurt Effect")]
    public GameObject serpentHurtEffect;
    public SpriteRenderer mainSpriteRenderer;

    [Header("Physics")]
    public Rigidbody2D rb;

    public bool isHurt = false;
    public bool isDead = false;

    private SerpentAI serpentAI;
    private SerpentAudio audioData;
    void Awake()
    {
        currentHealth = maxHealth;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (enemyAnimator == null)
            enemyAnimator = GetComponent<Animator>();

        serpentAI = GetComponent<SerpentAI>();
        audioData = GetComponent<SerpentAudio>();
        if (serpentHurtEffect != null)
            serpentHurtEffect.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        if (isHurt) return;
        if (isDead) return;

        serpentAI.SaveStateBeforeHurt();

        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
        else
            StartCoroutine(HurtSequence());
    }

    public void KillInstantly()
    {
        if (isDead) return;
        currentHealth = 0;
        StartCoroutine(HurtThenDie());
    }

    IEnumerator HurtThenDie()
    {
        isHurt = true;

        if (mainSpriteRenderer != null) mainSpriteRenderer.enabled = false;
        if (serpentHurtEffect != null) serpentHurtEffect.SetActive(true);

        yield return new WaitForSeconds(0.25f);

        if (serpentHurtEffect != null) serpentHurtEffect.SetActive(false);
        if (mainSpriteRenderer != null) mainSpriteRenderer.enabled = true;

        Die();
    }

    IEnumerator HurtSequence()
    {
        isHurt = true;

        // hide main sprite show hurt effect
        if (mainSpriteRenderer != null) mainSpriteRenderer.enabled = false;
        if (serpentHurtEffect != null) serpentHurtEffect.SetActive(true);
        // play hurt sound
        if (SoundManager.instance)
        {
            SoundManager.instance.PlayWorldRandom(audioData.hurtSounds, transform, 1f);
        }
        if (rb != null)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.25f);

        if (isDead) yield break;

        // hide hurt effect show main sprite
        if (serpentHurtEffect != null) serpentHurtEffect.SetActive(false);
        if (mainSpriteRenderer != null) mainSpriteRenderer.enabled = true;

        isHurt = false;

        serpentAI.RestoreStateAfterHurt();
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();

        // play death sound
        if (SoundManager.instance)
        {
            SoundManager.instance.PlayWorldClip(audioData.deathSound, transform, 1f);
        }

        if (serpentHurtEffect != null) serpentHurtEffect.SetActive(false);
        if (mainSpriteRenderer != null) mainSpriteRenderer.enabled = true;

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
    if (mainSpriteRenderer != null) mainSpriteRenderer.enabled = true;
    enemyAnimator.enabled = false;
    enemyAnimator.enabled = true;
    enemyAnimator.Play("Death", 0, 0f);

    SerpentAI ai = GetComponent<SerpentAI>();
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