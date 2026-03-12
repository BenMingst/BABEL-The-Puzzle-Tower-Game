using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifetime = 3f;

    private Vector2 direction;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void SetDirection(bool facingRight)
    {
        direction = facingRight ? Vector2.right : Vector2.left;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // hook into your PlayerHealth here, e.g.:
            // other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }

        // destroy on hitting terrain
        if (other.CompareTag("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}