using System.Collections;
using UnityEngine;

public class BombExplosionHitbox : MonoBehaviour
{
    public int damage = 2;
    private bool onCooldown = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // check destructible block
        DestructibleBlock block = other.GetComponentInParent<DestructibleBlock>();
        if (block != null)
        {
            block.Destroy();
            return;
        }

        // check necromancer
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
        if (ph != null)
        {
            if (!onCooldown)
            {
                ph.TakeDamageNoKnockback(damage);
                StartCoroutine(DamageCooldown());
            }
            return;
        }

        // damage enemies
        EnemyHealth eh = other.GetComponentInParent<EnemyHealth>();
        if (eh != null) eh.TakeDamage(damage);

        // damage armored skelly
        ArmoredSkellyHealth ash = other.GetComponentInParent<ArmoredSkellyHealth>();
        if (ash != null) ash.TakeBombDamage(damage);
    }

    IEnumerator DamageCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(1f);
        onCooldown = false;
    }

    void OnDisable()
    {
        onCooldown = false;
        StopAllCoroutines();
    }
}