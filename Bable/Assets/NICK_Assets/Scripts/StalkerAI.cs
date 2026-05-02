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
    public float bombThreatSenseRadius = 6.5f;
    public float bombFleeIfFuseRemainingBelow = 1.35f;
    public float bombFleeSpeedScale = 1.45f;

    [Header("Level 5 — Movement pressure")]
    [Tooltip("Blends toward the player like basic EnemyAI — reduces pure orbit kiting.")]
    [Range(0f, 1f)]
    public float approachBlendVsStrafe = 0.28f;

    [Header("Level 5 — Approach")]
    [Tooltip("How many seconds between forced approach dashes when player is at bow range.")]
    public float approachDashInterval = 1.8f;

    [Header("Level 5 — Melee Charge")]
    [Tooltip("Speed multiplier while winding up a sword swing (chasing the player).")]
    public float meleeWindupSpeedMul = 1.15f;

    #endregion

    #region Inspector — Combat (all levels)

    [Header("Combat (all levels)")]
    public float attackRange = 1.5f;
    public float attackCooldown = 0.7f;
    public float hitboxDelay = 0.3f;
    public float hitboxDuration = 0.1f;
    public float grappleSlowMultiplier = 1f;

    #endregion

    #region Inspector — Locomotion (matches PlayerController feel)

    [Header("Locomotion (matches PlayerController feel)")]
    public float moveSpeed = 7f;
    public float jumpForce = 12f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public Transform ceilingCheck;
    public float ceilingCheckRadius = 0.1f;
    public LayerMask ceilingLayer;

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
    bool wasHurtLastFrameAnim;

    const float DashBurstDuration = 0.15f;
    const float TeleportProximity = 1.5f;
    const float StrafeWalkSpeed = 2f;
    static readonly int TintId = Shader.PropertyToID("_TintAmount");
    [Header("Death Sequence")]
[Tooltip("How long the fade-out takes after death animation triggers.")]
public float deathFadeDuration = 2f;
[Tooltip("Delay after death starts before fade begins (lets the death animation play first).")]
public float deathFadeDelay = 0.3f;

