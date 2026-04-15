using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        // check normal enemy health
        EnemyHealth eh = GetComponentInParent<EnemyHealth>();
        if (eh != null && eh.isDead) return;

        // check armored skelly health
        ArmoredSkellyHealth ash = GetComponentInParent<ArmoredSkellyHealth>();
        if (ash != null && ash.isDead) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
            health?.TakeDamage(damage, transform.position);
        }
    }
}