using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sole AI controller for "The Stalker" — a gutted player prefab with <see cref="EnemyHealth"/>.
/// Drives movement, visibility tint, teleport combat, and duel-phase behavior by <see cref="stalkerLevel"/> (1, 3, or 5).
/// Does not inherit from <see cref="EnemyAI"/>; mirrors its combat/movement conventions only.
/// </summary>
/// <remarks>
/// Keep stalker-specific logic and tuning in this script only. Do not modify shared gameplay scripts
/// (for example <c>SlashHitbox</c>, <c>Bomb</c>, <c>Enemy_AI</c>) to adjust The Stalker — change this file instead.
/// </remarks>
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
    public float visibilityFadeStart = 3f;
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

    [Header("Level 5 — Bomb avoidance")]
    [Tooltip("How far out we scan for active Bomb components.")]
    public float bombThreatSenseRadius = 6.5f;
    [Tooltip("When a bomb’s fuse has at most this many seconds left, try to run away.")]
    public float bombFleeIfFuseRemainingBelow = 1.35f;
    [Tooltip("Horizontal flee speed multiplier while escaping an imminent blast.")]
    public float bombFleeSpeedScale = 1.45f;

    [Header("Level 5 — Movement pressure")]
    [Tooltip("Blends toward the player like basic EnemyAI — reduces pure orbit kiting.")]
    [Range(0f, 1f)]
    public float approachBlendVsStrafe = 0.28f;
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

    [Header("Animation (match PlayerController)")]
    public RuntimeAnimatorController meleeAnimatorController;
    public RuntimeAnimatorController rangedAnimatorController;

    #endregion

    #region Constants & cached components

    const float DashBurstDuration = 0.15f;
    const float TeleportProximity = 1.5f;
    const float StrafeWalkSpeed = 2f;
    static readonly int TintId = Shader.PropertyToID("_TintAmount");

    static readonly int AnimIsWalking  = Animator.StringToHash("IsWalking");
    static readonly int AnimIsRunning  = Animator.StringToHash("Is_running");

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

    // FIX: track whether we created the hitbox at runtime so we don't double-process it
    bool dynamicHitboxCreated;

    public StalkerAudio audioData;

    #endregion

    #region Level 1 state
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
    float strafeOrbitSign = 1f;
    float orbitFlipTimer;
    bool pendingSwordAfterDash;
    float level5BombCooldown;
    #endregion

    #region Core lifecycle

    void Awake()
    {
        animator    = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        rb          = GetComponent<Rigidbody2D>();
        audioData = GetComponent<StalkerAudio>();
        baseDashSpeed       = dashSpeed;
        baseAttackCooldown  = attackCooldown;

        AutoAssignMissingHierarchyReferences();
        SyncFacingBoolFromScale();
    }

    void SyncFacingBoolFromScale()
    {
        facingRight = transform.localScale.x >= 0f;
    }

    // -------------------------------------------------------------------------
    // FIX: expanded to search multiple name variants and fall back to component
    //      type detection, then finally creates a hitbox dynamically.
    // -------------------------------------------------------------------------
    void AutoAssignMissingHierarchyReferences()
    {
        // ── SpriteRenderer ──────────────────────────────────────────────────
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        // ── Sword hitbox ─────────────────────────────────────────────────────
        if (swordHitbox == null)
        {
            // Try common name variants first
            string[] hitboxNames = { "SlashHitbox", "slashHitbox", "Slash_Hitbox",
                                     "SwordHitbox",  "sword_hitbox", "HitBox",
                                     "MeleeHitbox",  "AttackHitbox" };
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                foreach (string n in hitboxNames)
                {
                    if (t.name.Equals(n, System.StringComparison.OrdinalIgnoreCase))
                    {
                        swordHitbox = t.gameObject;
                        break;
                    }
                }
                if (swordHitbox != null) break;
            }
        }

        // Fallback: find by SlashHitbox component on any child
        if (swordHitbox == null)
        {
            SlashHitbox sh = GetComponentInChildren<SlashHitbox>(true);
            if (sh != null) swordHitbox = sh.gameObject;
        }

        // Last resort: create a runtime hitbox so attacks never silently fail
        if (swordHitbox == null && !dynamicHitboxCreated)
        {
            dynamicHitboxCreated = true;
            swordHitbox = new GameObject("StalkerSwordHitbox_Runtime");
            swordHitbox.transform.SetParent(transform);
            swordHitbox.transform.localPosition = new Vector3(0.8f, 0f, 0f);
            BoxCollider2D col = swordHitbox.AddComponent<BoxCollider2D>();
            col.size    = new Vector2(1.2f, 1.0f);
            col.isTrigger = true;
            col.enabled   = false;          // starts disabled; SwordAttack enables it
            EnemyHitbox eh = swordHitbox.AddComponent<EnemyHitbox>();
            eh.damage = 1;
            Debug.LogWarning("[StalkerAI] No sword hitbox found in hierarchy — created a runtime one. " +
                             "Assign 'SlashHitbox' (or equivalent child) in the Prefab for reliable results.", this);
        }

        // ── Arrow spawn point ─────────────────────────────────────────────────
        if (arrowSpawnPoint == null)
        {
            string[] spawnNames = { "ArrowSpawnPoint", "arrowSpawnPoint",
                                    "Arrow_Spawn",      "BowSpawnPoint",
                                    "ProjectileSpawn" };
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                foreach (string n in spawnNames)
                {
                    if (t.name.Equals(n, System.StringComparison.OrdinalIgnoreCase))
                    {
                        arrowSpawnPoint = t;
                        break;
                    }
                }
                if (arrowSpawnPoint != null) break;
            }

            // Fallback: create a spawn point slightly in front of the stalker
            if (arrowSpawnPoint == null)
            {
                GameObject sp = new GameObject("ArrowSpawnPoint_Runtime");
                sp.transform.SetParent(transform);
                sp.transform.localPosition = new Vector3(0.5f, 0.25f, 0f);
                arrowSpawnPoint = sp.transform;
                Debug.LogWarning("[StalkerAI] No ArrowSpawnPoint found — created runtime fallback.", this);
            }
        }
    }

    // -------------------------------------------------------------------------
    // FIX: always overwrite animators from the player when the player is found,
    //      not only when the stalker field is null.  The prefab ships with them
    //      as None, so we must always copy on first run.
    // -------------------------------------------------------------------------
    void TryCopyAnimatorAndWeaponsFromPlayer(PlayerController pc)
    {
        if (pc == null) return;

        // Always copy animators — if the field was None we fill it; if somehow
        // set already we still prefer the live player's assets for consistency.
        if (pc.swordAnimator != null)
            meleeAnimatorController = pc.swordAnimator;
        if (pc.bowAnimator != null)
            rangedAnimatorController = pc.bowAnimator;

        if (arrowPrefab == null && pc.playerArrowPrefab != null)
            arrowPrefab = pc.playerArrowPrefab;

        // Bomb prefab: try BombAttack component on the Stalker itself
        if (bombPrefab == null)
        {
            BombAttack bombAttack = GetComponent<BombAttack>();
            if (bombAttack != null && bombAttack.bombPrefab != null)
                bombPrefab = bombAttack.bombPrefab;
        }
    }

    void ApplyMeleeAnimatorController()
    {
        if (animator != null && meleeAnimatorController != null)
        {
            animator.runtimeAnimatorController = meleeAnimatorController;
            Debug.Log("[StalkerAI] Applied melee animator: " + meleeAnimatorController.name, this);
        }
        else if (animator != null && meleeAnimatorController == null)
        {
            Debug.LogWarning("[StalkerAI] meleeAnimatorController is still null after player copy. " +
                             "Assign it in the Inspector or ensure PlayerController.swordAnimator is set.", this);
        }
    }

    void Start()
    {
        AutoAssignMissingHierarchyReferences();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player           = p.transform;
            playerController = p.GetComponent<PlayerController>();
            playerAnimator   = p.GetComponent<Animator>();
            playerRb         = p.GetComponent<Rigidbody2D>();
            TryCopyAnimatorAndWeaponsFromPlayer(playerController);
        }
        else
        {
            Debug.LogWarning("[StalkerAI] No GameObject tagged 'Player' found in scene.", this);
        }

        // Apply melee animator AFTER copying from the player
        ApplyMeleeAnimatorController();

        if (sightBlockLayers.value == 0)
            sightBlockLayers = -1;

        int level = GetEffectiveLevel();
        switch (level)
        {
            case 1: Level1_Start(); break;
            case 3: Level3_Start(); break;
            case 5: Level5_Start(); break;
        }

        if (level == 3)
            teleportCountdown = Random.Range(teleportMinInterval, teleportMaxInterval);

        EnsureSwordDamagesPlayer();
    }

    void Update()
    {
        if (enemyHealth == null) return;
        if (enemyHealth.isDead) { enabled = false; return; }
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
                Level5_UpdateLocomotionAnimator();
                break;
        }

        if (level == 3) Level3_UpdateTeleportEdgeFlag();
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
            case 3: Level3_FixedUpdate(); break;
            case 5: Level5_FixedUpdate(); break;
        }
    }

    #endregion

    #region Level routing
    int GetEffectiveLevel()
    {
        if (stalkerLevel >= 5) return 5;
        if (stalkerLevel >= 3) return 3;
        return 1;
    }
    #endregion

    #region Level 1 — Presence

    void Level1_Start()
    {
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        if (!level1DamageCollidersDisabled)
        {
            foreach (Collider2D col in GetComponentsInChildren<Collider2D>(true))
                if (col != null && col.isTrigger) col.enabled = false;
            level1DamageCollidersDisabled = true;
        }

        if (animator != null) animator.SetBool(AnimIsWalking, false);
    }

    void Level1_Update()
    {
        UpdateTintAmount(Mathf.Clamp01(Mathf.InverseLerp(visibilityFadeEnd, visibilityFadeStart, distanceToPlayer)));
    }

    #endregion

    #region Level 3 — Hunter

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

            bool tooClose  = distanceToPlayer < TeleportProximity;
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
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    IEnumerator TeleportDissolveRoutine(Transform node)
    {
        isTeleporting = true;
        float step = Mathf.Max(0.01f, dissolveTime);

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        yield return new WaitForSeconds(step);

        if (enemyHealth.isDead)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            isTeleporting = false;
            yield break;
        }

        transform.position = node.position;

        if (spriteRenderer != null) spriteRenderer.enabled = true;
        yield return new WaitForSeconds(step);
        isTeleporting = false;
    }

    bool TryPickTeleportNode(out Transform picked)
    {
        picked = null;
        if (teleportNodes == null || teleportNodes.Length == 0) return false;

        var candidates = new List<Transform>();
        foreach (Transform t in teleportNodes)
            if (t != null && NodeCanSeePlayer(t)) candidates.Add(t);

        if (candidates.Count == 0) return false;
        picked = candidates[Random.Range(0, candidates.Count)];
        return true;
    }

    bool NodeCanSeePlayer(Transform originNode)
    {
        if (player == null) return false;
        if (Mathf.Abs(originNode.position.y - player.position.y) > yTolerance) return false;

        Vector2 origin    = new Vector2(originNode.position.x, originNode.position.y + 1f);
        Vector2 target    = new Vector2(player.position.x,     player.position.y     + 1f);
        Vector2 direction = (target - origin).normalized;
        float dist        = Vector2.Distance(origin, target);
        RaycastHit2D hit  = Physics2D.Raycast(origin, direction, dist, sightBlockLayers);
        return hit.collider == null;
    }

    #endregion

    #region Level 5 — Duel

    void Level5_Start()
    {
        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;
        UpdateTintAmount(0.07f);
        if (animator != null) animator.speed = animSpeedMultiplier;
        baseDashSpeed       = dashSpeed;
        baseAttackCooldown  = attackCooldown;
        orbitFlipTimer      = 3f;
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

        dashSpeed       = isRaging ? baseDashSpeed      * 1.3f : baseDashSpeed;
        attackCooldown  = isRaging ? baseAttackCooldown * 0.7f : baseAttackCooldown;
    }

    void Level5_UpdateBrain()
    {
        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;

        orbitFlipTimer -= Time.deltaTime;
        if (orbitFlipTimer <= 0f)
        {
            strafeOrbitSign *= -1f;
            orbitFlipTimer = Random.Range(2f, 5f);
        }

        if (!enemyHealth.isHurt && canSee && !isAttacking)
            FacePlayerHorizontal();

        if (!isAttacking && !enemyHealth.isHurt)
        {
            float meleeReach = Mathf.Max(attackRange * 1.35f, 2.75f);

            if (canSee && distanceToPlayer <= meleeReach)
            {
                pendingSwordAfterDash = false;
                StartCoroutine(SwordAttack());
            }
            else if (IsPlayerBehindStalker())
            {
                pendingSwordAfterDash = false;
                TryBeginDashTowardPlayer();
            }
            else if (canSee && distanceToPlayer > meleeReach && distanceToPlayer <= sightRange)
            {
                bool rollBomb = bombPrefab != null && level5BombCooldown <= 0f
                    && distanceToPlayer >= 2.5f && distanceToPlayer <= 9f
                    && Random.value < 0.38f;
                if (rollBomb)
                {
                    level5BombCooldown = Level5BombRepeatInterval;
                    StartCoroutine(BombAttack());
                }
                else
                    StartCoroutine(BowAttack(bowWindupTime * 0.85f));
            }
            else if (distanceToPlayer > sightRange * 0.6f && IsPlayerDefensiveIdle() && canSee)
            {
                if (TryBeginDashTowardPlayer())
                    pendingSwordAfterDash = true;
            }
        }
    }

    void Level5_UpdateLocomotionAnimator()
    {
        if (animator == null) return;

        if (enemyHealth.isHurt || isAttacking)
        {
            animator.SetBool(AnimIsRunning, false);
            animator.SetBool(AnimIsWalking, false);
            return;
        }

        if (rb == null) return;
        bool moving = Mathf.Abs(rb.linearVelocity.x) > 0.08f;
        animator.SetBool(AnimIsRunning, moving);
        animator.SetBool(AnimIsWalking, moving);
    }

    // -------------------------------------------------------------------------
    // FIX: more robust — if swordHitbox already has an EnemyHitbox and no
    //      SlashHitbox remains, skip; otherwise do the swap correctly.
    //      Also ignore the dynamically created hitbox (it was built right).
    // -------------------------------------------------------------------------
    void EnsureSwordDamagesPlayer()
    {
        if (swordHitbox == null) return;

        // Dynamic hitbox was already built with EnemyHitbox — nothing to do.
        if (dynamicHitboxCreated) return;

        // Already correctly configured
        if (swordHitbox.GetComponent<EnemyHitbox>() != null &&
            swordHitbox.GetComponent<SlashHitbox>()  == null)
            return;

        // Swap SlashHitbox → EnemyHitbox
        SlashHitbox slash = swordHitbox.GetComponent<SlashHitbox>();
        int dmg = 1;
        if (slash != null)
        {
            dmg = slash.damage > 0 ? slash.damage : 1;
            Destroy(slash);
        }

        EnemyHitbox eh = swordHitbox.GetComponent<EnemyHitbox>();
        if (eh == null) eh = swordHitbox.AddComponent<EnemyHitbox>();
        eh.damage = dmg;

        // Make sure the sword collider doesn't clip with the stalker's own body
        Collider2D swordCol = swordHitbox.GetComponent<Collider2D>();
        if (swordCol != null)
        {
            foreach (Collider2D bodyCol in GetComponentsInChildren<Collider2D>(true))
            {
                if (bodyCol == null || bodyCol == swordCol) continue;
                Physics2D.IgnoreCollision(swordCol, bodyCol, true);
            }
        }

        // Ensure it's a trigger so it doesn't push the player around
        if (swordCol != null) swordCol.isTrigger = true;

        Debug.Log("[StalkerAI] Sword hitbox configured: SlashHitbox → EnemyHitbox (damage=" + dmg + ")", this);
    }

    void Level5_FixedUpdate()
    {
        if (enemyHealth.isHurt)
        {
            isDashing           = false;
            dashBurstTimer      = 0f;
            rb.linearVelocity   = Vector2.zero;
            predictiveDodgeTimer = 0f;
            return;
        }

        float grapple = Mathf.Max(0.01f, grappleSlowMultiplier);

        if (TryGetBombFleeVelocity(grapple, out Vector2 bombFlee))
        {
            rb.linearVelocity = new Vector2(bombFlee.x, rb.linearVelocity.y);
            return;
        }

        if (predictiveDodgeTimer > 0f)
        {
            rb.linearVelocity    = new Vector2(predictiveDodgeVx * grapple, rb.linearVelocity.y);
            predictiveDodgeTimer -= Time.fixedDeltaTime;
            return;
        }

        if (IsPlayerMeleeHitboxActive())
        {
            float dodgeDir   = playerController != null && playerController.facingRight ? -1f : 1f;
            predictiveDodgeVx    = dodgeDir * dashSpeed * 0.6f * grapple;
            predictiveDodgeTimer = 0.2f;
            rb.linearVelocity    = new Vector2(predictiveDodgeVx, rb.linearVelocity.y);
            return;
        }

        if (isDashing)
        {
            dashBurstTimer -= Time.fixedDeltaTime;
            if (dashBurstTimer <= 0f)
            {
                isDashing           = false;
                rb.linearVelocity   = Vector2.zero;
                dashCooldownTimer   = dashCooldown / grapple;

                if (pendingSwordAfterDash)
                {
                    pendingSwordAfterDash = false;
                    if (!enemyHealth.isDead) StartCoroutine(SwordAttack());
                }
            }
            else
                rb.linearVelocity = dashDirection * dashSpeed * grapple;
            return;
        }

        if (!isAttacking)
        {
            Vector2 toPlayer = (Vector2)(player.position - transform.position);
            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                toPlayer.Normalize();
                Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x);
                float lateral = tangent.x;
                if (Mathf.Abs(lateral) < 0.1f)
                    lateral = strafeOrbitSign;
                else
                    lateral = Mathf.Sign(lateral) * strafeOrbitSign;

                float strafeX = lateral * StrafeWalkSpeed * 0.8f * grapple;
                float towardX = Mathf.Sign(player.position.x - transform.position.x) * StrafeWalkSpeed * 1.05f * grapple;
                float w = Mathf.Clamp01((distanceToPlayer - 2.5f) / 6f) * approachBlendVsStrafe;
                float vx = Mathf.Lerp(strafeX, towardX, w);
                rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
            }
            else
                rb.linearVelocity = Vector2.zero;
        }
        else
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    bool TryGetBombFleeVelocity(float grapple, out Vector2 horizontalVel)
    {
        horizontalVel = Vector2.zero;
        if (rb == null) return false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, bombThreatSenseRadius);
        float bestUrgency = 0f;
        Vector2 bestDir = Vector2.zero;

        foreach (Collider2D h in hits)
        {
            if (h == null) continue;
            Bomb b = h.GetComponent<Bomb>();
            if (b == null) b = h.GetComponentInParent<Bomb>();
            if (b == null) continue;

            float rem = b.fuseTime;
            if (rem <= 0.01f || rem > bombFleeIfFuseRemainingBelow) continue;

            Vector2 fromBomb = (Vector2)transform.position - (Vector2)b.transform.position;
            float dist = fromBomb.magnitude;
            if (dist < 0.01f) continue;
            fromBomb /= dist;
            float blast = b.explosionRadius + 0.6f;
            float urgency = (blast / Mathf.Max(0.35f, dist)) * (1f / Mathf.Max(0.15f, rem));
            if (urgency > bestUrgency)
            {
                bestUrgency = urgency;
                bestDir = fromBomb;
            }
        }

        if (bestUrgency < 0.01f) return false;
        horizontalVel = new Vector2(bestDir.x * StrafeWalkSpeed * bombFleeSpeedScale * grapple, 0f);
        return true;
    }

    bool TryBeginDashTowardPlayer()
    {
        if (dashCooldownTimer > 0f || isDashing) return false;

        Vector2 to = (Vector2)(player.position - transform.position);
        if (to.sqrMagnitude < 0.0001f) return false;

        dashDirection = new Vector2(Mathf.Sign(to.x), 0f);
        if (dashDirection.sqrMagnitude < 0.01f)
            dashDirection = facingRight ? Vector2.right : Vector2.left;

        isDashing         = true;
        float g           = Mathf.Max(0.01f, grappleSlowMultiplier);
        dashBurstTimer    = DashBurstDuration / g;
        return true;
    }

    bool IsPlayerBehindStalker()
    {
        if (player == null) return false;
        return facingRight
            ? player.position.x < transform.position.x - 0.05f
            : player.position.x > transform.position.x + 0.05f;
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
        if (PlayerAnimatorHasBool("IsSlashing") && playerAnimator.GetBool("IsSlashing")) return true;
        AnimatorStateInfo st = playerAnimator.GetCurrentAnimatorStateInfo(0);
        return PlayerStateNameLooksLikeAttack(st) && st.normalizedTime < 0.99f;
    }

    static bool PlayerStateNameLooksLikeAttack(AnimatorStateInfo st)
    {
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
            if (p.name == name && p.type == AnimatorControllerParameterType.Bool) return true;
        return false;
    }

    #endregion

    #region Combat coroutines (all levels that fight)

    IEnumerator SwordAttack()
    {
        // FIX: guard is now a warning, not a silent fail — so you can diagnose in console
        if (swordHitbox == null)
        {
            Debug.LogWarning("[StalkerAI] SwordAttack aborted: swordHitbox is null.", this);
            yield break;
        }

        FacePlayerHorizontal();
        isAttacking = true;
        yield return null;

        if (enemyHealth.isHurt || enemyHealth.isDead) { isAttacking = false; yield break; }

        ApplyAnimatorForMelee();
        yield return null;

        PlayMeleeStrikeAnimations();
        SoundManager.instance.PlayWorldRandom(audioData.attackSounds, transform, 1f);

        yield return new WaitForSeconds(hitboxDelay);

        if (enemyHealth.isHurt || enemyHealth.isDead)
        {
            ClearMeleeStrikeAnimations();
            Collider2D c0 = swordHitbox.GetComponent<Collider2D>();
            if (c0 != null) c0.enabled = false;
            isAttacking = false;
            yield break;
        }

        // Position hitbox on the correct side
        Vector3 hp = swordHitbox.transform.localPosition;
        hp.x = facingRight ? Mathf.Abs(hp.x) : -Mathf.Abs(hp.x);
        swordHitbox.transform.localPosition = hp;

        Collider2D c = swordHitbox.GetComponent<Collider2D>();
        if (c != null) c.enabled = true;

        yield return new WaitForSeconds(hitboxDuration / Mathf.Max(0.01f, grappleSlowMultiplier));

        if (c != null) c.enabled = false;
        ClearMeleeStrikeAnimations();

        yield return new WaitForSeconds(attackCooldown / Mathf.Max(0.01f, grappleSlowMultiplier));

        isAttacking = false;
    }

    IEnumerator BowAttack(float windupSeconds)
    {
        FacePlayerHorizontal();
        isAttacking = true;
        yield return null;

        if (enemyHealth.isHurt || enemyHealth.isDead) { isAttacking = false; yield break; }

        ApplyAnimatorForRanged();
        yield return null;

        if (animator != null)
        {
            // FIX: only fire a trigger that actually exists in the controller
            string trigger = facingRight ? "BowAttackRight" : "BowAttackLeft";
            if (StalkerAnimatorHasTrigger(trigger))
                animator.SetTrigger(trigger);
            else if (StalkerAnimatorHasTrigger("BowAttackRight"))     // universal fallback
                animator.SetTrigger("BowAttackRight");
        }

        yield return new WaitForSeconds(windupSeconds);

        if (enemyHealth.isHurt || enemyHealth.isDead) { isAttacking = false; yield break; }

        FacePlayerHorizontal();

        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            GameObject arrowObj = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
            Arrow arrow = arrowObj.GetComponent<Arrow>();
            if (arrow != null)
            {
                arrow.isPlayerArrow = false;
                arrow.SetDirection(facingRight);
            }

            if (!facingRight)
            {
                Vector3 sc = arrowObj.transform.localScale;
                sc.x = -Mathf.Abs(sc.x);
                arrowObj.transform.localScale = sc;
            }
        }

        yield return new WaitForSeconds(attackCooldown / Mathf.Max(0.01f, grappleSlowMultiplier));
        isAttacking = false;
    }

    IEnumerator BombAttack()
    {
        FacePlayerHorizontal();
        isAttacking = true;
        yield return null;

        if (enemyHealth.isHurt || enemyHealth.isDead) { isAttacking = false; yield break; }

        if (bombPrefab != null && player != null)
        {
            Vector2 behind = playerController != null && playerController.facingRight
                ? Vector2.left : Vector2.right;
            Vector3 spawn = player.position + (Vector3)(behind * 1.75f);
            GameObject b  = Instantiate(bombPrefab, spawn, Quaternion.identity);
            Bomb bomb = b.GetComponent<Bomb>();
            if (bomb != null)
            {
                bomb.fuseTime        = bombFuse;
                bomb.explosionRadius = bombExplosionRadius;
            }
        }

        yield return new WaitForSeconds(0.35f / Mathf.Max(0.01f, grappleSlowMultiplier));
        isAttacking = false;
    }

    #endregion

    #region Shared helpers

    void ApplyAnimatorForMelee()
    {
        if (animator == null) return;
        if (meleeAnimatorController != null &&
            animator.runtimeAnimatorController != meleeAnimatorController)
            animator.runtimeAnimatorController = meleeAnimatorController;
    }

    void ApplyAnimatorForRanged()
    {
        if (animator == null) return;
        RuntimeAnimatorController target = rangedAnimatorController != null
            ? rangedAnimatorController
            : meleeAnimatorController;
        if (target != null && animator.runtimeAnimatorController != target)
            animator.runtimeAnimatorController = target;
    }

    bool StalkerAnimatorHasBool(string name)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == name && p.type == AnimatorControllerParameterType.Bool) return true;
        return false;
    }

    bool StalkerAnimatorHasTrigger(string name)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == name && p.type == AnimatorControllerParameterType.Trigger) return true;
        return false;
    }

    void PlayMeleeStrikeAnimations()
    {
        if (animator == null) return;
        if (StalkerAnimatorHasBool("IsSlashing"))
        {
            animator.SetBool("IsSlashing", true);
            return;
        }

        if (facingRight && StalkerAnimatorHasTrigger("AttackRight"))
            animator.SetTrigger("AttackRight");
        else if (!facingRight && StalkerAnimatorHasTrigger("AttackLeft"))
            animator.SetTrigger("AttackLeft");
        else if (StalkerAnimatorHasTrigger("AttackRight"))
            animator.SetTrigger("AttackRight");
    }

    void ClearMeleeStrikeAnimations()
    {
        if (animator == null) return;
        if (StalkerAnimatorHasBool("IsSlashing"))
            animator.SetBool("IsSlashing", false);
    }

    void UpdateTintAmount(float amount)
    {
        if (spriteRenderer == null || spriteRenderer.material == null) return;
        if (spriteRenderer.material.HasProperty(TintId))
            spriteRenderer.material.SetFloat(TintId, amount);
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;
        if (Mathf.Abs(transform.position.y - player.position.y) > yTolerance) return false;

        Vector2 origin    = new Vector2(transform.position.x, transform.position.y + 1f);
        Vector2 target    = new Vector2(player.position.x,    player.position.y    + 1f);
        Vector2 direction = (target - origin).normalized;
        float distance    = Vector2.Distance(origin, target);
        RaycastHit2D hit  = Physics2D.Raycast(origin, direction, distance, sightBlockLayers);
        return hit.collider == null;
    }

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
        if (player.position.x < transform.position.x && facingRight) Flip();
        else if (player.position.x > transform.position.x && !facingRight) Flip();
    }

    #endregion
}