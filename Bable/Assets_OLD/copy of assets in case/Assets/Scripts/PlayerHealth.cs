using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHearts = 3;
    private int currentHearts;

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
            Debug.Log("TakeDamage called by: " + new System.Diagnostics.StackTrace());

        if (isInvincible) return;
        if (playerController.isRolling) return;  // invincible during roll

        int heartIndex = (currentHearts - 1) / 2;
        bool isFullToHalf = currentHearts % 2 == 0;

        if (isFullToHalf)
        {
            heartAnimators[heartIndex].SetTrigger("HalfBreak");
        }
        else
        {
            heartAnimators[heartIndex].SetTrigger("FullBreak");
        }

        playerAnimator.SetTrigger("Hurt");
        StartCoroutine(HurtSequence(enemyPosition));

        currentHearts -= damage;

        if (currentHearts <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
        }
    }

    IEnumerator HurtSequence(Vector2 enemyPosition)
    {
        // Lock player movement
        playerController.isHurt = true;

        // Knock player back away from enemy
        float knockbackDirection = transform.position.x > enemyPosition.x ? 1f : -1f;
        playerRb.linearVelocity = new Vector2(knockbackDirection * knockbackForce, playerRb.linearVelocity.y);

        yield return new WaitForSeconds(0.15f);

        // Stop the knockback
        playerRb.linearVelocity = new Vector2(0, playerRb.linearVelocity.y);

        yield return new WaitForSeconds(0.45f);

        // Unlock player movement
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
        Debug.Log("Player died");
    }
}
