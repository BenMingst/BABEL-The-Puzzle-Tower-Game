using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private float footstepRate = 0f;
    private float footstepTimer = 0f;
    private AudioSource audioSource;
    private PlayerAudio playerAudio;

    [Header("Menus")]
    public GameObject deathCanvas;
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public bool isPaused = false;

    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Crouch")]
    public float crouchSpeedMultiplier = 0.5f;
    public Collider2D standingCollider;
    public Collider2D crouchingCollider;
    public Collider2D rollingCollider;
    public Transform ceilingCheck;
    public float ceilingCheckRadius = 0.1f;
    public LayerMask ceilingLayer;

    [Header("Dropdown")]
    public Collider2D dropdownCollider;
    public float dropdownCooldown = 0.5f;
    private bool isDropping = false;
    private bool isInsideDropdown = false;

    [Header("Attack")]
    public float slashDuration = 0.575f;
    public GameObject slashHitbox;
    public GameObject downAttackHitbox;
    public float downAttackBounceForce = 8f;

    [Header("Doink")]
    public float doinkDuration = 0.575f;
    public float doinkKnockbackForce = 1.5f;

    [Header("Down Attack")]
    public float downAttackDelay = 0.15f;
    private float jumpTimer = 0f;

    [Header("Bow")]
    public GameObject playerArrowPrefab;
    public GameObject iceArrowPrefab;
    public GameObject fireArrowPrefab;
    public Transform arrowSpawnPoint;
    public BowCooldownUI bowCooldownUI;

    [Header("Animator Controllers")]
    public RuntimeAnimatorController noSwordAnimator;
    public RuntimeAnimatorController swordAnimator;
    public RuntimeAnimatorController bowAnimator;
    public RuntimeAnimatorController bombAnimator;
    public bool hasSword = false;
    public bool hasBow = false;
    public bool hasBomb = false;

    [Header("Freeze Effect")]
    public float freezeEffect = 0.3f;
    public float freezeDuration = 1.5f;
    public float afterFreezeDuration = 1f;
    public GameObject freezeOverlayObject;
    private bool isFrozen = false;
    private Coroutine freezeCoroutine;

    [Header("Burn Effect")]
    public float afterBurnDuration = 1f;
    public int afterBurnDamage = 1;
    public GameObject burnOverlayObject;
    private bool isBurning = false;
    private Coroutine burnCoroutine;

    public bool isDead = false;
    public Vector2 platformVelocity = Vector2.zero;
    private Rigidbody2D rb;
    public Animator animator;
    private bool isGrounded;
    private bool isCrouching;
    private float horizontalInput;
    public bool facingRight = true;
    private bool isAttacking = false;
    public bool isRolling = false;
    public bool isHurt = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = noSwordAnimator;
        playerAudio = GetComponent<PlayerAudio>();
        if (playerAudio == null) playerAudio = gameObject.AddComponent<PlayerAudio>();

        if (freezeOverlayObject != null)
            freezeOverlayObject.SetActive(false);
        if (burnOverlayObject != null)
            burnOverlayObject.SetActive(false);
    }

    public void EquipSword()
    {
        hasSword = true;
        GameManager.hasSword = true;
        InventoryManager.Instance.AddItem(0, swordAnimator);
    }

    public void EquipBow()
    {
        hasBow = true;
        GameManager.hasBow = true;
        InventoryManager.Instance.AddItem(1, bowAnimator);
    }

    public void EquipBomb()
    {
        hasBomb = true;
        GameManager.hasBomb = true;
        InventoryManager.Instance.AddItem(2, bombAnimator);
    }

    public void SetInsideDropdown(bool value)
    {
        isInsideDropdown = value;
    }

    bool CanUseSword()
    {
        return hasSword && InventoryManager.Instance != null && InventoryManager.Instance.IsSwordSelected();
    }

    bool CanUseBow()
    {
        return hasBow && InventoryManager.Instance != null && InventoryManager.Instance.IsBowSelected();
    }

    bool CanUseBomb()
    {
        return hasBomb && InventoryManager.Instance != null && InventoryManager.Instance.IsBombSelected();
    }

    public void OnBombExploded()
    {
        BombAttack ba = GetComponent<BombAttack>();
        if (ba != null) ba.OnBombExploded();
    }

    public void OnDeath()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z);

        animator.runtimeAnimatorController = swordAnimator;
        animator.updateMode = AnimatorUpdateMode.Normal;

        if (facingRight)
            animator.SetTrigger("DeathRight");
        else
            animator.SetTrigger("DeathLeft");

        if (DeathScreenEffect.Instance == null)
            Debug.Log("DeathScreenEffect Instance is NULL");
        else
            DeathScreenEffect.Instance.PlayDeathEffect();
        // play game over sound
        SoundManager.instance.PlayUIClip(SoundManager.instance.gameOverSound, 1f);
    }

    public void SpawnPlayerArrow()
    {
        if (!isAttacking) return;
        if (bowCooldownUI != null)
            bowCooldownUI.UseArrow();
        SpawnArrowOfType(Arrow.ArrowType.Normal, playerArrowPrefab);
        // play arrow shoot sound
        SoundManager.instance.PlayUIClip(playerAudio.normalArrowSpawnSound, 1f);
    }

    public void SpawnIceArrow()
    {
        if (!isAttacking) return;
        if (bowCooldownUI != null)
            bowCooldownUI.UseArrow();
        SpawnArrowOfType(Arrow.ArrowType.Ice, iceArrowPrefab);
        // play ice arrow shoot sound
        SoundManager.instance.PlayUIClip(playerAudio.iceArrowSpawnSound, 1f);
    }

    public void SpawnFireArrow()
    {
        if (!isAttacking) return;
        if (bowCooldownUI != null)
            bowCooldownUI.UseArrow();
        SpawnArrowOfType(Arrow.ArrowType.Fire, fireArrowPrefab);
        // play fire arrow shoot sound
        SoundManager.instance.PlayUIClip(playerAudio.fireArrowSpawnSound, 1f);
    }

    void SpawnArrowOfType(Arrow.ArrowType type, GameObject prefab)
    {
        if (prefab == null) return;
        GameObject arrow = Instantiate(prefab, arrowSpawnPoint.position, Quaternion.identity);
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        arrowScript.arrowType = type;
        arrowScript.SetDirection(facingRight);
        if (!facingRight)
        {
            Vector3 scale = arrow.transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            arrow.transform.localScale = scale;
        }
    }

    bool HasRoomToStand()
    {
        if (ceilingCheck == null) return true;
        return !Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, ceilingLayer);
    }

    public void ApplyFreezeEffect()
    {
        if (freezeCoroutine != null)
            StopCoroutine(freezeCoroutine);
        freezeCoroutine = StartCoroutine(FreezeSequence());
    }

    IEnumerator FreezeSequence()
    {
        if (isFrozen)
            moveSpeed = moveSpeed / freezeEffect;

        isFrozen = true;
        SoundManager.instance.PlayWorldRandom(playerAudio.freezeSounds, transform, 1f);
        float originalSpeed = moveSpeed;
        moveSpeed *= freezeEffect;

        if (freezeOverlayObject != null)
            freezeOverlayObject.SetActive(true);

        yield return new WaitForSeconds(freezeDuration);
        yield return new WaitForSeconds(afterFreezeDuration);

        if (freezeOverlayObject != null)
            freezeOverlayObject.SetActive(false);

        moveSpeed = originalSpeed;
        isFrozen = false;
        freezeCoroutine = null;
    }

    public void ApplyBurnEffect()
    {
        if (burnCoroutine != null)
            StopCoroutine(burnCoroutine);
        burnCoroutine = StartCoroutine(BurnSequence());
    }

    IEnumerator BurnSequence()
    {
        isBurning = true;

        if (burnOverlayObject != null)
            burnOverlayObject.SetActive(true);
            SoundManager.instance.PlayWorldRandom(playerAudio.burnSounds, transform, 1f);

        yield return new WaitForSeconds(afterBurnDuration);

        PlayerHealth ph = GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamageNoKnockback(afterBurnDamage);

        if (burnOverlayObject != null)
            burnOverlayObject.SetActive(false);

        isBurning = false;
        burnCoroutine = null;
    }

    bool IsHittingInvulnerableEnemy()
    {
        if (slashHitbox == null) return false;

        Collider2D hitboxCol = slashHitbox.GetComponent<Collider2D>();
        if (hitboxCol == null) return false;

        bool wasActive = slashHitbox.activeSelf;
        slashHitbox.SetActive(true);

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            slashHitbox.transform.position,
            hitboxCol.bounds.size,
            0f);

        if (!wasActive) slashHitbox.SetActive(false);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player")) continue;
            if (hit.isTrigger) continue;

            // check necromancer
            NecromancerHealth necroHealth = hit.GetComponentInParent<NecromancerHealth>();
            if (necroHealth != null)
            {
                NecromancerAI necroAI = necroHealth.GetComponent<NecromancerAI>();
                if (necroAI != null && !necroAI.IsVulnerable()) return true;
            }

            // check normal enemy invulnerability
            EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.IsInvulnerable()) return true;

            // check armored skelly
            ArmoredSkellyHealth armoredHealth = hit.GetComponentInParent<ArmoredSkellyHealth>();
            if (armoredHealth != null)
            {
                ArmoredSkellyAI ai = armoredHealth.GetComponent<ArmoredSkellyAI>();
                if (ai != null && ai.isArmored) return true;
            }

            // check ground layer
            if (((1 << hit.gameObject.layer) & groundLayer) != 0) return true;
        }

        return false;
    }

    IEnumerator DoinkAttack()
    {
        isAttacking = true;

        if (facingRight)
            animator.SetTrigger("DoinkRight");
        else
            animator.SetTrigger("DoinkLeft");

        float knockbackDirection = facingRight ? -1f : 1f;
        rb.linearVelocity = new Vector2(knockbackDirection * doinkKnockbackForce, rb.linearVelocity.y);

        slashHitbox.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        slashHitbox.SetActive(false);

        rb.linearVelocity = new Vector2(knockbackDirection * 0.5f, rb.linearVelocity.y);

        yield return new WaitForSeconds(doinkDuration - 0.1f);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isAttacking = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isDead)
                Pause();
        }

        if (isDead) return;
        if (isHurt) return;

        ItemPickup pickup = FindObjectOfType<ItemPickup>();
        if (pickup != null && pickup.inCutscene) return;

        Chest chest = FindObjectOfType<Chest>();
        if (chest != null && chest.inCutscene) return;

        LockedDoor door = FindObjectOfType<LockedDoor>();
        if (door != null && door.inCutscene) return;

        OneWayDoorEntrance entranceDoor = FindObjectOfType<OneWayDoorEntrance>();
        if (entranceDoor != null && entranceDoor.inCutscene) return;

        OneWayDoorExit exitDoor = FindObjectOfType<OneWayDoorExit>();
        if (exitDoor != null && exitDoor.inCutscene) return;

        Sign sign = FindObjectOfType<Sign>();
        if (sign != null && sign.inCutscene) return;

        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A)) horizontalInput = -1f;
        if (Input.GetKey(KeyCode.D)) horizontalInput = 1f;


        // Footstep sounds

        if (isGrounded && Mathf.Abs(horizontalInput) > 0.1f && !isCrouching && !isRolling && !isAttacking)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                SoundManager.instance.PlayWorldRandom(playerAudio.walkSounds, transform, 1f);
                footstepTimer = footstepRate;
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        isCrouching = (Input.GetKey(KeyCode.S) || (!HasRoomToStand() && isGrounded)) && isGrounded && !isAttacking && !isRolling && !isHurt;

        BombAttack ba = GetComponent<BombAttack>();

        if (Input.GetKeyDown(KeyCode.W) && isGrounded && HasRoomToStand())
        {
            if (ba != null && ba.isWindingUp) { }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                SoundManager.instance.PlayWorldRandom(playerAudio.jumpSounds, transform, 1f);
                jumpTimer = 0f;
            }
        }

        if (!isGrounded)
        {
            jumpTimer += Time.deltaTime;
        }
        else
        {
            jumpTimer = 0f;
        }

        if (!isGrounded && rb.linearVelocity.y > 0 && !isDropping)
        {
            if (dropdownCollider != null)
            {
                Physics2D.IgnoreCollision(standingCollider, dropdownCollider, true);
                Physics2D.IgnoreCollision(crouchingCollider, dropdownCollider, true);
                Physics2D.IgnoreCollision(rollingCollider, dropdownCollider, true);
            }
        }
        else if (!isDropping && isGrounded && !isInsideDropdown)
        {
            if (dropdownCollider != null)
            {
                Physics2D.IgnoreCollision(standingCollider, dropdownCollider, false);
                Physics2D.IgnoreCollision(crouchingCollider, dropdownCollider, false);
                Physics2D.IgnoreCollision(rollingCollider, dropdownCollider, false);
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (!isGrounded && jumpTimer >= downAttackDelay && CanUseSword())
            {
                animator.SetBool("DownAttack", true);
                downAttackHitbox.GetComponent<Collider2D>().enabled = true;
            }
            else if (isGrounded && isCrouching && !isDropping)
            {
                StartCoroutine(DropDown());
            }
        }

        if (isGrounded)
        {
            animator.SetBool("DownAttack", false);
            downAttackHitbox.GetComponent<Collider2D>().enabled = false;
        }

        if (Input.GetMouseButtonDown(0) && !isPaused && !isAttacking && !isRolling && isGrounded && CanUseSword())
        {
            isCrouching = false;
            if (IsHittingInvulnerableEnemy())
                StartCoroutine(DoinkAttack());
            else
                StartCoroutine(SlashAttack());
        }
        else if (Input.GetMouseButtonDown(0) && !isPaused && !isAttacking && !isRolling && CanUseBow())
        {
            if (bowCooldownUI != null && !bowCooldownUI.HasArrows()) return;
            isCrouching = false;
            StartCoroutine(BowAttack());
        }
        else if (Input.GetMouseButtonDown(0) && !isPaused && CanUseBomb())
        {
            if (ba != null && !ba.bombActive)
            {
                ba.isCrouchingWhenThrown = isCrouching;
                ba.StartBombAttack();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking && !isRolling && isGrounded)
        {
            isCrouching = false;
            StartCoroutine(Roll());
        }

        HandleAnimations();
        HandleColliders();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (horizontalInput != 0 && !isAttacking && !isRolling && !isHurt && !isCrouching)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed + platformVelocity.x, rb.linearVelocity.y);
        }
        else if (!isAttacking && !isRolling && !isHurt)
        {
            rb.linearVelocity = new Vector2(platformVelocity.x, rb.linearVelocity.y);
        }

        platformVelocity = Vector2.zero;
    }

    IEnumerator DropDown()
    {
        isDropping = true;

        SoundManager.instance.PlayWorldRandom(playerAudio.dropDownSounds, transform, 1f);

        if (dropdownCollider != null)
        {
            Physics2D.IgnoreCollision(standingCollider, dropdownCollider, true);
            Physics2D.IgnoreCollision(crouchingCollider, dropdownCollider, true);
            Physics2D.IgnoreCollision(rollingCollider, dropdownCollider, true);
        }

        yield return new WaitForSeconds(dropdownCooldown);

        if (dropdownCollider != null)
        {
            Physics2D.IgnoreCollision(standingCollider, dropdownCollider, false);
            Physics2D.IgnoreCollision(crouchingCollider, dropdownCollider, false);
            Physics2D.IgnoreCollision(rollingCollider, dropdownCollider, false);
        }

        isDropping = false;
    }

    IEnumerator SlashAttack()
    {
        isAttacking = true;
        animator.SetBool("IsSlashing", true);

        SoundManager.instance.PlayWorldRandom(playerAudio.swordSlashAttackSounds, transform, 1f);

        float lungeDirection = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(lungeDirection * 1.5f, rb.linearVelocity.y);

        slashHitbox.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        rb.linearVelocity = new Vector2(lungeDirection * 0.75f, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.1f);

        slashHitbox.SetActive(false);

        rb.linearVelocity = new Vector2(lungeDirection * 0.25f, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.375f);

        animator.SetBool("IsSlashing", false);
        isAttacking = false;
    }

    IEnumerator BowAttack()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        SoundManager.instance.PlayWorldRandom(playerAudio.bowAttackSounds, transform, 1f);

        if (ArrowTypeManager.Instance != null)
            ArrowTypeManager.Instance.isShooting = true;

        ArrowTypeManager.ArrowType arrowType = ArrowTypeManager.Instance != null ?
            ArrowTypeManager.Instance.currentArrowType :
            ArrowTypeManager.ArrowType.Normal;

        if (facingRight)
        {
            switch (arrowType)
            {
                case ArrowTypeManager.ArrowType.Normal:
                    animator.SetTrigger("BowAttackRight");
                    break;
                case ArrowTypeManager.ArrowType.Ice:
                    animator.SetTrigger("IceBowAttackRight");
                    break;
                case ArrowTypeManager.ArrowType.Fire:
                    animator.SetTrigger("FireBowAttackRight");
                    break;
            }
        }
        else
        {
            switch (arrowType)
            {
                case ArrowTypeManager.ArrowType.Normal:
                    animator.SetTrigger("BowAttackLeft");
                    break;
                case ArrowTypeManager.ArrowType.Ice:
                    animator.SetTrigger("IceBowAttackLeft");
                    break;
                case ArrowTypeManager.ArrowType.Fire:
                    animator.SetTrigger("FireBowAttackLeft");
                    break;
            }
        }

        yield return new WaitForSeconds(0.5f);

        if (ArrowTypeManager.Instance != null)
            ArrowTypeManager.Instance.isShooting = false;

        isAttacking = false;
    }

    IEnumerator Roll()
    {
        isRolling = true;
        animator.SetBool("IsRolling", true);

        SoundManager.instance.PlayWorldRandom(playerAudio.rollSounds, transform, 1f);

        float rollDirection = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(rollDirection * 4f, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.3f);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.2f);

        animator.SetBool("IsRolling", false);
        isRolling = false;

        if (!HasRoomToStand() && isGrounded)
        {
            isCrouching = true;
            animator.SetBool("IsCrouching", true);
        }
    }

    public void DownAttackBounce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, downAttackBounceForce);
        animator.SetBool("DownAttack", true);
        SoundManager.instance.PlayWorldRandom(playerAudio.swordDownAttackSounds, transform, 1f);
    }

    void HandleAnimations()
    {
        if (isDead) return;

        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetBool("Is_running", Mathf.Abs(horizontalInput) > 0.1f);
        animator.SetFloat("Horizontal_Velocity", horizontalInput);
        animator.SetBool("FacingRight", facingRight);
        animator.SetBool("IsJumping", !isGrounded);
        animator.SetBool("IsFalling", rb.linearVelocity.y < 0);
        animator.SetBool("IsCrouching", isCrouching);

        if (!isAttacking && !isRolling)
        {
            if (horizontalInput > 0) facingRight = true;
            if (horizontalInput < 0) facingRight = false;
        }

        if (slashHitbox != null)
        {
            Vector3 hitboxPos = slashHitbox.transform.localPosition;
            hitboxPos.x = facingRight ? Mathf.Abs(hitboxPos.x) : -Mathf.Abs(hitboxPos.x);
            slashHitbox.transform.localPosition = hitboxPos;
        }
    }

    void HandleColliders()
    {
        if (standingCollider == null || crouchingCollider == null || rollingCollider == null) return;

        if (isRolling)
        {
            standingCollider.enabled = false;
            crouchingCollider.enabled = false;
            rollingCollider.enabled = true;
        }
        else if (isCrouching || (!HasRoomToStand() && isGrounded))
        {
            standingCollider.enabled = false;
            crouchingCollider.enabled = true;
            rollingCollider.enabled = false;
        }
        else
        {
            standingCollider.enabled = true;
            crouchingCollider.enabled = false;
            rollingCollider.enabled = false;
        }
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

    public void Pause()
    {
        if (!isPaused)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
            // play pause sound
            SoundManager.instance.PlayUIClip(SoundManager.instance.pauseSound, 1f);
        }
        else
        {
            pausePanel.SetActive(false);
            optionsPanel.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
            // play unpause sound
            SoundManager.instance.PlayUIClip(SoundManager.instance.unpauseSound, 1f);
        }
    }
}