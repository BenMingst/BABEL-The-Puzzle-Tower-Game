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

    private bool isAttacking = false;
    private bool facingRight = true;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // update facing direction when not attacking
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
        // update facing direction each shot
        facingRight = player.position.x > transform.position.x;

        if (facingRight)
        {
            archerTopAnimator.SetTrigger("AimRight");
        }
        else
        {
            archerTopAnimator.SetTrigger("AimLeft");
        }

        // wait for aim animation - 1000ms
        yield return new WaitForSeconds(1f);

        if (facingRight)
        {
            archerTopAnimator.SetTrigger("ShootRight");
        }
        else
        {
                Debug.Log("Firing ShootLeft trigger");

            archerTopAnimator.SetTrigger("ShootLeft");
        }

        // wait for shoot animation - 800ms
        yield return new WaitForSeconds(0.8f);

        // cooldown before next shot
        yield return new WaitForSeconds(attackCooldown);
    }

    isAttacking = false;
}
}
