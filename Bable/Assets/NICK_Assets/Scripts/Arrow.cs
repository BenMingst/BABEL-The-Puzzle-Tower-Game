using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 1.5f;
    public float maxDistance = 15f;
    public float stickDuration = 3f;
    public int damage = 1;
    public float spawnIgnoreTime = 0.1f;

    private float distanceTravelled = 0f;
    private bool isStuck = false;
    private bool ignoreGround = true;
    private Vector2 travelDirection;
    private Rigidbody2D rb;
    private Collider2D hurtbox;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hurtbox = GetComponent<Collider2D>();
        rb.linearVelocity = travelDirection * speed;
        StartCoroutine(EnableGroundCollision());
    }

    IEnumerator EnableGroundCollision()
    {
        yield return new WaitForSeconds(spawnIgnoreTime);
        ignoreGround = false;
    }

    public void SetDirection(bool facingRight)
    {
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
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isStuck) return;

        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<PlayerHealth>()?.TakeDamage(damage, transform.position);
        }
        else if (!ignoreGround && other.gameObject.layer == LayerMask.NameToLayer("Ground"))
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
}