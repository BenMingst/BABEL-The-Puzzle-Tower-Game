using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public enum ArrowType { Normal, Ice, Fire }
    public ArrowType arrowType = ArrowType.Normal;

    public float speed = 1.5f;
    public float maxDistance = 15f;
    public float stickDuration = 1f;
    public int damage = 1;
    public float spawnIgnoreTime = 0.1f;
    public bool isPlayerArrow = false;
    public float hitStopDuration = 0f;
    public float bounceDestroyDelay = 2f;

    private float distanceTravelled = 0f;
    private bool isStuck = false;
    private bool ignoreGround = true;
    private Vector2 travelDirection;
    private Rigidbody2D rb;
    private Collider2D[] allColliders;
    private Animator animator;
    
    // sounds for non player arrows are played on start, player arrow sounds are played in player script when shooting
    [SerializeField]
    public AudioClip normalShotSound;
    [SerializeField]
    public AudioClip iceShotSound;
    [SerializeField]
    public AudioClip fireShotSound;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        allColliders = GetComponents<Collider2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // play shoot sound if not player arrow
        if (!isPlayerArrow)
        {
            switch (arrowType)
            {
                case ArrowType.Normal:
                    SoundManager.instance.PlayWorldClip(normalShotSound, transform, 1f);
                    break;
                case ArrowType.Ice:
                    SoundManager.instance.PlayWorldClip(iceShotSound, transform, 1f);
                    break;
                case ArrowType.Fire:
                    SoundManager.instance.PlayWorldClip(fireShotSound, transform, 1f);
                    break;
            }
        }
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

    void DisableAllColliders()
    {
        foreach (Collider2D col in allColliders)
            col.enabled = false;
    }

    void BounceOff()
    {
        rb.linearVelocity = new Vector2(-rb.linearVelocity.x * 0.3f, 2f);
        rb.constraints = RigidbodyConstraints2D.None;
        rb.angularVelocity = Random.Range(-300f, 300f);
        rb.gravityScale = 1f;
        DisableAllColliders();

        // play bounce off sound
        if (SoundManager.instance != null)
            SoundManager.instance.PlayWorldClip(PlayerAudio.instance.arrow.bounceOffSound, transform, 1f);
        StartCoroutine(BounceDestroy());
    }

    void ShowInvulnIndicator(GameObject enemyObj)
    {
        InvulnerableIndicator indicator = enemyObj.GetComponentInChildren<InvulnerableIndicator>();
        if (indicator != null)
        {
            bool enemyFacingRight = enemyObj.transform.localScale.x > 0;
            indicator.Show(enemyFacingRight);
        }
    }

    void Update()
    {
        if (isStuck) return;

        distanceTravelled += speed * Time.deltaTime;

        if (distanceTravelled >= maxDistance)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isStuck) return;
        if (ignoreGround) return;
        if (isPlayerArrow)
        {
            if (other.CompareTag("Player")) return;

            if (other.CompareTag("Target"))
            {
                other.GetComponent<Target>()?.TakeHit();
                return;
            }

            IceWall iceWall = other.GetComponent<IceWall>();
            if (iceWall == null) iceWall = other.GetComponentInParent<IceWall>();
            if (iceWall != null && arrowType == ArrowType.Fire)
            {
                iceWall.HitByFireArrow();
                StartCoroutine(HitAndDestroy());
                return;
            }

            // check necromancer
            NecromancerHealth necroHealth = other.GetComponentInParent<NecromancerHealth>();
            if (necroHealth != null)
            {
                NecromancerAI necroAI = necroHealth.GetComponent<NecromancerAI>();
                if (necroAI != null && !necroAI.IsVulnerable())
                {
                    ShowInvulnIndicator(necroHealth.gameObject);
                    BounceOff();
                    return;
                }
                necroHealth.TakeDamage(damage, transform.position);
                StartCoroutine(HitAndDestroy());
                return;
            }

            // check serpent
            SerpentHealth serpentHealth = other.GetComponentInParent<SerpentHealth>();
            if (serpentHealth != null)
            {
                serpentHealth.TakeDamage(damage);
                StartCoroutine(HitAndDestroy());
                return;
            }

            // check armored skelly
            ArmoredSkellyHealth armoredHealth = other.GetComponentInParent<ArmoredSkellyHealth>();
            if (armoredHealth != null)
            {
                ArmoredSkellyAI ai = armoredHealth.GetComponent<ArmoredSkellyAI>();
                if (ai != null && ai.isArmored)
                {
                    ShowInvulnIndicator(armoredHealth.gameObject);
                    BounceOff();
                    return;
                }
                else
                {
                    armoredHealth.TakeDamageWithKnockback(damage, transform.position);
                    StartCoroutine(HitAndDestroy());
                    return;
                }
            }

            EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                if (enemyHealth.IsInvulnerable())
                {
                    ShowInvulnIndicator(enemyHealth.gameObject);
                    BounceOff();
                    return;
                }

                if (enemyHealth.IsImmuneToArrow(arrowType))
                {
                    ShowInvulnIndicator(enemyHealth.gameObject);
                    BounceOff();
                    return;
                }

                if (arrowType == ArrowType.Normal)
                    enemyHealth.TakeDamageWithKnockback(damage, transform.position);
                else
                    enemyHealth.TakeDamage(damage);
                StartCoroutine(HitAndDestroy());
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                DisableAllColliders();
                StartCoroutine(StickToWall());
            }
        }
        else
        {
            if (other.CompareTag("Player"))
            {
                if (!isStuck)
                    other.GetComponentInParent<PlayerHealth>()?.TakeDamage(damage, transform.position);
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                DisableAllColliders();
                StartCoroutine(StickToWall());
            }
        }
    }

    IEnumerator BounceDestroy()
    {
        isStuck = true;
        yield return new WaitForSeconds(bounceDestroyDelay);
        Destroy(gameObject);
        // play bounce sound
        SoundManager.instance.PlayWorldClip(SoundManager.instance.arrowBounceSound, transform, 1f);
    }

    IEnumerator HitAndDestroy()
    {
        isStuck = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        DisableAllColliders();

        yield return new WaitForSeconds(hitStopDuration);

        Destroy(gameObject);
    }

    IEnumerator StickToWall()
    {
        isStuck = true;
        // play stick sound
        SoundManager.instance.PlayWorldClip(SoundManager.instance.arrowStuckSound, transform, 1f);
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        DisableAllColliders();

        yield return new WaitForSeconds(stickDuration);

        Destroy(gameObject);
    }
}