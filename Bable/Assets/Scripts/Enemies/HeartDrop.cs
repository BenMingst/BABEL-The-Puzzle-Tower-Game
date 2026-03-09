using System.Collections;
using UnityEngine;

public class HeartDrop : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(Random.Range(-2f, 2f), 4f);
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(LifetimeSequence());
    }

    IEnumerator LifetimeSequence()
    {
        // 5 seconds fully visible
        yield return new WaitForSeconds(5f);

        // 3 seconds slow flashing
        float slowFlashTime = 0f;
        while (slowFlashTime < 3f)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.3f);
            slowFlashTime += 0.3f;
        }

        // 2 seconds rapid flashing
        float rapidFlashTime = 0f;
        while (rapidFlashTime < 2f)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
            rapidFlashTime += 0.1f;
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Heart entered");
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsFullHealth())
            {
                playerHealth.HealHalfHeart();
                Destroy(gameObject);
            }
        }
    }
}
