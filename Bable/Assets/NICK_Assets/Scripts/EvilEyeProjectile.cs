using System.Collections;
using UnityEngine;

public class EvilEyeProjectile : MonoBehaviour
{
    public float speed = 5f;
    public float arcHeight = 2f;
    public int damage = 1;

    [Header("Explosion")]
    public Animator explosionAnimator;
    public Collider2D explosionCollider;
    public float explosionDuration = 0.5f;
    [SerializeField] public AudioClip normalExplosionSound;

    [Header("Fire")]
    public bool isFireProjectile = false;
    public GameObject groundFirePrefab;
    public float fireSpreadRadius = 2f;
    [SerializeField] public AudioClip fireExplosionSound;

    [Header("Ice")]
    public bool isIceProjectile = false;
    public GameObject groundIcePrefab;
    public float iceSpreadRadius = 2f;
    [SerializeField] public AudioClip iceExplosionSound;

    [Header("Detection")]
    public LayerMask groundLayer;

    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float journeyLength;
    private float distanceTravelled = 0f;
    private bool hasExploded = false;
    private Rigidbody2D rb;
    private Collider2D projectileCollider;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Launch(Vector2 target)
    {
        startPosition = transform.position;
        targetPosition = target;
        journeyLength = Vector2.Distance(startPosition, targetPosition);

        if (explosionCollider != null)
            explosionCollider.enabled = false;
    }

    void Update()
    {
        if (hasExploded) return;

        distanceTravelled += speed * Time.deltaTime;
        float fractionOfJourney = distanceTravelled / journeyLength;

        if (fractionOfJourney >= 1f)
        {
            transform.position = targetPosition;
            StartCoroutine(Explode());
            return;
        }

        Vector2 currentPos = Vector2.Lerp(startPosition, targetPosition, fractionOfJourney);
        currentPos.y += arcHeight * Mathf.Sin(fractionOfJourney * Mathf.PI);
        transform.position = currentPos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StartCoroutine(Explode());
        }
        else if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
            PlayerController pc = other.GetComponentInParent<PlayerController>();
            if (ph != null) ph.TakeDamageNoKnockback(damage);
            if (pc != null && isFireProjectile) pc.ApplyBurnEffect();
            if (pc != null && isIceProjectile) pc.ApplyFreezeEffect();
            StartCoroutine(Explode());
        }
    }

    IEnumerator Explode()
    {
        hasExploded = true;
        // play explosion sound
        if (SoundManager.instance != null && !isFireProjectile && !isIceProjectile)
        {
            SoundManager.instance.PlayWorldClip(normalExplosionSound, transform, 1f);
        }
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        if (projectileCollider != null)
            projectileCollider.enabled = false;

        if (explosionAnimator != null)
            explosionAnimator.SetTrigger("Explode");

        if (explosionCollider != null)
            explosionCollider.enabled = true;

        if (isFireProjectile && groundFirePrefab != null)
        {
            // play fire explosion sound
            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlayWorldClip(fireExplosionSound, transform, 1f);
            }
            SpawnGroundFire();
        }

        if (isIceProjectile && groundIcePrefab != null)
        {
            // play ice explosion sound
            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlayWorldClip(iceExplosionSound, transform, .6f);
            }
            SpawnGroundIce();
        }
        yield return null;

        if (explosionCollider != null)
            explosionCollider.enabled = false;

        yield return new WaitForSeconds(explosionDuration);

        Destroy(gameObject);
    }

    void SpawnGroundFire()
    {
        Vector2 center = transform.position;
        int checks = 5;
        for (int i = 0; i < checks; i++)
        {
            float xOffset = Mathf.Lerp(-fireSpreadRadius, fireSpreadRadius, i / (float)(checks - 1));
            Vector2 checkPos = new Vector2(center.x + xOffset, center.y);
            RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, 3f, groundLayer);
            if (hit.collider != null)
            {
                Vector3 firePos = new Vector3(hit.point.x, hit.point.y, 0f);
                Collider2D existing = Physics2D.OverlapPoint(firePos);
                if (existing == null || existing.GetComponent<GroundFire>() == null)
                    Instantiate(groundFirePrefab, firePos, Quaternion.identity);
            }
        }
    }

    void SpawnGroundIce()
    {
        Vector2 center = transform.position;
        int checks = 5;
        for (int i = 0; i < checks; i++)
        {
            float xOffset = Mathf.Lerp(-iceSpreadRadius, iceSpreadRadius, i / (float)(checks - 1));
            Vector2 checkPos = new Vector2(center.x + xOffset, center.y);
            RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, 3f, groundLayer);
            if (hit.collider != null)
            {
                Vector3 icePos = new Vector3(hit.point.x, hit.point.y, 0f);
                Collider2D existing = Physics2D.OverlapPoint(icePos);
                if (existing == null || existing.GetComponent<GroundIce>() == null)
                    Instantiate(groundIcePrefab, icePos, Quaternion.identity);
            }
        }
    }
}