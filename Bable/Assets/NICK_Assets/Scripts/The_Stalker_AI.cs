using System.Collections;
using UnityEngine;

public class The_Stalker_AI : EnemyAI
{
    public enum State { Shadow, Attack, Retreat }

    [Header("Stalker")]
    public State currentState = State.Shadow;

    [Header("Invisibility")]
    public float visibleDistance = 4f;
    public float semiVisibleAlpha = 0.25f;
    private SpriteRenderer spriteRenderer;
    private float forceVisibleTimer = 0f;

    [Header("Movement Tuning")]
    public float retreatSpeed = 4f;
    public float retreatDuration = 0.8f;
    public float dodgeSpeed = 5f;
    public float circleSpeed = 1.5f;
    public float engageSpeed = 3.5f;

    [Header("AI Tuning")]
    public float patienceMin = 0.5f;
    public float patienceMax = 2f;
    public float preferredDistance = 3.5f;
    public float aggressionGrowthRate = 0.1f;

    private PlayerController playerController;
    private Animator playerAnim;
    private Camera mainCam;
    private Vector3 lastPlayerPos;
    private bool isRetreating = false;
    private bool isDodging = false;

    // ─── AI BRAIN ───────────────────────────────
    private float aggression = 0f;           // 0 = patient, 1 = aggressive
    private float patience = 0f;             // countdown before committing to attack
    private int attacksLanded = 0;
    private int attacksWhiffed = 0;
    private int timesHit = 0;
    private float lastAttackResult = 0f;     // +1 landed, -1 whiffed
    private float circleDirection = 1f;      // 1 or -1, which side to orbit
    private float circleTimer = 0f;
    private float decisionTimer = 0f;
    private bool waitingForOpening = true;

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
            playerAnim = playerObj.GetComponent<Animator>();
        }

        lastPlayerPos = player != null ? player.position : transform.position;
        patience = Random.Range(patienceMin, patienceMax);
        circleDirection = Random.value > 0.5f ? 1f : -1f;
    }

    protected override void Update()
    {
        if (player == null || enemyHealth.isHurt) return;

        CheckGrounded();
        UpdateForceVisibleTimer();
        UpdateAggression();

        switch (currentState)
        {
            case State.Shadow: ShadowUpdate(); break;
            case State.Attack: AttackUpdate(); break;
            case State.Retreat: RetreatUpdate(); break;
        }

        UpdateVisibility();
        lastPlayerPos = player.position;
    }

    // ═════════════════════════════════════════════
    // AI BRAIN — the actual decision-making
    // ═════════════════════════════════════════════

    void UpdateAggression()
    {
        // Aggression builds over time — the longer the fight goes, the bolder it gets
        aggression = Mathf.Clamp01(aggression + aggressionGrowthRate * Time.deltaTime);

        // Landing hits makes it bolder, getting hit makes it cautious
        if (lastAttackResult > 0)
            aggression = Mathf.Clamp01(aggression + 0.15f);
        else if (lastAttackResult < 0)
            aggression = Mathf.Clamp01(aggression - 0.2f);

        lastAttackResult = 0f;

        // Adjust preferred distance based on experience
        if (timesHit > attacksLanded && timesHit > 2)
            preferredDistance = Mathf.Min(preferredDistance + Time.deltaTime * 0.5f, 6f);
        else if (attacksLanded > timesHit && attacksLanded > 2)
            preferredDistance = Mathf.Max(preferredDistance - Time.deltaTime * 0.5f, 2f);
    }

    bool IsPlayerVulnerable()
    {
        if (playerAnim == null || playerController == null) return false;

        // Player is mid-slash (committed to animation, can't dodge)
        if (playerAnim.GetBool("IsSlashing")) return true;

        // Player is rolling (recovery frames at the end)
        if (playerController.isRolling) return true;

        // Player is landing from a jump/fall
        if (playerAnim.GetBool("IsFalling") && IsPlayerNearGround()) return true;

        // Player is hurt (stunned)
        if (playerController.isHurt) return true;

        return false;
    }

    bool IsPlayerNearGround()
    {
        if (player == null) return false;
        RaycastHit2D hit = Physics2D.Raycast(player.position, Vector2.down, 1.5f, groundLayer);
        return hit.collider != null;
    }

    bool IsPlayerFacingMe()
    {
        if (playerAnim == null) return true;
        bool playerFacingRight = playerAnim.GetBool("FacingRight");
        bool stalkerIsToRight = transform.position.x > player.position.x;
        return playerFacingRight == stalkerIsToRight;
    }

    bool ShouldAttackNow()
    {
        // Always go if player is vulnerable
        if (IsPlayerVulnerable()) return true;

        // High aggression = more likely to commit
        if (aggression > 0.7f && Random.value < aggression) return true;

        // If we've been patient long enough
        if (patience <= 0f) return true;

        return false;
    }

    // ═════════════════════════════════════════════
    // SHADOW — stalk, circle, wait for opening
    // ═════════════════════════════════════════════

    void ShadowUpdate()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (isAttacking || isDodging) return;

        // Tick down patience
        patience -= Time.deltaTime;

        // Periodically switch circle direction so it doesn't look robotic
        circleTimer += Time.deltaTime;
        if (circleTimer > Random.Range(1.5f, 3f))
        {
            circleDirection *= -1f;
            circleTimer = 0f;
        }

        // DECISION: close enough to consider attacking?
        if (dist <= attackRange * 1.5f && ShouldAttackNow())
        {
            currentState = State.Attack;
            waitingForOpening = false;
            return;
        }

        // DECISION: how to move
        if (dist > sightRange)
        {
            // Too far — beeline toward player
            FacePlayer();
            float dir = player.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * engageSpeed, rb.linearVelocity.y);
            animator.SetBool("IsWalking", true);
        }
        else if (dist > preferredDistance)
        {
            // Closing in — approach but at an angle (flanking)
            FacePlayer();
            float approachDir = player.position.x > transform.position.x ? 1f : -1f;

            // Try to approach from the side the player ISN'T facing
            if (IsPlayerFacingMe() && dist > attackRange * 2f)
            {
                // Player sees us — circle around instead of walking straight in
                rb.linearVelocity = new Vector2(circleDirection * circleSpeed, rb.linearVelocity.y);
                animator.SetBool("IsWalking", true);
            }
            else
            {
                // Player's back is to us — close in
                float closingSpeed = Mathf.Lerp(circleSpeed, engageSpeed, aggression);
                rb.linearVelocity = new Vector2(approachDir * closingSpeed, rb.linearVelocity.y);
                animator.SetBool("IsWalking", true);
            }
        }
        else
        {
            // At preferred distance — orbit and wait for opening
            FacePlayer();

            if (IsPlayerVulnerable())
            {
                // Opening detected — rush in
                float rushDir = player.position.x > transform.position.x ? 1f : -1f;
                rb.linearVelocity = new Vector2(rushDir * engageSpeed, rb.linearVelocity.y);
                animator.SetBool("IsWalking", true);
            }
            else
            {
                // Circle at preferred distance
                rb.linearVelocity = new Vector2(circleDirection * circleSpeed, rb.linearVelocity.y);
                animator.SetBool("IsWalking", true);
            }
        }

        NavigateObstacles();
    }

    // ═════════════════════════════════════════════
    // ATTACK — commit, dodge if needed, judge result
    // ═════════════════════════════════════════════

    void AttackUpdate()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        FacePlayer();

        // Player swinging at us — dodge (but only if we're not already committed)
        if (IsPlayerAttacking() && dist < attackRange * 2f && !isDodging && !isAttacking)
        {
            // Smarter dodge: only dodge if not very aggressive
            if (aggression < 0.8f || Random.value > aggression)
            {
                StartCoroutine(Dodge());
                return;
            }
            // High aggression = trade hits, don't dodge
        }

        if (dist <= attackRange && !isAttacking && !isDodging)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("IsWalking", false);
            BecomeVisible(0.6f);
            StartCoroutine(StalkerAttack());
        }
        else if (!isAttacking && !isDodging)
        {
            // Close the gap
            float dir = player.position.x > transform.position.x ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * engageSpeed, rb.linearVelocity.y);
            animator.SetBool("IsWalking", true);
            NavigateObstacles();

            // If player ran too far away, give up and go back to shadow
            if (dist > sightRange * 0.8f)
            {
                currentState = State.Shadow;
                patience = Random.Range(patienceMin, patienceMax);
            }
        }
    }

    // ═════════════════════════════════════════════
    // RETREAT — back off, learn, reset
    // ═════════════════════════════════════════════

    void RetreatUpdate()
    {
        if (!isRetreating)
            StartCoroutine(RetreatRoutine());
    }

    IEnumerator RetreatRoutine()
    {
        isRetreating = true;

        float awayDir = transform.position.x > player.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(awayDir * retreatSpeed, rb.linearVelocity.y);
        animator.SetBool("IsWalking", true);

        if (awayDir > 0 && !facingRight) Flip();
        else if (awayDir < 0 && facingRight) Flip();

        yield return new WaitForSeconds(retreatDuration);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("IsWalking", false);

        // After retreating, become more cautious
        patience = Random.Range(patienceMin, patienceMax) * 1.5f;
        waitingForOpening = true;

        currentState = State.Shadow;
        isRetreating = false;
    }

    public void OnDamageTaken()
    {
        BecomeVisible(0.5f);
        timesHit++;
        lastAttackResult = -1f;

        if (currentState != State.Retreat)
            currentState = State.Retreat;
    }

    // ═════════════════════════════════════════════
    // COMBAT
    // ═════════════════════════════════════════════

    IEnumerator StalkerAttack()
    {
        isAttacking = true;
        BecomeVisible(0.6f);

        yield return null;
        if (enemyHealth.isHurt) { isAttacking = false; yield break; }

        animator.SetTrigger("AttackRight");
        yield return new WaitForSeconds(hitboxDelay);

        if (enemyHealth.isHurt)
        {
            if (enemyHitbox != null)
                enemyHitbox.GetComponent<Collider2D>().enabled = false;
            isAttacking = false;
            yield break;
        }

        if (enemyHitbox != null)
        {
            Vector3 hitboxPos = enemyHitbox.transform.localPosition;
            hitboxPos.x = facingRight ? Mathf.Abs(hitboxPos.x) : -Mathf.Abs(hitboxPos.x);
            enemyHitbox.transform.localPosition = hitboxPos;
            enemyHitbox.GetComponent<Collider2D>().enabled = true;
        }

        // Check if we actually hit the player
        yield return new WaitForSeconds(hitboxDuration);

        bool landed = CheckHitLanded();

        if (enemyHitbox != null)
            enemyHitbox.GetComponent<Collider2D>().enabled = false;

        if (landed)
        {
            attacksLanded++;
            lastAttackResult = 1f;
        }
        else
        {
            attacksWhiffed++;
            lastAttackResult = -0.5f;
        }

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        patience = Random.Range(patienceMin, patienceMax);
        currentState = State.Shadow;
    }

    bool CheckHitLanded()
    {
        if (player == null) return false;
        if (playerController != null && playerController.isHurt) return true;
        float dist = Vector2.Distance(transform.position, player.position);
        return dist <= attackRange * 1.2f;
    }

    IEnumerator Dodge()
    {
        isDodging = true;

        float dodgeDir = transform.position.x > player.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(dodgeDir * dodgeSpeed, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.2f);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Brief pause — looks like it's reading the player
        yield return new WaitForSeconds(0.15f);

        isDodging = false;

        // After dodging, punish the player's whiff if aggressive enough
        if (aggression > 0.5f)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= attackRange * 2f)
            {
                currentState = State.Attack;
                yield break;
            }
        }
    }

    // ═════════════════════════════════════════════
    // INVISIBILITY
    // ═════════════════════════════════════════════

    void UpdateVisibility()
    {
        if (spriteRenderer == null) return;

        float targetAlpha;

        if (forceVisibleTimer > 0f)
        {
            targetAlpha = 1f;
        }
        else if (currentState == State.Attack || isAttacking)
        {
            targetAlpha = 1f;
        }
        else if (IsOffScreen() || IsBehindWall())
        {
            targetAlpha = 0f;
        }
        else
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= visibleDistance)
                targetAlpha = Mathf.Lerp(1f, semiVisibleAlpha, dist / visibleDistance);
            else
                targetAlpha = 0f;
        }

        Color c = spriteRenderer.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 10f);
        spriteRenderer.color = c;
    }

    void BecomeVisible(float duration)
    {
        forceVisibleTimer = Mathf.Max(forceVisibleTimer, duration);
    }

    void UpdateForceVisibleTimer()
    {
        if (forceVisibleTimer > 0f)
            forceVisibleTimer -= Time.deltaTime;
    }

    // ═════════════════════════════════════════════
    // HELPERS
    // ═════════════════════════════════════════════

    void FacePlayer()
    {
        if (isAttacking || isDodging) return;

        if (player.position.x < transform.position.x && facingRight)
            Flip();
        else if (player.position.x > transform.position.x && !facingRight)
            Flip();
    }

    bool IsPlayerAttacking()
    {
        if (playerAnim == null) return false;
        return playerAnim.GetBool("IsSlashing");
    }

    bool IsOffScreen()
    {
        if (mainCam == null) return false;
        Vector3 vp = mainCam.WorldToViewportPoint(transform.position);
        return vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f;
    }

    bool IsBehindWall()
    {
        if (player == null) return false;
        Vector2 dir = (player.position - transform.position).normalized;
        float dist = Vector2.Distance(transform.position, player.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }
}
