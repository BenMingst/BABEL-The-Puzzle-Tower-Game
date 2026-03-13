using UnityEngine;

public class SlashHitbox : MonoBehaviour
{
    public int damage = 1;

 void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log("SlashHitbox hit: " + other.gameObject.name + " tag: " + other.gameObject.tag + " layer: " + other.gameObject.layer);

    if (other.CompareTag("Enemy"))
    {
        other.GetComponentInChildren<EnemyHealth>()?.TakeDamageWithKnockback(damage, transform.position);
    }
    else if (other.CompareTag("Door"))
    {
        other.GetComponent<DoorHealth>()?.TakeHit();
    }
}
}