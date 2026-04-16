using System.Collections;
using UnityEngine;

public class ArcherAI : MonoBehaviour
{
    [Header("Detection")]
    public float shootRange = 8f;
    public float yTolerance = 2f;
    public Transform player;

    [Header("Attack")]
    public float attackCooldown = 2f;
    public Transform arrowSpawnPoint;
    public GameObject arrowPrefab;

    [Header("Sight")]
    public LayerMask sightBlockLayers;

    [Header("Components")]
    public Animator animator;

    [Header("Direction")]
    public bool facingRight = true;

    public Vector2 platformVelocity = Vector2.zero;

    private bool isShooting = false;
    private EnemyHealth enemyHealth;
    private Rigidbody2D rb;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        enemyHealth = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();

        // apply initial facing direction
        if (!facingRight)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    bool CanSeePlayer()
{
    if (Mathf.Abs(transform.position.y - player.position.y) > yTolerance) return false;

    // use a lower origin point so raycast doesn't shoot over player
    Vector2 origin = new Vector2(transform.position.x, transform.position.y);
    Vector2 target = new Vector2(player.position.x, player.position.y + 0.5f);
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

    void Update()
    {
        if (enemyHealth.isDead) return;
        if (enemyHealth.isHurt) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer();

        // face player when not shooting
        if (!isShooting && canSee)
        {
            if (player.position.x < transform.position.x && facingRight)
                Flip();
            else if (player.position.x > transform.position.x && !facingRight)
                Flip();
        }

        if (distanceToPlayer <= shootRange && !isShooting && canSee)
        {
            StartCoroutine(ShootSequence());
        }
        else if (distanceToPlayer > shootRange || !canSee)
        {
            if (isShooting) return;
            animator.SetBool("IsIdle", true);
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(platformVelocity.x, rb.linearVelocity.y);
            platformVelocity = Vector2.zero;
        }
    }

    public void SpawnArrow()
    {
        if (enemyHealth.isHurt || enemyHealth.isDead) return;
        if (arrowPrefab == null || arrowSpawnPoint == null) return;

        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        SoundManager.instance.PlayWorldClip(PlayerAudio.instance.normalArrowSpawnSound, transform, 1f, 0f);
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null)
            arrowScript.SetDirection(facingRight);

        if (!facingRight)
        {
            Vector3 scale = arrow.transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            arrow.transform.localScale = scale;
        }
    }

    IEnumerator ShootSequence()
{
    isShooting = true;
    animator.SetBool("IsIdle", false);

    // always use ShootRight trigger - scale flip handles visual direction
    animator.SetTrigger("ShootRight");
    yield return new WaitForSeconds(attackCooldown);
    isShooting = false;
    yield break;
}
}