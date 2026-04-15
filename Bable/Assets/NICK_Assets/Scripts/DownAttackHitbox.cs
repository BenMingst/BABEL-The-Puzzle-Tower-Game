using UnityEngine;

public class DownAttackHitbox : MonoBehaviour
{
    public int damage = 1;
    public PlayerController playerController;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // check necromancer first
            NecromancerHealth necroHealth = other.GetComponentInParent<NecromancerHealth>();
            if (necroHealth != null)
            {
                NecromancerAI necroAI = necroHealth.GetComponent<NecromancerAI>();
                if (necroAI != null && !necroAI.IsVulnerable())
                {
                    playerController.DownAttackBounce();
                    return;
                }
                necroHealth.TakeDamage(damage, transform.position);
                playerController.DownAttackBounce();
                return;
            }

            // check armored skelly
            ArmoredSkellyHealth armoredHealth = other.GetComponentInParent<ArmoredSkellyHealth>();
            if (armoredHealth != null)
            {
                ArmoredSkellyAI ai = armoredHealth.GetComponent<ArmoredSkellyAI>();
                if (ai != null && ai.isArmored)
                {
                    playerController.DownAttackBounce();
                    return;
                }
                else
                {
                    armoredHealth.TakeDamageWithKnockback(damage, transform.position);
                    playerController.DownAttackBounce();
                    return;
                }
            }

            // check serpent
            SerpentHealth serpentHealth = other.GetComponentInParent<SerpentHealth>();
            if (serpentHealth != null)
            {
                serpentHealth.TakeDamage(damage);
                playerController.DownAttackBounce();
                return;
            }

            // normal enemy
            EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamageWithKnockback(damage, transform.position);
                playerController.DownAttackBounce();
            }
        }
    }
}