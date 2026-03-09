using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damageAmount = 1;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(damageAmount, transform.position);
        }
    }
}