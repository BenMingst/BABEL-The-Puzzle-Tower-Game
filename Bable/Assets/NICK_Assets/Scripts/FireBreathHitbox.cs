using System.Collections;
using UnityEngine;

public class FireBreathHitbox : MonoBehaviour
{
    public int damage = 2;
    public float damageCooldown = 1f;
    private bool onCooldown = false;

    void OnTriggerStay2D(Collider2D other)
    {
        if (onCooldown) return;
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
            PlayerController pc = other.GetComponentInParent<PlayerController>();
            if (ph != null)
            {
                ph.TakeDamage(damage, transform.position);
                if (pc != null) pc.ApplyBurnEffect();
                StartCoroutine(DamageCooldown());
            }
        }
    }

    IEnumerator DamageCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(damageCooldown);
        onCooldown = false;
    }

    void OnDisable()
    {
        onCooldown = false;
        StopAllCoroutines();
    }
}