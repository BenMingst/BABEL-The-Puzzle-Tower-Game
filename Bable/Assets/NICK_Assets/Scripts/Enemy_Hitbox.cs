using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("EnemyHitbox detected: " + other.gameObject.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected, attempting TakeDamage");
            PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
            Debug.Log("PlayerHealth found: " + (health != null));
            health?.TakeDamage(damage, transform.position);
        }
    }
}