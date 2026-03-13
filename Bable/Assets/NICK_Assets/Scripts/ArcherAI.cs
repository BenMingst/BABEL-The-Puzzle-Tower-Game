using System.Collections;
using UnityEngine;

public class ArcherAI : MonoBehaviour
{
    [Header("Detection")]
    public float shootRange = 8f;
    public Transform player;

    [Header("Attack")]
    public float attackCooldown = 2f;
    public Animator archerTopAnimator;

    [Header("Arrow")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;

    private bool isAttacking = false;
    private bool facingRight = true;
    private EnemyHealth enemyHealth;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        enemyHealth = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isAttacking)
        {
            facingRight = player.position.x > transform.position.x;
        }

        if (distanceToPlayer <= shootRange && !isAttacking)
        {
            StartCoroutine(ShootSequence());
        }
    }

    IEnumerator ShootSequence()
    {
        isAttacking = true;

        yield return null;

        while (Vector2.Distance(transform.position, player.position) <= shootRange)
        {
            facingRight = player.position.x > transform.position.x;

            if (facingRight)
                archerTopAnimator.SetTrigger("AimRight");
            else
                archerTopAnimator.SetTrigger("AimLeft");

            float aimTimer = 0f;
            bool directionChanged = false;
            while (aimTimer < 1f)
            {
                aimTimer += Time.deltaTime;
                bool newFacingRight = player.position.x > transform.position.x;
                if (newFacingRight != facingRight)
                {
                    directionChanged = true;
                    break;
                }
                yield return null;
            }

            if (directionChanged) continue;

            if (facingRight)
                archerTopAnimator.SetTrigger("ShootRight");
            else
                archerTopAnimator.SetTrigger("ShootLeft");

            // wait for shoot animation to finish
            yield return new WaitForSeconds(0.8f);

            yield return new WaitForSeconds(attackCooldown);
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