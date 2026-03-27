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

    private Animator animator;
    private bool isShooting = false;
    private bool facingRight = true;
    private EnemyHealth enemyHealth;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;
        enemyHealth = GetComponent<EnemyHealth>();
        rb = GetComponentInParent<Rigidbody2D>();
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

    void Update()
    {
        if (enemyHealth.isDead) return;
        if (enemyHealth.isHurt) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer();

        if (canSee)
        {
            if (player.position.x < transform.position.x && facingRight)
                Flip();
            else if (player.position.x > transform.position.x && !facingRight)
                Flip();
        }

        if (rb != null)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y);

        if (distanceToPlayer <= shootRange && !isShooting && canSee)
            StartCoroutine(ShootSequence());
    }

    public void SpawnArrow()
    {
        if (enemyHealth.isHurt || enemyHealth.isDead) return;
        if (arrowPrefab == null || arrowSpawnPoint == null) return;

        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
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
            Debug.Log("ShootSequence STARTED");

        isShooting = true;

        if (facingRight)
            animator.SetTrigger("ShootRight");
        else
            animator.SetTrigger("ShootLeft");

        yield return new WaitForSeconds(attackCooldown);

        isShooting = false;
    }
}