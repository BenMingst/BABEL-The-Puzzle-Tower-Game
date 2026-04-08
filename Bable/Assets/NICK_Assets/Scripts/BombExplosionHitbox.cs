using UnityEngine;

public class BombExplosionHitbox : MonoBehaviour
{
    public int damage = 2;

    void OnTriggerEnter2D(Collider2D other)
    {
        // check necromancer first
        NecromancerHealth necroHealth = other.GetComponentInParent<NecromancerHealth>();
        if (necroHealth != null)
        {
            NecromancerAI necroAI = necroHealth.GetComponent<NecromancerAI>();
            if (necroAI != null && !necroAI.IsVulnerable()) return;
            necroHealth.TakeDamage(damage, transform.position);
            return;
        }

        // damage player
        PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
        if (ph != null) ph.TakeDamageNoKnockback(damage);

        // damage enemies
        EnemyHealth eh = other.GetComponentInParent<EnemyHealth>();
        if (eh != null) eh.TakeDamage(damage);

        // damage armored skelly
        ArmoredSkellyHealth ash = other.GetComponentInParent<ArmoredSkellyHealth>();
        if (ash != null) ash.TakeBombDamage(damage);
    }
}