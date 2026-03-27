using System.Collections;
using UnityEngine;

public class GroundFire : MonoBehaviour
{
    public float duration = 3f;
    public int damage = 1;
    public float fireDamageCooldown = 1f;
    private bool canDamage = true;

    void Start()
    {
        StartCoroutine(Burn());
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!canDamage) return;
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
            PlayerController pc = other.GetComponentInParent<PlayerController>();
            if (ph != null)
            {
                ph.TakeDamageNoKnockback(damage);
                if (pc != null) pc.ApplyBurnEffect();
                StartCoroutine(DamageCooldown());
            }
        }
    }

    IEnumerator DamageCooldown()
    {
        canDamage = false;
        yield return new WaitForSeconds(fireDamageCooldown);
        canDamage = true;
    }

    IEnumerator Burn()
    {
        yield return new WaitForSeconds(duration);
        GetComponent<Animator>()?.SetBool("FireEnd", true);
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}