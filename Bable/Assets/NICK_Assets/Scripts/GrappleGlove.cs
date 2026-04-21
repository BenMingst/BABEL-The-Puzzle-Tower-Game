using System.Collections;
using UnityEngine;

public class GrappleGlove : MonoBehaviour
{
    public static GrappleGlove Instance;

    [Header("Grapple")]
    public GameObject grappleHeadPrefab;
    public Transform grappleSpawnPoint;
    public Transform grapplePulledSpawnPoint;

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
        return isBeingPulled && grapplePulledSpawnPoint != null
            ? grapplePulledSpawnPoint
            : grappleSpawnPoint;
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

        Vector3 spawnPos = CurrentSpawnPoint().position;
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
            lineRenderer.SetPosition(0, CurrentSpawnPoint().position);
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
                lineRenderer.SetPosition(0, CurrentSpawnPoint().position);
                lineRenderer.SetPosition(1, activeHead.transform.position);
            }

            yield return null;
        }

        // arrived
        isBeingPulled = false;
        isGroundedGrappling = false;
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
        activeHead = null;
    }
}