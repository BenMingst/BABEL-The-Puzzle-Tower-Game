using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
            Debug.Log("EnemyHitbox detected: " + other.gameObject.name);

        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage, transform.position);
        }
    }
}
