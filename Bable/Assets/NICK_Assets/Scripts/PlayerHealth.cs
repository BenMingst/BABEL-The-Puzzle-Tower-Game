using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHearts = 3;
    private int currentHearts;

    [SerializeField] private AudioClip[] hurtSounds;
    [SerializeField] private AudioClip[] deathSounds;

    private AudioSource audioSource;

    [Header("Heart UI")]
    public Animator[] heartAnimators;

    [Header("Player Animation")]
    public Animator playerAnimator;
    public Rigidbody2D playerRb;
    public PlayerController playerController;

    public float knockbackForce = 3f;
    public float hurtDuration = 0.6f;

    private bool isInvincible = false;
    public float invincibilityDuration = 1f;

    void Start()
    {
        currentHearts = maxHearts * 2;
    }

    public void TakeDamage(int damage, Vector2 enemyPosition)
    {
        if (isInvincible) return;
        if (playerController.isRolling) return;
        if (playerController.isDead) return;

        int heartIndex = (currentHearts - 1) / 2;
        bool isFullToHalf = currentHearts % 2 == 0;

        if (isFullToHalf)
            heartAnimators[heartIndex].SetTrigger("HalfBreak");
        else
            heartAnimators[heartIndex].SetTrigger("FullBreak");

        playerAnimator.SetTrigger("Hurt");
        StartCoroutine(HurtSequence(enemyPosition));

        currentHearts -= damage;

        
        // play hurt sound
        SoundFXManager.instance.PlayRandomSoundFXClip(hurtSounds, transform, 1f, 0.1f);

        if (currentHearts <= 0)
            Die();
        else
            StartCoroutine(InvincibilityFrames());
    }

    public void TakeDamageNoKnockback(int damage)
    {
        if (isInvincible) return;
        if (playerController.isRolling) return;
        if (playerController.isDead) return;

        int heartIndex = (currentHearts - 1) / 2;
        bool isFullToHalf = currentHearts % 2 == 0;

        if (isFullToHalf)
            heartAnimators[heartIndex].SetTrigger("HalfBreak");
        else
            heartAnimators[heartIndex].SetTrigger("FullBreak");

        playerAnimator.SetTrigger("Hurt");
        StartCoroutine(HurtSequenceNoKnockback());

        currentHearts -= damage;

        if (currentHearts <= 0)
            Die();
        else
            StartCoroutine(InvincibilityFrames());
    }

    public bool IsFullHealth()
    {
        return currentHearts >= maxHearts * 2;
    }

    public void HealHalfHeart()
    {
        currentHearts += 1;

        int heartIndex = currentHearts / 2 - (currentHearts % 2 == 0 ? 1 : 0);
        bool isHalfToFull = currentHearts % 2 == 0;

        if (isHalfToFull)
            heartAnimators[heartIndex].SetTrigger("FullHeal");
        else
            heartAnimators[heartIndex].SetTrigger("HalfHeal");
    }

    IEnumerator HurtSequence(Vector2 enemyPosition)
    {
        playerController.isHurt = true;

        float knockbackDirection = transform.position.x > enemyPosition.x ? 1f : -1f;
        playerRb.linearVelocity = new Vector2(knockbackDirection * knockbackForce, playerRb.linearVelocity.y);

        yield return new WaitForSeconds(0.15f);

        playerRb.linearVelocity = new Vector2(0, playerRb.linearVelocity.y);

        yield return new WaitForSeconds(0.45f);

        playerController.isHurt = false;
    }

    IEnumerator HurtSequenceNoKnockback()
    {
        playerController.isHurt = true;
        
        playerRb.linearVelocity = new Vector2(0, playerRb.linearVelocity.y);

        yield return new WaitForSeconds(0.6f);

        playerController.isHurt = false;
    }

    IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    void Die()
    {
        playerController.OnDeath();
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Game Over");
    }
}