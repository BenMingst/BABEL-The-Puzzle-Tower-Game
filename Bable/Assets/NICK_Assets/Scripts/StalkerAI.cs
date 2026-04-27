using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sole AI controller for "The Stalker" — a gutted player prefab with <see cref="EnemyHealth"/>.
/// Drives movement, visibility tint, teleport combat, and duel-phase behavior by <see cref="stalkerLevel"/> (1, 3, or 5).
/// Does not inherit from <see cref="EnemyAI"/>; mirrors its combat/movement conventions only.
/// </summary>
public class StalkerAI : MonoBehaviour
{
    #region Inspector — Stalker Config

    [Header("Stalker Config")]
    /// <summary>Difficulty / behavior tier: 1 = Presence, 3 = Hunter, 5 = Duel.</summary>
    public int stalkerLevel = 5;

    #endregion

    #region Inspector — Detection

    [Header("Detection")]
    public float sightRange = 5f;
    public float yTolerance = 2f;
    public LayerMask sightBlockLayers;

    #endregion

    #region Inspector — Level 1 — Presence

    [Header("Level 1 — Presence")]
    /// <summary>At or beyond this distance from the player, silhouette tint is fully on (<c>_TintAmount = 1</c>).</summary>
    public float visibilityFadeStart = 3f;
    /// <summary>Within this distance, the stalker is fully faded out (<c>_TintAmount = 0</c>).</summary>
    public float visibilityFadeEnd = 1f;

    #endregion

    #region Inspector — Level 3 — Hunter

    [Header("Level 3 — Hunter")]
    public float meleeThreshold = 4f;
    public float teleportMinInterval = 1.5f;
    public float teleportMaxInterval = 3.5f;
    public float bowWindupTime = 1.5f;
    public float dissolveTime = 0.1f;
    public Transform[] teleportNodes;

    #endregion

    #region Inspector — Level 5 — Duel

    [Header("Level 5 — Duel")]
    public float dashSpeed = 8f;
    public float dashCooldown = 0.3f;
    public float bombFuse = 1.2f;
    public float bombExplosionRadius = 2f;
    public float animSpeedMultiplier = 1.07f;
    public int rageHitThreshold = 3;

    #endregion

    #region Inspector — Combat (all levels)

    [Header("Combat (all levels)")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public float hitboxDelay = 0.45f;
    public float hitboxDuration = 0.1f;
    public float grappleSlowMultiplier = 1f;

    #endregion

    #region Inspector — Weapon References

    [Header("Weapon References")]
    public GameObject swordHitbox;
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public GameObject bombPrefab;

    #endregion

    #region Inspector — Visuals

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;

    #endregion

    #region Constants & cached components

    const float DashBurstDuration = 0.15f;
    const float TeleportProximity = 1.5f;
    const float StrafeWalkSpeed = 2f;
    const int TintId = Shader.PropertyToID("_TintAmount");

    static readonly int AnimIsWalking = Animator.StringToHash("IsWalking");

    const float Level5BombRepeatInterval = 3f;

    Animator animator;
    EnemyHealth enemyHealth;
    Rigidbody2D rb;
    Transform player;
    PlayerController playerController;
    Animator playerAnimator;
    Rigidbody2D playerRb;

    bool facingRight = true;
    protected bool isAttacking;
    float distanceToPlayer;
    bool canSee;

    #endregion

    #region Level 1 state

    /// <summary>Tracks one-time disabling of trigger hurt colliders for Presence mode.</summary>
    bool level1DamageCollidersDisabled;

    #endregion

    #region Level 3 state

    float teleportCountdown;
    bool isTeleporting;
    bool wasHurtLastFrameTeleport;

    #endregion

    #region Level 5 state — rage & duel

    int hitsTaken;
    bool isRaging;
    bool wasHurtLastFrameRage;
    float baseDashSpeed;
    float baseAttackCooldown;
    float dashBurstTimer;
    float dashCooldownTimer;
    bool isDashing;
    Vector2 dashDirection;
    float predictiveDodgeTimer;
    float predictiveDodgeVx;
    bool kiteBowActive;
    float strafeOrbitSign = 1f;
    float orbitFlipTimer;

    bool pendingSwordAfterDash;
    float level5BombCooldown;

    #endregion

    #region Core lifecycle

    void Awake()
    {
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();
        baseDashSpeed = dashSpeed;
        baseAttackCooldown = attackCooldown;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerController = p.GetComponent<PlayerController>();
            playerAnimator = p.GetComponent<Animator>();
            playerRb = p.GetComponent<Rigidbody2D>();
        }

        int level = GetEffectiveLevel();
        switch (level)
        {
            case 1:
                Level1_Start();
                break;
            case 3:
                Level3_Start();
                break;
            case 5:
                Level5_Start();
                break;
        }

        if (level == 3 || level == 5)
            teleportCountdown = Random.Range(teleportMinInterval, teleportMaxInterval);
    }

