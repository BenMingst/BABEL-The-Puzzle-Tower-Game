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
    // set max hearts based on scene
    string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    int buildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

    if (sceneName == "Level_3")
        maxHearts = 5;
    else if (sceneName == "new_level2")
        maxHearts = 4;
    else
        maxHearts = 3;

    currentHearts = maxHearts * 2;

    // activate correct heart UI slots
    for (int i = 0; i < heartAnimators.Length; i++)
    {
        if (heartAnimators[i] != null)
            heartAnimators[i].gameObject.SetActive(i < maxHearts);
    }
}

    void UpdateHeartUI(int previousHearts, int damage)
    {
        if (damage > 1)
        {
            // jump directly to correct state for each heart
            for (int i = 0; i < heartAnimators.Length; i++)
            {
                int ticksForSlot = Mathf.Clamp(currentHearts - (i * 2), 0, 2);

                if (ticksForSlot >= 2)
                    heartAnimators[i].Play("Full", 0, 0f);
                else if (ticksForSlot == 1)
                    heartAnimators[i].Play("half_heart_UI", 0, 0f);
                else
                    heartAnimators[i].Play("empty_heart_UI", 0, 0f);
            }
        }
        else
        {
            // normal single tick animation
            int heartIndex = (previousHearts - 1) / 2;
            bool isFullToHalf = previousHearts % 2 == 0;

            if (isFullToHalf)
                heartAnimators[heartIndex].SetTrigger("HalfBreak");
            else
                heartAnimators[heartIndex].SetTrigger("FullBreak");
        }
        // update stats
        StatManager.Instance.damageTaken += damage;
    }

    public void TakeDamage(int damage, Vector2 enemyPosition)
    {
        if (isInvincible) return;
        if (playerController.isRolling) return;
        if (playerController.isDead) return;
            Debug.Log("TakeDamage called - damage: " + damage + " from position: " + enemyPosition + " caller: " + new System.Diagnostics.StackTrace().ToString());


        int previousHearts = currentHearts;
        currentHearts -= damage;
        currentHearts = Mathf.Max(0, currentHearts);

        UpdateHeartUI(previousHearts, damage);

        playerAnimator.SetTrigger("Hurt");
        StartCoroutine(HurtSequence(enemyPosition));

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
            Debug.Log("TakeDamageNoKnockback called - damage: " + damage + " caller: " + new System.Diagnostics.StackTrace().ToString());


        int previousHearts = currentHearts;
        currentHearts -= damage;
        currentHearts = Mathf.Max(0, currentHearts);

        UpdateHeartUI(previousHearts, damage);

        playerAnimator.SetTrigger("Hurt");
        StartCoroutine(HurtSequenceNoKnockback());

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
        int previousHearts = currentHearts;
        currentHearts = Mathf.Min(maxHearts * 2, currentHearts + 1);

        int heartIndex = (currentHearts - 1) / 2;
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
        if (StatManager.Instance != null)
            StatManager.Instance.deaths++;
        playerController.OnDeath();
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Game Over");
    }
}