bool deathSequenceStarted;

    // Animator parameters matching PlayerController.HandleAnimations()
    static readonly int AnimSpeed              = Animator.StringToHash("Speed");
    static readonly int AnimIsRunning          = Animator.StringToHash("Is_running");
    static readonly int AnimHorizontalVelocity = Animator.StringToHash("Horizontal_Velocity");
    static readonly int AnimFacingRight        = Animator.StringToHash("FacingRight");
    static readonly int AnimIsJumping          = Animator.StringToHash("IsJumping");
    static readonly int AnimIsFalling          = Animator.StringToHash("IsFalling");
    static readonly int AnimIsCrouching        = Animator.StringToHash("IsCrouching");
    static readonly int AnimIsWalking          = Animator.StringToHash("IsWalking");

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
    bool isInAttackWindup;       // winding up — keep chasing
    bool isInAttackActiveFrames; // hitbox is live — stand still briefly
    float distanceToPlayer;
    bool canSee;

    bool dynamicHitboxCreated;
    bool isGrounded;

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
    float level5ApproachTimer;
    float jumpCooldownTimer;
    #endregion

    #region Core lifecycle

    void Awake()
    {
        animator    = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        rb          = GetComponent<Rigidbody2D>();
        audioData   = GetComponent<StalkerAudio>();
        baseDashSpeed       = dashSpeed;
        baseAttackCooldown  = attackCooldown;

        AutoAssignMissingHierarchyReferences();
        SyncFacingBoolFromScale();
    }

    void SyncFacingBoolFromScale()
    {
        facingRight = transform.localScale.x >= 0f;
    }

    void AutoAssignMissingHierarchyReferences()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (swordHitbox == null)
        {
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

        if (swordHitbox == null)
        {
            SlashHitbox sh = GetComponentInChildren<SlashHitbox>(true);
            if (sh != null) swordHitbox = sh.gameObject;
        }

        if (swordHitbox == null && !dynamicHitboxCreated)
        {
            dynamicHitboxCreated = true;
            swordHitbox = new GameObject("StalkerSwordHitbox_Runtime");
            swordHitbox.transform.SetParent(transform);
            swordHitbox.transform.localPosition = new Vector3(0.8f, 0f, 0f);
            BoxCollider2D col = swordHitbox.AddComponent<BoxCollider2D>();
            col.size      = new Vector2(1.2f, 1.0f);
            col.isTrigger = true;
            col.enabled   = false;
            EnemyHitbox eh = swordHitbox.AddComponent<EnemyHitbox>();
            eh.damage = 1;
            Debug.LogWarning("[StalkerAI] No sword hitbox found — created runtime one.", this);
        }

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

            if (arrowSpawnPoint == null)
            {
                GameObject sp = new GameObject("ArrowSpawnPoint_Runtime");
                sp.transform.SetParent(transform);
                sp.transform.localPosition = new Vector3(0.5f, 0.25f, 0f);
                arrowSpawnPoint = sp.transform;
                Debug.LogWarning("[StalkerAI] No ArrowSpawnPoint found — created runtime fallback.", this);
            }
        }

        if (groundCheck == null)
        {
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Equals("GroundCheck", System.StringComparison.OrdinalIgnoreCase))
                {
                    groundCheck = t;
                    break;
                }
            }
        }

        if (ceilingCheck == null)
        {
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Equals("CeilingCheck", System.StringComparison.OrdinalIgnoreCase))
                {
                    ceilingCheck = t;
                    break;
                }
            }
        }
    }

    void TryCopyAnimatorAndWeaponsFromPlayer(PlayerController pc)
    {
        if (pc == null) return;

        if (pc.swordAnimator != null)
            meleeAnimatorController = pc.swordAnimator;
        if (pc.bowAnimator != null)
            rangedAnimatorController = pc.bowAnimator;

        if (arrowPrefab == null && pc.playerArrowPrefab != null)
            arrowPrefab = pc.playerArrowPrefab;

        if (bombPrefab == null)
        {
            BombAttack playerBombAttack = pc.GetComponent<BombAttack>();
            if (playerBombAttack != null && playerBombAttack.bombPrefab != null)
            {
                bombPrefab = playerBombAttack.bombPrefab;
                Debug.Log("[StalkerAI] Copied bombPrefab from Player's BombAttack component.", this);
            }
        }

        if (bombPrefab == null)
        {
            BombAttack stalkerBombAttack = GetComponent<BombAttack>();
            if (stalkerBombAttack != null && stalkerBombAttack.bombPrefab != null)
                bombPrefab = stalkerBombAttack.bombPrefab;
        }

        if (bombPrefab == null)
            Debug.LogWarning("[StalkerAI] bombPrefab is still null after player copy. " +
                             "Assign it in the Stalker Inspector or add BombAttack to the Player.", this);

        if (groundLayer.value == 0 && pc.groundLayer.value != 0)
            groundLayer = pc.groundLayer;
        if (ceilingLayer.value == 0 && pc.ceilingLayer.value != 0)
            ceilingLayer = pc.ceilingLayer;
    }

    void ApplyMeleeAnimatorController()
    {
        if (animator != null && meleeAnimatorController != null)
            animator.runtimeAnimatorController = meleeAnimatorController;
        else if (animator != null && meleeAnimatorController == null)
            Debug.LogWarning("[StalkerAI] meleeAnimatorController is null. " +
                             "Ensure PlayerController.swordAnimator is set.", this);
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
if (enemyHealth.isDead)
{
    if (!deathSequenceStarted)
    {
        deathSequenceStarted = true;
        StartCoroutine(DeathSequence());
    }
    return;
}
        if (player == null) return;

        distanceToPlayer = Vector2.Distance(transform.position, player.position);
        canSee = CanSeePlayer();

        UpdateGroundedFlag();

        if (jumpCooldownTimer > 0f) jumpCooldownTimer -= Time.deltaTime;

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

       if (level == 3 || level == 5)
{
    DriveLocomotionAnimator();
    DriveHurtAnimation();
}
void DriveHurtAnimation()
{
    if (animator == null || enemyHealth == null) return;

    bool isHurtNow = enemyHealth.isHurt;

    if (isHurtNow != wasHurtLastFrameAnim)
        animator.SetBool("Hurt", isHurtNow);

    wasHurtLastFrameAnim = isHurtNow;
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

    void UpdateGroundedFlag()
    {
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        else
            isGrounded = true;
    }

    bool HasRoomToStand()
    {
        if (ceilingCheck == null) return true;
        return !Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, ceilingLayer);
    }

    /// <summary>
    /// Drives the same animator parameters PlayerController.HandleAnimations() drives,
    /// so the Stalker's animator (using the player's controller) plays the proper
    /// run/walk/jump/fall/idle states based on actual physics state.
    /// </summary>
    void DriveLocomotionAnimator()
    {
        if (animator == null || rb == null) return;

        // During hurt or the brief active-frames swing, suppress locomotion params.
        // During windup, KEEP driving them so the Stalker visibly runs at the player.
        if (enemyHealth.isHurt || isInAttackActiveFrames)
        {
            animator.SetFloat(AnimSpeed, 0f);
            animator.SetBool(AnimIsRunning, false);
            animator.SetBool(AnimIsWalking, false);
            return;
        }

        float vx = rb.linearVelocity.x;
        float vy = rb.linearVelocity.y;
        float horizontalInput = Mathf.Clamp(vx / Mathf.Max(0.01f, moveSpeed), -1f, 1f);
        float speedMag = Mathf.Abs(horizontalInput);

        animator.SetFloat(AnimSpeed, speedMag);
        animator.SetBool(AnimIsRunning, speedMag > 0.1f);
        animator.SetBool(AnimIsWalking, speedMag > 0.1f);
        animator.SetFloat(AnimHorizontalVelocity, horizontalInput);
        animator.SetBool(AnimFacingRight, facingRight);
        animator.SetBool(AnimIsJumping, !isGrounded);
        animator.SetBool(AnimIsFalling, vy < -0.05f);
        animator.SetBool(AnimIsCrouching, false);
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
        UpdateTintAmount(Mathf.Clamp01(
            Mathf.InverseLerp(visibilityFadeEnd, visibilityFadeStart, distanceToPlayer)));
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
        level5ApproachTimer = Random.Range(0.5f, approachDashInterval);
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

        dashSpeed      = isRaging ? baseDashSpeed      * 1.3f : baseDashSpeed;
        attackCooldown = isRaging ? baseAttackCooldown * 0.7f : baseAttackCooldown;
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

        if (!isAttacking && !enemyHealth.isHurt && !isDashing)
        {
            // Be more permissive — start the swing earlier and let the windup chase close the gap
            float swingStartRange = attackRange * 2.0f;

            if (canSee && distanceToPlayer <= swingStartRange)
            {
                pendingSwordAfterDash = false;
                StartCoroutine(SwordAttack());
            }
            else if (IsPlayerBehindStalker())
            {
                pendingSwordAfterDash = false;
                TryBeginDashTowardPlayer();
            }
            else if (canSee && distanceToPlayer > swingStartRange && distanceToPlayer <= sightRange)
            {
                level5ApproachTimer -= Time.deltaTime;
                if (level5ApproachTimer <= 0f && dashCooldownTimer <= 0f)
                {
                    level5ApproachTimer = Random.Range(approachDashInterval * 0.7f,
                                                       approachDashInterval * 1.3f);
                    if (TryBeginDashTowardPlayer())
                    {
                        pendingSwordAfterDash = true;
                        return;
                    }
                }

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

    void EnsureSwordDamagesPlayer()
    {
        if (swordHitbox == null) return;
        if (dynamicHitboxCreated) return;

        if (swordHitbox.GetComponent<EnemyHitbox>() != null &&
            swordHitbox.GetComponent<SlashHitbox>()  == null)
            return;

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
// NEW: Force the hitbox onto your enemy attack layer so it can hit the player!
// Replace "EnemyAttack" with whatever layer your normal enemies use for their hitboxes.
//swordHitbox.layer = LayerMask.NameToLayer("Character");
        Collider2D swordCol = swordHitbox.GetComponent<Collider2D>();
        if (swordCol != null)
        {
            foreach (Collider2D bodyCol in GetComponentsInChildren<Collider2D>(true))
            {
                if (bodyCol == null || bodyCol == swordCol) continue;
                Physics2D.IgnoreCollision(swordCol, bodyCol, true);
            }
            swordCol.isTrigger = true;
        }

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

        if (IsPlayerMeleeHitboxActive() && distanceToPlayer < attackRange * 3f)
        {
            float dodgeDir       = playerController != null && playerController.facingRight ? -1f : 1f;
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
                rb.linearVelocity   = new Vector2(0f, rb.linearVelocity.y);
                dashCooldownTimer   = dashCooldown / grapple;

                if (pendingSwordAfterDash)
                {
                    pendingSwordAfterDash = false;
                    if (!enemyHealth.isDead) StartCoroutine(SwordAttack());
                }
            }
            else
                rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed * grapple, rb.linearVelocity.y);
            return;
        }

        // Keep moving during windup; stand still only during the actual swing's active frames
        bool canMove = !isInAttackActiveFrames;

        if (canMove)
        {
            Vector2 toPlayer = (Vector2)(player.position - transform.position);
            float horizontalInput = 0f;

            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                if (isInAttackWindup)
                {
                    // charge directly toward the player while raising the sword
                    horizontalInput = Mathf.Sign(toPlayer.x);
                }
                else
                {
                    float tangentX = -Mathf.Sign(toPlayer.y);
                    if (Mathf.Abs(toPlayer.y) < 0.3f) tangentX = 0f;
                    float lateralSign = strafeOrbitSign;

                    float towardPlayer = Mathf.Sign(toPlayer.x);
                    float w = Mathf.Clamp01((distanceToPlayer - 1.5f) / 3.5f) * approachBlendVsStrafe;

                    float strafeIntent  = lateralSign * 0.8f;
                    float approachIntent = towardPlayer * 1.0f;
                    horizontalInput = Mathf.Lerp(strafeIntent, approachIntent, w);
                }
            }

            float speedMul = isInAttackWindup ? meleeWindupSpeedMul : 1.0f;
            float targetVx = horizontalInput * moveSpeed * speedMul * grapple;
            rb.linearVelocity = new Vector2(targetVx, rb.linearVelocity.y);

            TryAutoJump();
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    void TryAutoJump()
    {
        if (!isGrounded) return;
        if (jumpCooldownTimer > 0f) return;
        if (!HasRoomToStand()) return;
        if (player == null) return;

        bool playerAbove = player.position.y > transform.position.y + 1.2f;
        bool wallInFront = IsWallInFront();

        if (playerAbove || wallInFront)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCooldownTimer = 0.5f;
        }
    }

    bool IsWallInFront()
    {
        if (groundCheck == null) return false;
        Vector2 origin = (Vector2)transform.position;
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, 0.6f, groundLayer);
        return hit.collider != null;
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
            float blast   = b.explosionRadius + 0.6f;
            float urgency = (blast / Mathf.Max(0.35f, dist)) * (1f / Mathf.Max(0.15f, rem));
            if (urgency > bestUrgency)
            {
                bestUrgency = urgency;
                bestDir     = fromBomb;
            }
        }

        if (bestUrgency < 0.01f) return false;
        horizontalVel = new Vector2(bestDir.x * moveSpeed * bombFleeSpeedScale * grapple, 0f);
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

        isDashing      = true;
        float g        = Mathf.Max(0.01f, grappleSlowMultiplier);
        dashBurstTimer = DashBurstDuration / g;
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

        if (playerController.downAttackHitbox != null &&
            playerController.downAttackHitbox.activeInHierarchy)
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
        if (swordHitbox == null)
        {
            Debug.LogWarning("[StalkerAI] SwordAttack aborted: swordHitbox is null.", this);
            yield break;
        }

        FacePlayerHorizontal();
        isAttacking = true;
        isInAttackWindup = true;
        isInAttackActiveFrames = false;
        yield return null;

        if (enemyHealth.isHurt || enemyHealth.isDead) { ResetAttackFlags(); yield break; }

        ApplyAnimatorForMelee();
        yield return null;

        PlayMeleeStrikeAnimations();
        if (audioData != null && audioData.attackSounds != null && audioData.attackSounds.Length > 0)
            SoundManager.instance.PlayWorldRandom(audioData.attackSounds, transform, 1f);

        // windup phase: keep chasing the player
        yield return new WaitForSeconds(hitboxDelay);

        if (enemyHealth.isHurt || enemyHealth.isDead)
        {
            ClearMeleeStrikeAnimations();
            Collider2D c0 = swordHitbox.GetComponent<Collider2D>();
            if (c0 != null) c0.enabled = false;
            ResetAttackFlags();
            yield break;
        }

        // active frames: face player, plant feet, swing
FacePlayerHorizontal();
isInAttackWindup = false;
isInAttackActiveFrames = true;

Collider2D c = swordHitbox.GetComponent<Collider2D>();
if (c != null) c.enabled = true;

yield return new WaitForSeconds(hitboxDuration / Mathf.Max(0.01f, grappleSlowMultiplier));

if (c != null) c.enabled = false;
ClearMeleeStrikeAnimations();

isInAttackActiveFrames = false;

// recovery: short cooldown but stalker can move again
yield return new WaitForSeconds(attackCooldown / Mathf.Max(0.01f, grappleSlowMultiplier));

ResetAttackFlags();
    }

    void ResetAttackFlags()
    {
        isAttacking = false;
        isInAttackWindup = false;
        isInAttackActiveFrames = false;
    }

    IEnumerator BowAttack(float windupSeconds)
{
    FacePlayerHorizontal();
    isAttacking = true;
    isInAttackActiveFrames = true;  // lock movement during bow shot
    yield return null;

    if (enemyHealth.isHurt || enemyHealth.isDead) 
    { 
        isAttacking = false; 
        isInAttackActiveFrames = false;
        yield break; 
    }

    ApplyAnimatorForRanged();
    yield return null;

    if (animator != null)
    {
        string trigger = facingRight ? "BowAttackRight" : "BowAttackLeft";
        if (StalkerAnimatorHasTrigger(trigger))
            animator.SetTrigger(trigger);
        else if (StalkerAnimatorHasTrigger("BowAttackRight"))
            animator.SetTrigger("BowAttackRight");
    }

    yield return new WaitForSeconds(windupSeconds);

    if (enemyHealth.isHurt || enemyHealth.isDead)
    {
        ApplyAnimatorForMelee();
        isAttacking = false;
        isInAttackActiveFrames = false;
        yield break;
    }

    FacePlayerHorizontal();

    if (arrowPrefab != null && arrowSpawnPoint != null)
    {
        GameObject arrowObj = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);

        arrowObj.layer = LayerMask.NameToLayer("Default");

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

    // brief pause after firing for animation completion, but unlock movement here
    isInAttackActiveFrames = false;

    yield return new WaitForSeconds(attackCooldown / Mathf.Max(0.01f, grappleSlowMultiplier));

    ApplyAnimatorForMelee();
    isAttacking = false;
}

    IEnumerator BombAttack()
    {
        FacePlayerHorizontal();
        isAttacking = true;
        yield return null;

        if (enemyHealth.isHurt || enemyHealth.isDead)
        {
            ApplyAnimatorForMelee();
            isAttacking = false;
            yield break;
        }

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

        ApplyAnimatorForMelee();
        isAttacking = false;
    }