    void Update()
    {
        if (enemyHealth == null) return;

        if (enemyHealth.isDead)
        {
            enabled = false;
            return;
        }

        if (player == null) return;

        distanceToPlayer = Vector2.Distance(transform.position, player.position);
        canSee = CanSeePlayer();

        int level = GetEffectiveLevel();

        switch (level)
        {
            case 1:
                Level1_Update();
                break;
            case 3:
                Level3_Update();
                break;
            case 5:
                Level5_UpdateRageTint();
                Level5_UpdateBrain();
                break;
        }

        if (level == 3)
            Level3_UpdateTeleportEdgeFlag();
        if (level == 5)
        {
            wasHurtLastFrameRage = enemyHealth.isHurt;
            if (level5BombCooldown > 0f)
                level5BombCooldown -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (enemyHealth == null || enemyHealth.isDead || player == null) return;

        int level = GetEffectiveLevel();

        switch (level)
        {
            case 3:
                Level3_FixedUpdate();
                break;
            case 5:
                Level5_FixedUpdate();
                break;
        }
    }

    #endregion

    #region Level routing

    /// <summary>Maps arbitrary inspector values to the three supported tiers.</summary>
    int GetEffectiveLevel()
    {
        if (stalkerLevel >= 5) return 5;
        if (stalkerLevel >= 3) return 3;
        return 1;
    }

    #endregion

    #region Level 1 — Presence

    /// <summary>
    /// Level 1 — "Presence": kinematic rigidbody, all trigger colliders disabled so <see cref="EnemyHealth.TakeDamage"/>
    /// cannot fire, while non-trigger geometry should block the player. Distance drives <c>_TintAmount</c> only.
    /// </summary>
    void Level1_Start()
    {
        if (rb != null)
            rb.bodyType = RigidbodyType2D.Kinematic;

        if (!level1DamageCollidersDisabled)
        {
            foreach (Collider2D col in GetComponentsInChildren<Collider2D>(true))
            {
                if (col != null && col.isTrigger)
                    col.enabled = false;
            }
            level1DamageCollidersDisabled = true;
        }

        if (animator != null)
            animator.SetBool(AnimIsWalking, false);
    }

    void Level1_Update()
    {
        UpdateTintAmount(Mathf.Clamp01(Mathf.InverseLerp(visibilityFadeEnd, visibilityFadeStart, distanceToPlayer)));
    }

    #endregion

    #region Level 3 — Hunter

    /// <summary>Level 3 — "Hunter": static 50% tint, teleport loop with dissolve, melee vs bow by distance.</summary>
    void Level3_Start()
    {
        if (rb != null && rb.bodyType == RigidbodyType2D.Kinematic)
            rb.bodyType = RigidbodyType2D.Dynamic;

        UpdateTintAmount(0.5f);
    }

    void Level3_Update()
    {
        if (isTeleporting) return;

        bool hurtEdgeTeleport = enemyHealth.isHurt && !wasHurtLastFrameTeleport;

        if (canSee)
        {
            if (!enemyHealth.isHurt && teleportCountdown > 0f)
                teleportCountdown -= Time.deltaTime;

            bool tooClose = distanceToPlayer < TeleportProximity;
            bool timerDone = teleportCountdown <= 0f;

            if (timerDone || hurtEdgeTeleport || tooClose)
            {
                if (TryPickTeleportNode(out Transform node))
                    StartCoroutine(TeleportDissolveRoutine(node));
                teleportCountdown = Random.Range(teleportMinInterval, teleportMaxInterval);
            }
        }

        if (!isAttacking && !enemyHealth.isHurt && canSee && !isTeleporting)
        {
            FacePlayerHorizontal();

            if (distanceToPlayer < meleeThreshold)
                StartCoroutine(SwordAttack());
            else
                StartCoroutine(BowAttack(bowWindupTime));
        }
    }

    void Level3_UpdateTeleportEdgeFlag()
    {
        wasHurtLastFrameTeleport = enemyHealth.isHurt;
    }

    void Level3_FixedUpdate()
    {
        if (enemyHealth.isHurt) return;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    IEnumerator TeleportDissolveRoutine(Transform node)
    {
        isTeleporting = true;
        float step = Mathf.Max(0.01f, dissolveTime);

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        yield return new WaitForSeconds(step);
        if (enemyHealth.isDead)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = true;
            isTeleporting = false;
            yield break;
        }

        transform.position = node.position;

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
        yield return new WaitForSeconds(step);
        isTeleporting = false;
    }

    bool TryPickTeleportNode(out Transform picked)
    {
        picked = null;
        if (teleportNodes == null || teleportNodes.Length == 0) return false;

        var candidates = new List<Transform>();
        foreach (Transform t in teleportNodes)
        {
            if (t == null) continue;
            if (NodeCanSeePlayer(t))
                candidates.Add(t);
        }

        if (candidates.Count == 0) return false;
        picked = candidates[Random.Range(0, candidates.Count)];
        return true;
    }

    bool NodeCanSeePlayer(Transform originNode)
    {
        if (player == null) return false;
        if (Mathf.Abs(originNode.position.y - player.position.y) > yTolerance) return false;

        Vector2 origin = new Vector2(originNode.position.x, originNode.position.y + 1f);
        Vector2 target = new Vector2(player.position.x, player.position.y + 1f);
        Vector2 direction = (target - origin).normalized;
        float dist = Vector2.Distance(origin, target);
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, dist, sightBlockLayers);
        return hit.collider == null;
    }

