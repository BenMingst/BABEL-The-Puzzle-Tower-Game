<<<<<<< HEAD
=======
using System.Collections;
>>>>>>> d84f4e240e1a950fc9fa3dcae56d38f5a56a9654
using UnityEngine;

public class Arrow : MonoBehaviour
{
<<<<<<< HEAD
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
=======
    public float speed = 6f;
    public float maxDistance = 15f;
    public float stickDuration = 3f;
    public int damage = 1;

    private float distanceTravelled = 0f;
    private bool isStuck = false;
    private Vector2 travelDirection;
    private Rigidbody2D rb;
    private Collider2D hurtbox;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hurtbox = GetComponent<Collider2D>();
        rb.linearVelocity = travelDirection * speed;
>>>>>>> d84f4e240e1a950fc9fa3dcae56d38f5a56a9654
    }

    public void SetDirection(bool facingRight)
    {
<<<<<<< HEAD
        direction = facingRight ? Vector2.right : Vector2.left;
=======
        travelDirection = facingRight ? Vector2.right : Vector2.left;
    }

    void Update()
    {
        if (isStuck) return;

        distanceTravelled += speed * Time.deltaTime;

        if (distanceTravelled >= maxDistance)
        {
            Destroy(gameObject);
        }
>>>>>>> d84f4e240e1a950fc9fa3dcae56d38f5a56a9654
    }

    void OnTriggerEnter2D(Collider2D other)
    {
<<<<<<< HEAD
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
=======
        if (isStuck) return;

        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage, transform.position);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StartCoroutine(StickToWall());
        }
    }

    IEnumerator StickToWall()
    {
        isStuck = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        hurtbox.enabled = false;

        yield return new WaitForSeconds(stickDuration);

        Destroy(gameObject);
    }
>>>>>>> d84f4e240e1a950fc9fa3dcae56d38f5a56a9654
}