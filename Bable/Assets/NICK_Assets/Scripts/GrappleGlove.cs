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
    public Transform getOverHereSpawnPoint;
    public Transform getOverHereAirSpawnPoint;

    [Header("Pull Settings")]
    public float pullSpeed = 15f;
    public float stopDistance = 0.5f;

    [Header("Enemy Catch Settings")]
    public Transform enemyReleasePoint;
    public float enemyPullSpeed = 20f;

    [Header("Line Renderer")]
    public LineRenderer lineRenderer;

    public bool isGrappling = false;
    public bool isBeingPulled = false;
    public bool isGroundedGrappling = false;
    public bool isCatchingEnemy = false;

    [Header("Audio")]
    [SerializeField] private AudioSource soundFXObject;
    [SerializeField] private AudioClip grappleShootSFX;
    [SerializeField] private AudioClip grappleRetractSFX;

    private PlayerController pc;
    private Animator animator;
    private Rigidbody2D rb;
    private GrappleHead activeHead;
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

        if (soundFXObject == null)
            soundFXObject = gameObject.AddComponent<AudioSource>();

        soundFXObject.playOnAwake = false;

        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    Transform CurrentSpawnPoint()
    {
        if (isCatchingEnemy)
        {
            if (startedFromAir && getOverHereAirSpawnPoint != null)
                return getOverHereAirSpawnPoint;
            if (getOverHereSpawnPoint != null)
                return getOverHereSpawnPoint;
        }

        if (isBeingPulled)
        {
            if (startedFromAir && grapplePulledAirSpawnPoint != null)
                return grapplePulledAirSpawnPoint;
            if (grapplePulledSpawnPoint != null)
                return grapplePulledSpawnPoint;
        }

        if (isGrappling && !isGroundedGrappling && grappleAirSpawnPoint != null)
            return grappleAirSpawnPoint;

        return grappleSpawnPoint;
    }

    public Vector3 CurrentSpawnWorldPosition()
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

    bool IsPlayerBlockedByWall()
    {
        if (pc == null) return false;

        Collider2D playerCol = pc.standingCollider != null ? pc.standingCollider : pc.GetComponent<Collider2D>();
        if (playerCol == null) return false;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            playerCol.bounds.center,
            playerCol.bounds.size * 0.9f,
            0f,
            pc.groundLayer);

        foreach (var hit in hits)
        {
            if (hit != playerCol && hit.gameObject != gameObject)
                return true;
        }

        return false;
    }

    bool IsLineBlockedByGround(Vector3 targetPosition)
    {
        if (pc == null) return false;

        Vector3 spawnPos = CurrentSpawnWorldPosition();
        Vector2 direction = (targetPosition - spawnPos);
        float distance = direction.magnitude;

        if (distance < 0.01f) return false;

        RaycastHit2D hit = Physics2D.Raycast(spawnPos, direction.normalized, distance, pc.groundLayer);
        return hit.collider != null;
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

        // play grapple shoot sound
        if (soundFXObject != null && grappleShootSFX != null)
            SoundManager.instance.PlayWorldClip(grappleShootSFX, transform, 1f);
        
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
        // play grapple retract sound
        if (soundFXObject != null && grappleRetractSFX != null)
            SoundManager.instance.PlayWorldClip(grappleRetractSFX, transform, 1f);
        StartCoroutine(PullToTarget(hitPosition));
    }

    public void OnGrappleHitEnemy(GrappleCatchable caught)
    {
        // play grapple retract sound
        if (soundFXObject != null && grappleRetractSFX != null)
            SoundManager.instance.PlayWorldClip(grappleRetractSFX, transform, 1f);
        StartCoroutine(PullEnemyToPlayer(caught));
    }

    public void OnGrappleHitBlock(GrappleableBlock block)
    {
        // play grapple retract sound
        if (soundFXObject != null && grappleRetractSFX != null)
            SoundManager.instance.PlayWorldClip(grappleRetractSFX, transform, 1f);
        StartCoroutine(PullBlockToPlayer(block));
    }

    IEnumerator PullEnemyToPlayer(GrappleCatchable caught)
    {
        isBeingPulled = true;
        isCatchingEnemy = true;

        bool facingRight = pc.facingRight;
        if (startedFromAir)
            animator.SetTrigger(facingRight ? "GetOverHereAirRight" : "GetOverHereAirLeft");
        else
            animator.SetTrigger(facingRight ? "GetOverHereRight" : "GetOverHereLeft");

        caught.OnGrappleCaught();

        Vector3 releasePoint;
        if (enemyReleasePoint != null)
        {
            Vector3 localOffset = enemyReleasePoint.localPosition;
            if (!facingRight)
                localOffset.x = -localOffset.x;
            releasePoint = transform.TransformPoint(localOffset);
        }
        else
        {
            releasePoint = transform.position + new Vector3(facingRight ? 1f : -1f, 0f, 0f);
        }

        Transform enemyTf = caught.transform;
        Vector3 anchorOffset = caught.GetAnchorPosition() - enemyTf.position;

        while (Vector3.Distance(caught.GetAnchorPosition(), releasePoint) > 0.1f)
        {
            Vector3 enemyTargetPos = releasePoint - anchorOffset;
            enemyTf.position = Vector3.MoveTowards(enemyTf.position, enemyTargetPos, enemyPullSpeed * Time.deltaTime);

            if (lineRenderer != null && activeHead != null)
            {
                activeHead.transform.position = caught.GetAnchorPosition();
                lineRenderer.SetPosition(0, CurrentSpawnWorldPosition());
                lineRenderer.SetPosition(1, caught.GetAnchorPosition());
            }

            if (IsLineBlockedByGround(caught.GetAnchorPosition()))
                break;

            yield return null;
        }

        caught.OnGrappleReleased();

        isBeingPulled = false;
        isCatchingEnemy = false;
        isGroundedGrappling = false;
        startedFromAir = false;
        animator.SetTrigger("GrappleShootEnd");
        animator.ResetTrigger("GrappleShoot");
        animator.ResetTrigger("GrappleShootAir");
        animator.ResetTrigger("GetOverHereRight");
        animator.ResetTrigger("GetOverHereLeft");
        animator.ResetTrigger("GetOverHereAirRight");
        animator.ResetTrigger("GetOverHereAirLeft");

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

    IEnumerator PullBlockToPlayer(GrappleableBlock block)
    {
        isBeingPulled = true;
        isCatchingEnemy = true;

        bool facingRight = pc.facingRight;
        if (startedFromAir)
            animator.SetTrigger(facingRight ? "GetOverHereAirRight" : "GetOverHereAirLeft");
        else
            animator.SetTrigger(facingRight ? "GetOverHereRight" : "GetOverHereLeft");

        Coroutine monitor = StartCoroutine(MonitorBlockPull(block));

        // run pull alongside hard timeout
        float timeout = 1f;
        float elapsed = 0f;
        Coroutine pullCoroutine = StartCoroutine(block.PullTowardPlayer(transform, lineRenderer, activeHead, CurrentSpawnWorldPosition));

        while (block.IsBeingPulled() && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (block.IsBeingPulled())
        {
            block.CancelPull();
            yield return null;
        }

        if (pullCoroutine != null) StopCoroutine(pullCoroutine);
        if (monitor != null) StopCoroutine(monitor);

        Rigidbody2D blockRb = block.GetComponent<Rigidbody2D>();
        if (blockRb != null)
            blockRb.linearVelocity = new Vector2(0f, blockRb.linearVelocity.y);

        isBeingPulled = false;
        isCatchingEnemy = false;
        isGroundedGrappling = false;
        startedFromAir = false;
        animator.SetTrigger("GrappleShootEnd");
        animator.ResetTrigger("GrappleShoot");
        animator.ResetTrigger("GrappleShootAir");
        animator.ResetTrigger("GetOverHereRight");
        animator.ResetTrigger("GetOverHereLeft");
        animator.ResetTrigger("GetOverHereAirRight");
        animator.ResetTrigger("GetOverHereAirLeft");

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

    IEnumerator MonitorBlockPull(GrappleableBlock block)
    {
        Vector3 lastPosition = block.transform.position;
        int stallFrames = 0;
        int stallFrameThreshold = 3;
        float stallDistance = 0.005f;

        while (block != null && block.IsBeingPulled())
        {
            if (IsLineBlockedByGround(block.transform.position))
            {
                block.CancelPull();
                yield break;
            }

            float moved = Vector3.Distance(block.transform.position, lastPosition);
            if (moved < stallDistance)
            {
                stallFrames++;
                if (stallFrames >= stallFrameThreshold)
                {
                    block.CancelPull();
                    yield break;
                }
            }
            else
            {
                stallFrames = 0;
            }

            lastPosition = block.transform.position;
            yield return null;
        }
    }

    IEnumerator PullToTarget(Vector3 target)
    {
        isBeingPulled = true;
        animator.SetBool("GrappleGetPulled", true);

        // play grapple retract loop sound
        if (soundFXObject == null)
        {
            soundFXObject = SoundManager.instance.PlayWorldClip(
                grappleRetractSFX,
                transform,
                1f
            );

            soundFXObject.loop = true;
        }

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

            if (IsPlayerBlockedByWall())
                break;

            yield return null;
        }

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
        if (soundFXObject != null)
        {
            Destroy(soundFXObject.gameObject);
            soundFXObject = null;
        }

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
        isCatchingEnemy = false;
        activeHead = null;
    }
}