    #endregion

    #region Level 5 — Duel (rage, matrix, movement helpers)

    /// <summary>
    /// Level 5 — "Duel": light tint, faster animator, rage after repeated hurts, dash/strafe/bomb matrix,
    /// knockback cleared each hurt <see cref="FixedUpdate"/>, and predictive dodges vs the player's sword.
    /// </summary>
    void Level5_Start()
    {
        if (rb != null)
            rb.bodyType = RigidbodyType2D.Dynamic;

        UpdateTintAmount(0.07f);

        if (animator != null)
            animator.speed = animSpeedMultiplier;

        baseDashSpeed = dashSpeed;
        baseAttackCooldown = attackCooldown;
        orbitFlipTimer = 3f;
    }

    void Level5_UpdateRageTint()
    {
        if (enemyHealth.isHurt && !wasHurtLastFrameRage)
        {
            hitsTaken++;
            if (hitsTaken >= rageHitThreshold)
            {
                isRaging = true;
                UpdateTintAmount(0f);
            }
        }

        dashSpeed = isRaging ? baseDashSpeed * 1.3f : baseDashSpeed;
        attackCooldown = isRaging ? baseAttackCooldown * 0.7f : baseAttackCooldown;
    }

    void Level5_UpdateBrain()
    {
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        orbitFlipTimer -= Time.deltaTime;
        if (orbitFlipTimer <= 0f)
        {
            strafeOrbitSign *= -1f;
            orbitFlipTimer = Random.Range(2f, 5f);
        }

        bool playerAttacking = IsPlayerAttacking();
        bool playerIdle = IsPlayerDefensiveIdle();

        if (!isAttacking && !enemyHealth.isHurt)
        {
            if (IsPlayerBehindStalker())
            {
                kiteBowActive = false;
                pendingSwordAfterDash = false;
                TryBeginDashTowardPlayer();
            }
            else if (distanceToPlayer < attackRange && playerAttacking && canSee)
            {
                kiteBowActive = true;
                StartCoroutine(BowAttack(bowWindupTime * 0.8f));
            }
            else if (distanceToPlayer > sightRange * 0.6f && playerIdle && canSee)
            {
                kiteBowActive = false;
                if (TryBeginDashTowardPlayer())
                    pendingSwordAfterDash = true;
            }
            else if (distanceToPlayer >= 4f && distanceToPlayer <= 6f && !playerAttacking && !playerIdle && canSee && bombPrefab != null && level5BombCooldown <= 0f)
            {
                kiteBowActive = false;
                level5BombCooldown = Level5BombRepeatInterval;
                StartCoroutine(BombAttack());
            }
        }

        if (kiteBowActive && (distanceToPlayer > attackRange + 0.75f || !playerAttacking))
            kiteBowActive = false;

        if (!enemyHealth.isHurt && canSee && !isAttacking)
            FacePlayerHorizontal();
    }

