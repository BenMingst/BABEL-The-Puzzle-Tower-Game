using System.Collections;
using UnityEngine;

public class SpiderAI : MonoBehaviour
{
    private SpiderAudio audioData;
    public enum SpiderState { AtNest, Dropping, Attacking, Returning, Hurt, GrappledStun }

    [Header("References")]
    public Transform nest;
    public BoxCollider2D pounceArea;
    public Animator animator;
    public LineRenderer webRenderer;
    public Rigidbody2D rb;
    public Collider2D gasHitbox;

    [Header("Settings")]
    public float dropSpeed = 8f;
    public float returnSpeed = 6f;
    public float attackDuration = 1.2f;
    public float cooldown = 3f;
    public float hurtDuration = 0.3f;
    public float groundDetectOffset = 0.1f;
    public LayerMask groundLayer;

    [Header("Nest Offset")]
    public float restingYOffset = 0.5f;

    [Header("Grapple Stun")]
    public float grappleStunDelay = 2f;

    [Header("Grapple Slow")]
    public float grappleSlowMultiplier = 1f;

    [Header("Web Visual")]
    public float webEndYOffset = 0.3f;

    public SpiderState currentState = SpiderState.AtNest;

    private Transform player;
    private EnemyHealth enemyHealth;
    private GrappleCatchable grappleCatchable;
    private float cooldownTimer = 0f;
    private bool playerInPounceArea = false;
    private bool wasGrappled = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        enemyHealth = GetComponent<EnemyHealth>();
        grappleCatchable = GetComponent<GrappleCatchable>();
        audioData = GetComponent<SpiderAudio>();
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (webRenderer == null) webRenderer = GetComponent<LineRenderer>();

        transform.position = RestingPosition();

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;

        if (gasHitbox != null) gasHitbox.gameObject.SetActive(false);

        if (webRenderer != null)
            webRenderer.positionCount = 2;
    }

    Vector3 RestingPosition()
    {
        return nest.position + Vector3.down * restingYOffset;
    }

    void OnEnable()
    {
        if (grappleCatchable != null && wasGrappled)
        {
            wasGrappled = false;
            StartCoroutine(GrappleRecoverySequence());
        }
    }

    void OnDisable()
    {
        if (grappleCatchable != null && grappleCatchable.IsCaught())
        {
            wasGrappled = true;
            if (gasHitbox != null) gasHitbox.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (webRenderer != null)
        {
            Vector3 spiderLinePoint = transform.position + new Vector3(0f, webEndYOffset, 0f);
            webRenderer.SetPosition(0, nest.position);
            webRenderer.SetPosition(1, spiderLinePoint);
        }

        if (enemyHealth != null && enemyHealth.isDead)
        {
            StopAllCoroutines();
            return;
        }

        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime * grappleSlowMultiplier;

        playerInPounceArea = IsPlayerInPounceArea();

        if (currentState == SpiderState.AtNest && playerInPounceArea && cooldownTimer <= 0)
        {
            StartCoroutine(AttackSequence());
        }
    }

    bool IsPlayerInPounceArea()
    {
        if (pounceArea == null || player == null) return false;
        return pounceArea.OverlapPoint(player.position);
    }

    IEnumerator GrappleRecoverySequence()
    {
        currentState = SpiderState.GrappledStun;
        animator.SetTrigger("Idle");

        yield return new WaitForSeconds(grappleStunDelay / grappleSlowMultiplier);

        currentState = SpiderState.Returning;
        animator.SetTrigger("Webbing");

        Vector3 restingPos = RestingPosition();
        while (Vector3.Distance(transform.position, restingPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, restingPos, returnSpeed * grappleSlowMultiplier * Time.deltaTime);
            yield return null;
        }

        transform.position = restingPos;
        animator.SetTrigger("Idle");
        currentState = SpiderState.AtNest;
        cooldownTimer = cooldown;
    }

    IEnumerator AttackSequence()
    {
        currentState = SpiderState.Dropping;
        animator.SetTrigger("Webbing");

        // play drop sound
        SoundManager.instance.PlayWorldClip(audioData.webDropSound, transform, 1f);

        yield return new WaitForSeconds(GetAnimLength("Webbing") / grappleSlowMultiplier);

        float dropTargetY = FindGroundYBelow();

        while (transform.position.y > dropTargetY)
        {
            transform.position += Vector3.down * dropSpeed * grappleSlowMultiplier * Time.deltaTime;
            yield return null;
        }

        currentState = SpiderState.Attacking;
        animator.SetTrigger("Attack");
        SoundManager.instance.PlayWorldRandom(audioData.attackSounds, transform, 1f);
        if (gasHitbox != null) gasHitbox.gameObject.SetActive(true);

        yield return new WaitForSeconds(attackDuration / grappleSlowMultiplier);

        if (gasHitbox != null) gasHitbox.gameObject.SetActive(false);

        currentState = SpiderState.Returning;
        animator.SetTrigger("Webbing");

        Vector3 restingPos = RestingPosition();
        while (Vector3.Distance(transform.position, restingPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, restingPos, returnSpeed * grappleSlowMultiplier * Time.deltaTime);
            yield return null;
        }

        transform.position = restingPos;
        animator.SetTrigger("Idle");

        currentState = SpiderState.AtNest;
        cooldownTimer = cooldown;
    }

    float FindGroundYBelow()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100f, groundLayer);
        if (hit.collider != null)
        {
            return hit.point.y + groundDetectOffset + GetComponent<Collider2D>().bounds.extents.y;
        }
        return transform.position.y - 10f;
    }

    public void OnHurtByPlayer(Vector2 knockbackForce)
    {
        if (enemyHealth != null && enemyHealth.isDead) return;

        StopAllCoroutines();
        if (gasHitbox != null) gasHitbox.gameObject.SetActive(false);

        StartCoroutine(HurtSequence());
    }

    IEnumerator HurtSequence()
    {
        currentState = SpiderState.Hurt;
        animator.SetBool("IsHurt", true);

        // play hurt sound
        SoundManager.instance.PlayWorldRandom(audioData.hurtSounds, transform, 1f);

        yield return new WaitForSeconds(hurtDuration);

        animator.SetBool("IsHurt", false);
        currentState = SpiderState.Returning;
        animator.SetTrigger("Webbing");

        Vector3 restingPos = RestingPosition();
        while (Vector3.Distance(transform.position, restingPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, restingPos, returnSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = restingPos;
        animator.SetTrigger("Idle");
        currentState = SpiderState.AtNest;
        cooldownTimer = cooldown;
    }

    float GetAnimLength(string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 0.5f;
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == stateName) return clip.length;
        }
        return 0.5f;
    }

    void OnDrawGizmosSelected()
    {
        if (nest != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, nest.position);
            Gizmos.DrawWireSphere(nest.position, 0.3f);
        }
    }
}