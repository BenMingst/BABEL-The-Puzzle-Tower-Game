using System.Collections;
using UnityEngine;

public class GroundIce : MonoBehaviour
{
    public float duration = 3f;
    public int damage = 1;
    public float iceDamageCooldown = 3f;
    private bool canDamage = true;

    void Start()
    {
        StartCoroutine(Melt());
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
                if (pc != null) pc.ApplyFreezeEffect();
                StartCoroutine(DamageCooldown());
            }
        }
    }

    IEnumerator DamageCooldown()
    {
        canDamage = false;
        yield return new WaitForSeconds(iceDamageCooldown);
        canDamage = true;
    }

    IEnumerator Melt()
    {
        yield return new WaitForSeconds(duration);
        GetComponent<Animator>()?.SetBool("IceEnd", true);
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