    void Level5_FixedUpdate()
    {
        if (enemyHealth.isHurt)
        {
            isDashing = false;
            dashBurstTimer = 0f;
            rb.linearVelocity = Vector2.zero;
            predictiveDodgeTimer = 0f;
            return;
        }

        float grapple = Mathf.Max(0.01f, grappleSlowMultiplier);

        if (predictiveDodgeTimer > 0f)
        {
            rb.linearVelocity = new Vector2(predictiveDodgeVx * grapple, rb.linearVelocity.y);
            predictiveDodgeTimer -= Time.fixedDeltaTime;
            return;
        }

        if (IsPlayerMeleeHitboxActive())
        {
            float dodgeDir = playerController != null && playerController.facingRight ? -1f : 1f;
            predictiveDodgeVx = dodgeDir * dashSpeed * 0.6f * grapple;
            predictiveDodgeTimer = 0.2f;
            rb.linearVelocity = new Vector2(predictiveDodgeVx, rb.linearVelocity.y);
            return;
        }

        if (isDashing)
        {
            dashBurstTimer -= Time.fixedDeltaTime;
            if (dashBurstTimer <= 0f)
            {
                isDashing = false;
                rb.linearVelocity = Vector2.zero;
                dashCooldownTimer = dashCooldown / grapple;

                if (pendingSwordAfterDash)
                {
                    pendingSwordAfterDash = false;
                    if (!enemyHealth.isDead)
                        StartCoroutine(SwordAttack());
                }
            }
            else
                rb.linearVelocity = dashDirection * dashSpeed * grapple;
            return;
        }

        if (kiteBowActive)
        {
            float away = Mathf.Sign(transform.position.x - player.position.x);
            if (Mathf.Abs(away) < 0.01f)
                away = facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(away * StrafeWalkSpeed * 0.8f * grapple, rb.linearVelocity.y);
            return;
        }

        if (!isAttacking)
        {
            Vector2 toPlayer = ((Vector2)(player.position - transform.position));
            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                toPlayer.Normalize();
                Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x);
                float lateral = tangent.x;
                if (Mathf.Abs(lateral) < 0.1f)
                    lateral = strafeOrbitSign;
                else
                    lateral = Mathf.Sign(lateral) * strafeOrbitSign;

                rb.linearVelocity = new Vector2(lateral * StrafeWalkSpeed * 0.8f * grapple, rb.linearVelocity.y);
            }
            else
                rb.linearVelocity = Vector2.zero;
        }
        else
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    bool TryBeginDashTowardPlayer()
    {
        if (dashCooldownTimer > 0f || isDashing) return false;

        Vector2 to = (Vector2)(player.position - transform.position);
        if (to.sqrMagnitude < 0.0001f) return false;

        dashDirection = new Vector2(Mathf.Sign(to.x), 0f).normalized;
        if (dashDirection.sqrMagnitude < 0.01f)
            dashDirection = facingRight ? Vector2.right : Vector2.left;

        isDashing = true;
        float g = Mathf.Max(0.01f, grappleSlowMultiplier);
        dashBurstTimer = DashBurstDuration / g;
        return true;
    }

    bool IsPlayerBehindStalker()
    {
        if (player == null) return false;
        if (facingRight)
            return player.position.x < transform.position.x - 0.05f;
        return player.position.x > transform.position.x + 0.05f;
    }

    bool IsPlayerDefensiveIdle()
    {
        if (playerRb == null) return false;
        return Mathf.Abs(playerRb.linearVelocity.x) < 0.25f && !IsPlayerAttacking();
    }

    bool IsPlayerAttacking()
    {
        if (IsPlayerMeleeHitboxActive()) return true;
        if (playerAnimator == null) return false;

        if (PlayerAnimatorHasBool("IsSlashing") && playerAnimator.GetBool("IsSlashing"))
            return true;

        AnimatorStateInfo st = playerAnimator.GetCurrentAnimatorStateInfo(0);
        return PlayerStateNameLooksLikeAttack(st) && st.normalizedTime < 0.99f;
    }

    static bool PlayerStateNameLooksLikeAttack(AnimatorStateInfo st)
    {
        // Triggers like "AttackRight" are not readable; approximate via common state names.
        return st.IsName("Slash") || st.IsName("Attack")
            || st.IsName("Player_Attack") || st.IsName("SwordSlash");
    }

    bool IsPlayerMeleeHitboxActive()
    {
        if (playerController == null) return false;

        if (playerController.slashHitbox != null && playerController.slashHitbox.activeInHierarchy)
        {
            Collider2D c = playerController.slashHitbox.GetComponent<Collider2D>();
            if (c != null && c.enabled) return true;
        }

        if (playerController.downAttackHitbox != null && playerController.downAttackHitbox.activeInHierarchy)
        {
            Collider2D c2 = playerController.downAttackHitbox.GetComponent<Collider2D>();
            if (c2 != null && c2.enabled) return true;
        }

        return false;
    }

    bool PlayerAnimatorHasBool(string name)
    {
        if (playerAnimator == null) return false;
        foreach (AnimatorControllerParameter p in playerAnimator.parameters)
        {
            if (p.name == name && p.type == AnimatorControllerParameterType.Bool)
                return true;
        }
        return false;
    }

    #endregion

    #region Combat coroutines (all levels that fight)

    /// <summary>
    /// Melee coroutine: <see cref="hitboxDelay"/> → <c>AttackRight</c> trigger + SFX → align <see cref="swordHitbox"/> →
    /// enable collider for <see cref="hitboxDuration"/> / grapple → cooldown / grapple (same flow as <see cref="EnemyAI"/>).
    /// </summary>
    IEnumerator SwordAttack()
    {
        if (swordHitbox == null) yield break;

        isAttacking = true;
        yield return null;

        if (enemyHealth.isHurt || enemyHealth.isDead)
        {
            isAttacking = false;
            yield break;
        }

        if (animator != null)
            animator.SetTrigger("AttackRight");
        SoundManager.instance.PlayWorldRandom(EnemyAudio.instance.universal.attackSounds, transform, 1f);
        yield return new WaitForSeconds(hitboxDelay);

        if (enemyHealth.isHurt || enemyHealth.isDead)
        {
            Collider2D c0 = swordHitbox.GetComponent<Collider2D>();
            if (c0 != null) c0.enabled = false;
            isAttacking = false;
            yield break;
        }

        Vector3 hitboxPos = swordHitbox.transform.localPosition;
        hitboxPos.x = facingRight ? Mathf.Abs(hitboxPos.x) : -Mathf.Abs(hitboxPos.x);
        swordHitbox.transform.localPosition = hitboxPos;

        Collider2D c = swordHitbox.GetComponent<Collider2D>();
        if (c != null) c.enabled = true;

        yield return new WaitForSeconds(hitboxDuration / Mathf.Max(0.01f, grappleSlowMultiplier));

        if (c != null) c.enabled = false;

        yield return new WaitForSeconds(attackCooldown / Mathf.Max(0.01f, grappleSlowMultiplier));

        isAttacking = false;
    }

    /// <summary>Ranged: bow trigger, windup, spawn enemy arrow toward facing.</summary>
    IEnumerator BowAttack(float windupSeconds)
    {
        isAttacking = true;
        yield return null;

        if (enemyHealth.isHurt || enemyHealth.isDead)
        {
            isAttacking = false;
            yield break;
        }

        if (animator != null)
        {
            if (facingRight)
                animator.SetTrigger("BowAttackRight");
            else
                animator.SetTrigger("BowAttackLeft");
        }

        yield return new WaitForSeconds(windupSeconds);

        if (enemyHealth.isHurt || enemyHealth.isDead)
        {
            isAttacking = false;
            yield break;
        }

        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            GameObject arrowObj = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
            Arrow arrow = arrowObj.GetComponent<Arrow>();
            if (arrow != null)
            {
                arrow.isPlayerArrow = false;
                arrow.SetDirection(facingRight);
            }
        }

        yield return new WaitForSeconds(attackCooldown / Mathf.Max(0.01f, grappleSlowMultiplier));

        isAttacking = false;
    }

    /// <summary>Places a bomb behind the player; fuse and radius pushed to <see cref="Bomb"/> when present.</summary>
    IEnumerator BombAttack()
    {
        isAttacking = true;
        yield return null;

        if (enemyHealth.isHurt || enemyHealth.isDead)
        {
            isAttacking = false;
            yield break;
        }

        if (bombPrefab != null && player != null)
        {
            Vector2 behind = playerController != null && playerController.facingRight ? Vector2.left : Vector2.right;
            Vector3 spawn = player.position + (Vector3)(behind * 1.75f);
            GameObject b = Instantiate(bombPrefab, spawn, Quaternion.identity);
            Bomb bomb = b.GetComponent<Bomb>();
            if (bomb != null)
            {
                bomb.fuseTime = bombFuse;
                bomb.explosionRadius = bombExplosionRadius;
            }
        }

        yield return new WaitForSeconds(0.35f / Mathf.Max(0.01f, grappleSlowMultiplier));

        isAttacking = false;
    }

    #endregion

    #region Shared helpers

    /// <summary>Writes <c>_TintAmount</c> when the material supports it (silhouette / rage visibility).</summary>
    void UpdateTintAmount(float amount)
    {
        if (spriteRenderer == null || spriteRenderer.material == null) return;
        if (spriteRenderer.material.HasProperty(TintId))
            spriteRenderer.material.SetFloat(TintId, amount);
    }

    /// <summary>Line-of-sight check to the player using <see cref="yTolerance"/> and <see cref="sightBlockLayers"/> (same idea as <see cref="EnemyAI"/>).</summary>
    bool CanSeePlayer()
    {
        if (player == null) return false;
        if (Mathf.Abs(transform.position.y - player.position.y) > yTolerance) return false;

        Vector2 origin = new Vector2(transform.position.x, transform.position.y + 1f);
        Vector2 target = new Vector2(player.position.x, player.position.y + 1f);
        Vector2 direction = (target - origin).normalized;
        float distance = Vector2.Distance(origin, target);
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, sightBlockLayers);
        return hit.collider == null;
    }

    /// <summary>Flip local X scale (same pattern as <see cref="EnemyAI"/>).</summary>
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    void FacePlayerHorizontal()
    {
        if (player == null) return;
        if (player.position.x < transform.position.x && facingRight)
            Flip();
        else if (player.position.x > transform.position.x && !facingRight)
            Flip();
    }

    #endregion
}
