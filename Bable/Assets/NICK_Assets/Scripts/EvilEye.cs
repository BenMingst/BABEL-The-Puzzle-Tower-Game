using System.Collections;
using UnityEngine;

public class EvilEye : MonoBehaviour
{
    public enum EvilEyeState { FrontIdle, SideIdle, Attack, Defend, Hurt }
    public enum EyeType { Normal, Ice, Fire }

    public EvilEyeState currentState = EvilEyeState.FrontIdle;
    public EyeType eyeType = EyeType.Normal;

    [Header("Detection")]
    public float sightRange = 5f;
    public float attackRange = 3f;
    public float yTolerance = 2f;
    public LayerMask sightBlockLayers;

    [Header("Attack")]
    public float attackCooldown = 1.5f;
    public GameObject enemyHitbox;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public GameObject iceProjectilePrefab;
    public GameObject fireProjectilePrefab;
    public Transform projectileSpawnPoint;

    [Header("Components")]
    public Animator eyeAnimator;
    public EnemyHealth enemyHealth;

    public Vector2 platformVelocity = Vector2.zero;

    private Transform player;
    private bool facingRight = true;
    private bool isAttacking = false;
    private bool isDefending = false;
    private bool playerInDefendRange = false;
    private Rigidbody2D rb;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        enemyHealth = GetComponent<EnemyHealth>();
        eyeAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    bool CanSeePlayer()
    {
        if (Mathf.Abs(transform.position.y - player.position.y) > yTolerance) return false;

        Vector2 origin = transform.position;
        Vector2 target = player.position;
        Vector2 direction = (target - origin).normalized;
        float distance = Vector2.Distance(origin, target);
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, sightBlockLayers);

        return hit.collider == null;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void FacePlayer()
    {
        if (player.position.x < transform.position.x && facingRight)
            Flip();
        else if (player.position.x > transform.position.x && !facingRight)
            Flip();
    }

    void Update()
    {
        if (enemyHealth.isDead) return;
        if (enemyHealth.isHurt) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer();

        if (rb != null)
            rb.linearVelocity = new Vector2(platformVelocity.x, rb.linearVelocity.y);

        platformVelocity = Vector2.zero;

        if (playerInDefendRange)
        {
            if (!isDefending)
            {
                isDefending = true;
                StartCoroutine(EnterDefend());
            }
            return;
        }

        if (isDefending || isAttacking) return;

        if (distanceToPlayer <= sightRange && canSee)
        {
            FacePlayer();

            if (distanceToPlayer <= attackRange)
            {
                currentState = EvilEyeState.Attack;
                StartCoroutine(AttackSequence());
            }
            else
            {
                currentState = EvilEyeState.SideIdle;
                eyeAnimator.SetBool("SideIdle", true);
                eyeAnimator.SetBool("FrontIdle", false);
                eyeAnimator.SetBool("DefendIdle", false);
            }
        }
        else
        {
            currentState = EvilEyeState.FrontIdle;
            eyeAnimator.SetBool("FrontIdle", true);
            eyeAnimator.SetBool("SideIdle", false);
            eyeAnimator.SetBool("DefendIdle", false);
        }
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;

        eyeAnimator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    public void SpawnProjectile()
    {
        SpawnProjectileOfType(projectilePrefab);
    }

    public void SpawnIceProjectile()
    {
        SpawnProjectileOfType(iceProjectilePrefab);
    }

    public void SpawnFireProjectile()
    {
        SpawnProjectileOfType(fireProjectilePrefab);
    }

    void SpawnProjectileOfType(GameObject prefab)
    {
        if (enemyHealth.isDead) return;
        if (enemyHealth.isHurt) return;
        if (prefab == null) return;

        GameObject proj = Instantiate(prefab, projectileSpawnPoint.position, Quaternion.identity);
        EvilEyeProjectile projectile = proj.GetComponent<EvilEyeProjectile>();
        projectile.Launch(player.position);
    }

    IEnumerator EnterDefend()
    {
        isAttacking = false;
        currentState = EvilEyeState.Defend;

        eyeAnimator.SetBool("SideIdle", false);
        eyeAnimator.SetBool("FrontIdle", false);
        eyeAnimator.SetTrigger("CloseEye");

        yield return new WaitForSeconds(0.3f);

        eyeAnimator.SetBool("DefendIdle", true);

        while (playerInDefendRange)
        {
            yield return null;
        }

        eyeAnimator.SetBool("DefendIdle", false);
        eyeAnimator.SetTrigger("OpenEye");

        yield return new WaitForSeconds(0.3f);

        isDefending = false;
        currentState = EvilEyeState.SideIdle;
        eyeAnimator.SetBool("SideIdle", true);
    }

    public void PlayerEnteredDefendRange()
    {
        playerInDefendRange = true;
    }

    public void PlayerExitedDefendRange()
    {
        playerInDefendRange = false;
    }
}