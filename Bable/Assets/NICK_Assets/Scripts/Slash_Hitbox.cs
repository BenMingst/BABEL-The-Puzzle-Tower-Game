using UnityEngine;

public class SlashHitbox : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
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

            // check armored skelly
            ArmoredSkellyHealth armoredHealth = other.GetComponentInParent<ArmoredSkellyHealth>();
            if (armoredHealth != null)
            {
                ArmoredSkellyAI ai = armoredHealth.GetComponent<ArmoredSkellyAI>();
                if (ai != null && ai.isArmored)
                {
                    ai.TakeSlashKnockback();
                    return;
                }
                else
                {
                    armoredHealth.TakeDamageWithKnockback(damage, transform.position);
                    return;
                }
            }

            // check serpent
            SerpentHealth serpentHealth = other.GetComponentInParent<SerpentHealth>();
            if (serpentHealth != null)
            {
                serpentHealth.TakeDamage(damage);
                return;
            }

            // normal enemy
            other.GetComponentInParent<EnemyHealth>()?.TakeDamageWithKnockback(damage, transform.position);
        }
        else if (other.CompareTag("Door"))
        {
            other.GetComponent<DoorHealth>()?.TakeHit();
        }
        else if (other.CompareTag("Target"))
        {
            other.GetComponent<Target>()?.TakeHit();
        }
    }
}