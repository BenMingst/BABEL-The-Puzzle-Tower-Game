using System.Collections;
using UnityEngine;

public class BombExplosionHitbox : MonoBehaviour
{
    public int damage = 2;

    void OnTriggerEnter2D(Collider2D other)
    {
        // damage player
        PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
        if (ph != null) ph.TakeDamageNoKnockback(damage);

        // damage enemies
        EnemyHealth eh = other.GetComponentInParent<EnemyHealth>();
        if (eh != null) eh.TakeDamage(damage);
    }
}