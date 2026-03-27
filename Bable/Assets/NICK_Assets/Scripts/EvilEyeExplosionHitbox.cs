using System.Collections;
using UnityEngine;

public class EvilEyeExplosionHitbox : MonoBehaviour
{
    public int damage = 1;
    public float damageCooldown = 1f;
    public bool isIceExplosion = false;
    public bool isFireExplosion = false;
    private bool canDamage = true;

    void OnEnable()
    {
        canDamage = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage) return;
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
            PlayerController pc = other.GetComponentInParent<PlayerController>();
            if (ph != null)
            {
                ph.TakeDamageNoKnockback(damage);
                if (isIceExplosion && pc != null)
                    pc.ApplyFreezeEffect();
                if (isFireExplosion && pc != null)
                    pc.ApplyBurnEffect();
                StartCoroutine(DamageCooldown());
            }
        }
    }

    IEnumerator DamageCooldown()
    {
        canDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canDamage = true;
    }
}
