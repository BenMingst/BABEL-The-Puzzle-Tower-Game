using System.Collections;
using UnityEngine;

public class GrappleGlove : MonoBehaviour
{
    public static GrappleGlove Instance;

    [Header("Grapple")]
    public GameObject grappleHeadPrefab;
    public Transform grappleSpawnPoint;

    [Header("Pull Settings")]
    public float pullSpeed = 15f;
    public float stopDistance = 0.5f;

    [Header("Line Renderer")]
    public LineRenderer lineRenderer;

    public bool isGrappling = false;
    public bool isBeingPulled = false;

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

    public void StartGrapple()
    {
        if (isGrappling || isBeingPulled) return;

        isGrappling = true;

        // lock player in place during shoot
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // play shoot animation (stays on last frame)
        animator.SetTrigger("GrappleShoot");

        // spawn head
        Vector3 spawnPos = grappleSpawnPoint.position;
        Vector2 dir = pc.facingRight ? Vector2.right : Vector2.left;

        GameObject headObj = Instantiate(grappleHeadPrefab, spawnPos, Quaternion.identity);
        activeHead = headObj.GetComponent<GrappleHead>();
        activeHead.Launch(dir, this);

        // enable line renderer
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
        }
    }

    void Update()
    {
        // draw line from spawn point to head position every frame
        if (lineRenderer != null && lineRenderer.enabled && activeHead != null)
        {
            lineRenderer.SetPosition(0, grappleSpawnPoint.position);
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

        while (Vector3.Distance(transform.position, target) > stopDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, pullSpeed * Time.deltaTime);

            // keep line drawing during pull
            if (lineRenderer != null && activeHead != null)
            {
                lineRenderer.SetPosition(0, grappleSpawnPoint.position);
                lineRenderer.SetPosition(1, activeHead.transform.position);
            }

            yield return null;
        }

        // arrived
        isBeingPulled = false;
        animator.SetBool("GrappleGetPulled", false);
        animator.SetTrigger("GrappleShootEnd");

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
        // hit nothing, grapple returned empty
        if (lineRenderer != null)
            lineRenderer.enabled = false;

        animator.SetTrigger("GrappleShootEnd");
        rb.bodyType = RigidbodyType2D.Dynamic;
        isGrappling = false;
        activeHead = null;
    }
}