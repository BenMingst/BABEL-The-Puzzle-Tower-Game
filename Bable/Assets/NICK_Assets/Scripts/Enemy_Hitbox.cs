using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D other)
    {

        EnemyHealth eh = GetComponentInParent<EnemyHealth>();
    if (eh != null && eh.isDead) return;
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
            health?.TakeDamage(damage, transform.position);
        }
    }
}