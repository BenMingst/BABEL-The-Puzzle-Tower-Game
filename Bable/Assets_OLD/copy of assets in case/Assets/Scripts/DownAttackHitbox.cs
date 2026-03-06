using UnityEngine;

public class DownAttackHitbox : MonoBehaviour
{
    public int damage = 1;
    public PlayerController playerController;

    void OnTriggerEnter2D(Collider2D other)
    {
            Debug.Log("DownAttackHitbox detected: " + other.gameObject.name);

        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            playerController.DownAttackBounce();
        }
    }
}