IEnumerator DeathSequence()
{
     if (EndCutscene.Instance != null)
        EndCutscene.Instance.StartCutscene();
    // stop all motion
    if (rb != null)
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    // disable any colliders so the corpse doesn't keep blocking the player
    foreach (Collider2D col in GetComponentsInChildren<Collider2D>(true))
        if (col != null) col.enabled = false;

    // make sure we're using the melee animator (death animations live there)
    ApplyAnimatorForMelee();

    // trigger the right-facing or left-facing death animation
    if (animator != null)
    {
        if (facingRight && StalkerAnimatorHasTrigger("DeathRight"))
            animator.SetTrigger("DeathRight");
        else if (!facingRight && StalkerAnimatorHasTrigger("DeathLeft"))
            animator.SetTrigger("DeathLeft");
        else if (StalkerAnimatorHasTrigger("DeathRight"))
            animator.SetTrigger("DeathRight"); // fallback
    }

    // brief delay so the death animation plays before the fade starts
    yield return new WaitForSeconds(deathFadeDelay);

    // fade the sprite out over deathFadeDuration
    if (spriteRenderer != null)
    {
        Color startColor = spriteRenderer.color;
        float elapsed = 0f;
        while (elapsed < deathFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / deathFadeDuration;
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            spriteRenderer.color = c;
            yield return null;
        }

        Color finalColor = startColor;
        finalColor.a = 0f;
        spriteRenderer.color = finalColor;
    }

    // remove from scene
    Destroy(gameObject);
}
    #endregion

    #region Shared helpers

    public void SpawnPlayerArrow() { }
    public void SpawnIceArrow()    { }
    public void SpawnFireArrow()   { }

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
    
    // mirror the sword hitbox to the new facing side
    if (swordHitbox != null)
    {
        Vector3 hbPos = swordHitbox.transform.localPosition;
        hbPos.x = -hbPos.x;
        swordHitbox.transform.localPosition = hbPos;
    }
}

    void FacePlayerHorizontal()
    {
        if (player == null) return;
        if (player.position.x < transform.position.x && facingRight) Flip();
        else if (player.position.x > transform.position.x && !facingRight) Flip();
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (ceilingCheck != null)
        {
            Gizmos.color = HasRoomToStand() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
        }
    }

    #endregion
}