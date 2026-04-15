using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerpentAI : MonoBehaviour
{
    [Header("Detection")]
    public float sightRange = 8f;
    public float attackRange = 2f;
    public float yTolerance = 2f;

    [Header("Sight")]
    public LayerMask sightBlockLayers;

    [Header("Movement")]
    public float crawlSpeed = 2f;

    [Header("Hurtboxes")]
    public Collider2D idleHurtbox;
    public Collider2D crawlHurtbox;
    public Collider2D windupHurtbox;
    public Collider2D attackHurtbox;

    [Header("Bomb Mouth Trigger")]
    public Collider2D mouthTrigger;

    [Header("Attack Effects")]
    public Collider2D fireBreathHitbox;
    public BoxCollider2D fireSpreadZone;
    public GameObject groundFirePrefab;
    public LayerMask groundFireLayer;
    public float fireSpreadSpacing = 1f;
    public float fireSpreadDelay = 0.1f;
    public float fireSpawnHeightOffset = 0.1f;

    [Header("Upward Attack")]
    public Collider2D upwardFireBreathHitbox;
    public Vector2 upwardRangeOffset = new Vector2(0f, 3f);
    public float upwardAttackRange = 2f;

    [Header("Attack Duration")]
    public float normalAttackDuration = 2f;
    public float upwardAttackDuration = 2f;

    [Header("Platform")]
    public Vector2 platformVelocity = Vector2.zero;

    private Animator animator;
    private Rigidbody2D rb;
    private SerpentHealth serpentHealth;
    private Transform player;
    private bool facingRight = true;
    public bool isInSequence = false;
    private bool isBombSwallowed = false;
    private List<GameObject> activeGroundFires = new List<GameObject>();
    private Coroutine despawnFiresCoroutine = null;

    public string stateBeforeHurt = "Idle";
    public float stateTimeBeforeHurt = 0f;

    private float windupElapsed = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        serpentHealth = GetComponent<SerpentHealth>();
        player = GameObject.FindWithTag("Player").transform;

        if (mouthTrigger != null)
            mouthTrigger.enabled = false;

        if (fireBreathHitbox != null)
            fireBreathHitbox.gameObject.SetActive(false);

        if (upwardFireBreathHitbox != null)
            upwardFireBreathHitbox.gameObject.SetActive(false);

        SetHurtbox("Idle");
    }

    void SetHurtbox(string state)
    {
        if (idleHurtbox != null) idleHurtbox.enabled = false;
        if (crawlHurtbox != null) crawlHurtbox.enabled = false;
        if (windupHurtbox != null) windupHurtbox.enabled = false;
        if (attackHurtbox != null) attackHurtbox.enabled = false;

        switch (state)
        {
            case "Idle":
                if (idleHurtbox != null) idleHurtbox.enabled = true;
                break;
            case "Crawl":
                if (crawlHurtbox != null) crawlHurtbox.enabled = true;
                break;
            case "Windup":
                if (windupHurtbox != null) windupHurtbox.enabled = true;
                break;
            case "Attack":
                if (attackHurtbox != null) attackHurtbox.enabled = true;
                break;
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    bool CanSeePlayer()
    {
        if (Mathf.Abs(transform.position.y - player.position.y) > yTolerance) return false;

        if (sightBlockLayers == 0) return true;

        Vector2 origin = new Vector2(transform.position.x, transform.position.y + 1f);
        Vector2 target = new Vector2(player.position.x, player.position.y + 1f);
        Vector2 direction = (target - origin).normalized;
        float distance = Vector2.Distance(origin, target);
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, sightBlockLayers);

        return hit.collider == null;
    }

    bool IsPlayerInUpwardRange()
    {
        Vector2 upwardCenter = (Vector2)transform.position + upwardRangeOffset;
        return Vector2.Distance(upwardCenter, player.position) <= upwardAttackRange;
    }

    void Update()
    {
        if (serpentHealth.isDead) return;
        if (serpentHealth.isHurt) return;
        if (isBombSwallowed) return;
        if (isInSequence) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer();

        if (canSee)
        {
            if (player.position.x < transform.position.x && facingRight)
                Flip();
            else if (player.position.x > transform.position.x && !facingRight)
                Flip();
        }

        if ((distanceToPlayer <= attackRange || IsPlayerInUpwardRange()) && canSee)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsCrawling", false);
            isInSequence = true;
            StartCoroutine(AttackSequence());
            return;
        }
        else if (distanceToPlayer <= sightRange && canSee)
        {
            animator.SetBool("IsCrawling", true);
            SetHurtbox("Crawl");
            stateBeforeHurt = "Crawl";
        }
        else
        {
            animator.SetBool("IsCrawling", false);
            SetHurtbox("Idle");
            stateBeforeHurt = "Idle";
        }
    }

    void FixedUpdate()
    {
        if (serpentHealth.isDead) return;
        if (serpentHealth.isHurt) return;
        if (isBombSwallowed) return;
        if (isInSequence) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer();

        if (distanceToPlayer <= sightRange && distanceToPlayer > attackRange && !IsPlayerInUpwardRange() && canSee)
        {
            float direction = player.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * crawlSpeed + platformVelocity.x, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(platformVelocity.x, rb.linearVelocity.y);
        }

        platformVelocity = Vector2.zero;
    }

    IEnumerator AttackSequence()
    {
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        animator.SetTrigger("Windup");
        SetHurtbox("Windup");
        stateBeforeHurt = "Windup";

        if (mouthTrigger != null)
            mouthTrigger.enabled = true;

        float windupLength = GetAnimationLength("Windup");
        windupElapsed = 0f;

        while (windupElapsed < windupLength)
        {
            if (!serpentHealth.isHurt)
                windupElapsed += Time.deltaTime;
            yield return null;
        }

        if (mouthTrigger != null)
            mouthTrigger.enabled = false;

        if (isBombSwallowed) yield break;
        if (serpentHealth.isDead) yield break;

        if (IsPlayerInUpwardRange())
            yield return StartCoroutine(UpwardAttack());
        else
            yield return StartCoroutine(NormalAttack());

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        isInSequence = false;

        animator.SetBool("IsCrawling", false);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsUpwardAttacking", false);
        SetHurtbox("Idle");
        stateBeforeHurt = "Idle";
    }

    IEnumerator NormalAttack()
    {
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        if (despawnFiresCoroutine != null)
        {
            StopCoroutine(despawnFiresCoroutine);
            despawnFiresCoroutine = null;
        }

        animator.SetBool("IsAttacking", true);
        animator.SetBool("IsUpwardAttacking", false);
        SetHurtbox("Attack");
        stateBeforeHurt = "Attack";

        yield return null;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            yield return null;

        if (fireBreathHitbox != null)
            fireBreathHitbox.gameObject.SetActive(true);

        if (activeGroundFires.Count == 0)
            SpawnGroundFires();

        float elapsed = 0f;
        while (elapsed < normalAttackDuration)
        {
            if (isBombSwallowed) yield break;
            if (serpentHealth.isDead) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }

        animator.SetBool("IsAttacking", false);
        if (fireBreathHitbox != null) fireBreathHitbox.gameObject.SetActive(false);
        despawnFiresCoroutine = StartCoroutine(DespawnFiresAfterDelay(2f));
    }

    IEnumerator UpwardAttack()
    {
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        animator.SetBool("IsUpwardAttacking", true);
        animator.SetBool("IsAttacking", false);
        SetHurtbox("Attack");
        stateBeforeHurt = "Attack";

        yield return null;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("UpwardAttack"))
            yield return null;

        if (upwardFireBreathHitbox != null)
            upwardFireBreathHitbox.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < upwardAttackDuration)
        {
            if (isBombSwallowed) yield break;
            if (serpentHealth.isDead) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }

        animator.SetBool("IsUpwardAttacking", false);
        if (upwardFireBreathHitbox != null) upwardFireBreathHitbox.gameObject.SetActive(false);
    }

    IEnumerator DespawnFiresAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DespawnGroundFires();
        despawnFiresCoroutine = null;
    }

    void SpawnGroundFires()
    {
        if (fireSpreadZone == null || groundFirePrefab == null) return;

        Bounds bounds = fireSpreadZone.bounds;
        float x = bounds.min.x;

        List<Vector2> firePositions = new List<Vector2>();

        while (x <= bounds.max.x)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                new Vector2(x, bounds.max.y),
                Vector2.down,
                bounds.size.y,
                groundFireLayer);

            if (hit.collider != null)
                firePositions.Add(new Vector2(x, hit.point.y + fireSpawnHeightOffset));

            x += fireSpreadSpacing;
        }

        Vector2 center = new Vector2(bounds.center.x, bounds.center.y);
        firePositions.Sort((a, b) =>
            Vector2.Distance(a, center).CompareTo(
            Vector2.Distance(b, center)));

        StartCoroutine(SpawnFiresInWave(firePositions));
    }

    IEnumerator SpawnFiresInWave(List<Vector2> positions)
    {
        foreach (Vector2 pos in positions)
        {
            if (isBombSwallowed) break;
            if (serpentHealth.isDead) break;

            GameObject fire = Instantiate(groundFirePrefab, pos, Quaternion.identity);
            activeGroundFires.Add(fire);

            yield return new WaitForSeconds(fireSpreadDelay);
        }
    }

    void DespawnGroundFires()
    {
        foreach (GameObject fire in activeGroundFires)
        {
            if (fire != null)
                Destroy(fire);
        }
        activeGroundFires.Clear();
    }

    public void BombSwallowed()
    {
        if (isBombSwallowed) return;
        isBombSwallowed = true;

        if (mouthTrigger != null)
            mouthTrigger.enabled = false;

        if (fireBreathHitbox != null)
            fireBreathHitbox.gameObject.SetActive(false);

        if (upwardFireBreathHitbox != null)
            upwardFireBreathHitbox.gameObject.SetActive(false);

        if (despawnFiresCoroutine != null)
        {
            StopCoroutine(despawnFiresCoroutine);
            despawnFiresCoroutine = null;
        }

        DespawnGroundFires();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        StopAllCoroutines();
        isInSequence = false;

        StartCoroutine(BombSwallowedSequence());
    }

    IEnumerator BombSwallowedSequence()
    {
        animator.SetBool("IsCrawling", false);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsUpwardAttacking", false);
        animator.SetTrigger("BombSwallowed");
        SetHurtbox("Idle");

        yield return new WaitForSeconds(GetAnimationLength("BombSwallowed"));

        serpentHealth.KillInstantly();
    }

    public void RestoreStateAfterHurt()
    {
        if (isBombSwallowed) return;
        if (serpentHealth.isDead) return;

        if (stateBeforeHurt == "Windup")
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            float normalizedTime = windupElapsed / GetAnimationLength("Windup");
            animator.Play("Windup", 0, normalizedTime);
            SetHurtbox("Windup");
            if (mouthTrigger != null) mouthTrigger.enabled = true;
        }
        else if (stateBeforeHurt == "Attack")
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            animator.SetBool("IsAttacking", true);
            SetHurtbox("Attack");
            if (fireBreathHitbox != null)
                fireBreathHitbox.gameObject.SetActive(true);
        }
        else if (stateBeforeHurt == "Crawl")
        {
            isInSequence = false;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            animator.SetBool("IsCrawling", true);
            SetHurtbox("Crawl");
        }
        else
        {
            isInSequence = false;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            animator.SetBool("IsCrawling", false);
            SetHurtbox("Idle");
        }
    }

    public void SaveStateBeforeHurt()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        stateTimeBeforeHurt = stateInfo.normalizedTime % 1f;

        if (stateInfo.IsName("Crawl"))
            stateBeforeHurt = "Crawl";
        else if (stateInfo.IsName("Windup"))
            stateBeforeHurt = "Windup";
        else if (stateInfo.IsName("Attack"))
            stateBeforeHurt = "Attack";
        else if (stateInfo.IsName("UpwardAttack"))
            stateBeforeHurt = "Attack";
        else
            stateBeforeHurt = "Idle";
    }

    float GetAnimationLength(string stateName)
    {
        if (animator == null) return 1f;
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;

        string clipName = stateName;
        switch (stateName)
        {
            case "Windup": clipName = "stand_windup"; break;
            case "Attack": clipName = "attack"; break;
            case "Crawl": clipName = "crawl"; break;
            case "Idle": clipName = "idle"; break;
            case "Hurt": clipName = "hurt"; break;
            case "BombSwallowed": clipName = "Bomb_swallow"; break;
        }

        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }

        return 1f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere((Vector2)transform.position + upwardRangeOffset, upwardAttackRange);
    }
}