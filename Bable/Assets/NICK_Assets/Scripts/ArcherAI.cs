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
    public Animator archerTopAnimator;

    [Header("Arrow")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;

    [Header("Sight")]
    public LayerMask sightBlockLayers;

    private bool isAttacking = false;
    private bool facingRight = true;
    private EnemyHealth enemyHealth;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        enemyHealth = GetComponent<EnemyHealth>();
    }

    bool CanSeePlayer()
{
    if (Mathf.Abs(transform.position.y - player.position.y) > yTolerance) return false;

    Vector2 origin = new Vector2(transform.position.x, transform.position.y + 1f);
    Vector2 target = new Vector2(player.position.x, player.position.y + 1f);
    Vector2 direction = (target - origin).normalized;
    float distance = Vector2.Distance(origin, target);
    RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, sightBlockLayers);

    return hit.collider == null;
}

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isAttacking)
        {
            facingRight = player.position.x > transform.position.x;
        }

        if (distanceToPlayer <= shootRange && !isAttacking && CanSeePlayer())
        {
            StartCoroutine(ShootSequence());
        }
    }

    IEnumerator ShootSequence()
    {
        isAttacking = true;

        yield return null;

        while (Vector2.Distance(transform.position, player.position) <= shootRange && CanSeePlayer())
        {
            if (enemyHealth.isHurt || enemyHealth.isDead) { yield return null; continue; }

            facingRight = player.position.x > transform.position.x;

            if (facingRight)
                archerTopAnimator.SetTrigger("AimRight");
            else
                archerTopAnimator.SetTrigger("AimLeft");

            // wait for aim + shoot + cooldown
            yield return new WaitForSeconds(1f + 0.8f + attackCooldown);
        }

        isAttacking = false;
    }

    public void SpawnArrow()
    {
        if (enemyHealth.isHurt || enemyHealth.isDead) return;

        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
            Arrow arrowScript = arrow.GetComponent<Arrow>();
            arrowScript.SetDirection(facingRight);

            if (!facingRight)
            {
                Vector3 scale = arrow.transform.localScale;
                scale.x *= -1;
                arrow.transform.localScale = scale;
            }
        }
    }
}