using UnityEngine;

public class DownAttackHitbox : MonoBehaviour
{
    public int damage = 1;
    public PlayerController playerController;

 void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Enemy"))
    {
        other.GetComponentInChildren<EnemyHealth>()?.TakeDamageWithKnockback(damage, transform.position);
        playerController.DownAttackBounce();
    }
}
}
