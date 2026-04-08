using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float fuseTime = 3f;
    public int damage = 2;
    public float explosionRadius = 2f;
    public GameObject explosionChild;
    public float explosionDuration = 0.3f;
    public LayerMask damageableLayers;

    public Rigidbody2D rb;
    private bool hasExploded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (explosionChild != null)
            explosionChild.SetActive(false);
    }

    void Start()
    {
        StartCoroutine(FuseSequence());
    }

    public void Launch(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            rb.linearDamping = 3f;
            rb.angularDamping = 3f;
        }
    }

    IEnumerator FuseSequence()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Debug.DrawRay(transform.position, Vector3.up * explosionRadius, Color.red, 2f);
        Debug.DrawRay(transform.position, Vector3.down * explosionRadius, Color.red, 2f);
        Debug.DrawRay(transform.position, Vector3.left * explosionRadius, Color.red, 2f);
        Debug.DrawRay(transform.position, Vector3.right * explosionRadius, Color.red, 2f);

        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null) pc.OnBombExploded();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Necro")) continue;

            // check necromancer
            NecromancerHealth necroHealth = hit.GetComponentInParent<NecromancerHealth>();
            if (necroHealth != null)
            {
                NecromancerAI necroAI = necroHealth.GetComponent<NecromancerAI>();
                if (necroAI != null && necroAI.IsVulnerable())
                    necroHealth.TakeDamage(damage, transform.position);
                continue;
            }

            // check armored skelly
            ArmoredSkellyHealth armoredHealth = hit.GetComponentInParent<ArmoredSkellyHealth>();
            if (armoredHealth != null)
            {
                armoredHealth.TakeBombDamage(damage);
                continue;
            }

            // damage player
            PlayerHealth ph = hit.GetComponentInParent<PlayerHealth>();
            if (ph != null) ph.TakeDamageNoKnockback(damage);

            // damage enemies
            EnemyHealth eh = hit.GetComponentInParent<EnemyHealth>();
            if (eh != null) eh.TakeDamage(damage);
        }

        if (explosionChild != null)
            StartCoroutine(ExplosionSequence());
        else
            Destroy(gameObject);
    }

    IEnumerator ExplosionSequence()
    {
        explosionChild.transform.SetParent(null);
        explosionChild.transform.rotation = Quaternion.identity;
        explosionChild.SetActive(true);

        yield return new WaitForSeconds(explosionDuration);

        explosionChild.SetActive(false);
        Destroy(explosionChild);
        Destroy(gameObject);
    }
}