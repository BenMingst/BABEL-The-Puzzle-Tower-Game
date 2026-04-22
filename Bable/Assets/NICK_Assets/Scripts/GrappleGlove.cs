using System.Collections;
using UnityEngine;

public class GrappleGlove : MonoBehaviour
{
    public static GrappleGlove Instance;

    [Header("Grapple")]
    public GameObject grappleHeadPrefab;
    public Transform grappleSpawnPoint;
    public Transform grappleAirSpawnPoint;
    public Transform grapplePulledSpawnPoint;
    public Transform grapplePulledAirSpawnPoint;

    [Header("Pull Settings")]
    public float pullSpeed = 15f;
    public float stopDistance = 0.5f;

    [Header("Line Renderer")]
    public LineRenderer lineRenderer;

    public bool isGrappling = false;
    public bool isBeingPulled = false;
    public bool isGroundedGrappling = false;

    private PlayerController pc;
    private Animator animator;
    private Rigidbody2D rb;
    private GrappleHead activeHead;

    // tracks whether the current grapple started from the air (persists through pull)
    private bool startedFromAir = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        pc = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    Transform CurrentSpawnPoint()
    {
        // while being pulled
        if (isBeingPulled)
        {
            if (startedFromAir && grapplePulledAirSpawnPoint != null)
                return grapplePulledAirSpawnPoint;
            if (grapplePulledSpawnPoint != null)
                return grapplePulledSpawnPoint;
        }

        // while grappling (not pulling yet)
        if (isGrappling && !isGroundedGrappling && grappleAirSpawnPoint != null)
            return grappleAirSpawnPoint;

        return grappleSpawnPoint;
    }

    Vector3 CurrentSpawnWorldPosition()
    {
        Transform sp = CurrentSpawnPoint();
        if (sp == null) return transform.position;

        Vector3 localOffset = sp.localPosition;
        if (!pc.facingRight)
            localOffset.x = -localOffset.x;

        return transform.TransformPoint(localOffset);
    }

    bool IsPlayerGrounded()
    {
        if (pc == null || pc.groundCheck == null) return false;
        return Physics2D.OverlapCircle(pc.groundCheck.position, pc.groundCheckRadius, pc.groundLayer);
    }

    public void StartGrapple()
    {
        if (isGrappling || isBeingPulled) return;

        isGrappling = true;

        animator.ResetTrigger("GrappleShootEnd");

        bool grounded = IsPlayerGrounded();
        startedFromAir = !grounded;

        if (grounded)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            isGroundedGrappling = true;
        }

        if (grounded)
            animator.SetTrigger("GrappleShoot");
        else
            animator.SetTrigger("GrappleShootAir");

        Vector3 spawnPos = CurrentSpawnWorldPosition();
        Vector2 dir = pc.facingRight ? Vector2.right : Vector2.left;

        GameObject headObj = Instantiate(grappleHeadPrefab, spawnPos, Quaternion.identity);
        activeHead = headObj.GetComponent<GrappleHead>();
        activeHead.Launch(dir, this);

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
        }
    }

    void Update()
    {
        if (lineRenderer != null && lineRenderer.enabled && activeHead != null)
        {
            lineRenderer.SetPosition(0, CurrentSpawnWorldPosition());
            lineRenderer.SetPosition(1, activeHead.transform.position);
        }
    }

    public void OnGrappleHit(Vector3 hitPosition)
    {
        StartCoroutine(PullToTarget(hitPosition));
    }

    IEnumerator PullToTarget(Vector3 target)
    {
        isBeingPulled = true;
        animator.SetBool("GrappleGetPulled", true);

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        while (Vector3.Distance(transform.position, target) > stopDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, pullSpeed * Time.deltaTime);

            if (lineRenderer != null && activeHead != null)
            {
                lineRenderer.SetPosition(0, CurrentSpawnWorldPosition());
                lineRenderer.SetPosition(1, activeHead.transform.position);
            }

            yield return null;
        }

        // arrived
        isBeingPulled = false;
        isGroundedGrappling = false;
        startedFromAir = false;
        animator.SetBool("GrappleGetPulled", false);
        animator.SetTrigger("GrappleShootEnd");

        animator.ResetTrigger("GrappleShoot");
        animator.ResetTrigger("GrappleShootAir");

        if (pc.facingRight)
            animator.Play("Idle_Right");
        else
            animator.Play("Idle_Left");

        if (activeHead != null) Destroy(activeHead.gameObject);
        activeHead = null;

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;

        isGrappling = false;
    }

    public void OnGrappleRetracted()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;

        animator.SetTrigger("GrappleShootEnd");

        animator.ResetTrigger("GrappleShoot");
        animator.ResetTrigger("GrappleShootAir");

        if (pc.facingRight)
            animator.Play("Idle_Right");
        else
            animator.Play("Idle_Left");

        rb.bodyType = RigidbodyType2D.Dynamic;
        isGrappling = false;
        isGroundedGrappling = false;
        startedFromAir = false;
        activeHead = null;
    }
}