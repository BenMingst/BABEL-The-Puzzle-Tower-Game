using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerController : MonoBehaviour
{
    // ─── Movement ───────────────────────────────────────────────────────────────
    public float walkSpeed    = 5f;
    public float runSpeed     = 10f;
    public float airWalkSpeed = 5f;
    public float airRunSpeed = 5f;

    [Header("Crouch")]
    public float     crouchSpeedMultiplier = 0.5f;
    public Collider2D standingCollider;
    public Collider2D crouchingCollider;

    // ─── Jump ────────────────────────────────────────────────────────────────────
    [Header("Jump")]
    public float jumpImpulse      = 10f;   // initial burst
    public float jumpHoldForce    = 25f;   // sustained upward force while held
    public float maxJumpHoldTime  = 0.20f; // max seconds of hold boost

    // ─── Attack ──────────────────────────────────────────────────────────────────
    [Header("Attack")]
    public float     slashDuration       = 0.575f;
    public GameObject slashHitbox;
    public GameObject downAttackHitbox;
    public float     downAttackBounceForce = 8f;

    [Header("Down Attack")]
    public float downAttackDelay = 0.15f;  // min airborne time before down-attack triggers

    // ─── UI / Meta ───────────────────────────────────────────────────────────────
    [Header("Death")]
    [SerializeField] private GameObject deathPanel;

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private bool pauseStopsTime = true;

    // ─── Private state ───────────────────────────────────────────────────────────
    private Rigidbody2D       rb;
    private Animator          animator;
    private TouchingDirections touchingDirections;

    private Vector2 moveInput;

    // jump
    private bool  isJumpHeld;
    private float jumpHoldTimer;
    private float jumpTimer = 0f; // tracks time airborne (for down-attack delay)

    // actions
    private bool isCrouching = false;
    private bool isAttacking  = false;
    public  bool isRolling    = false;
    public  bool isHurt       = false;

    // meta
    private bool  isPaused      = false;
    private float prevTimeScale = 1f;

    // ─── Serialized backing fields (visible in Inspector) ────────────────────────
    [SerializeField] private bool _isMoving    = false;
    [SerializeField] private bool _isRunning   = false;
    [SerializeField] private bool _isJumping   = false;
    [SerializeField] private bool _isDead      = false;
    public                   bool _isFacingRight = true;

    // ─── Properties ──────────────────────────────────────────────────────────────
    public bool IsMoving
    {
        get => _isMoving;
        private set { _isMoving = value; animator.SetBool(AnimationStrings.isMoving, value); }
    }

    public bool IsRunning
    {
        get => _isRunning;
        set { _isRunning = value; animator.SetBool(AnimationStrings.isRunning, value); }
    }

    public bool IsJumping
    {
        get => _isJumping;
        set { _isJumping = value; animator.SetBool(AnimationStrings.isJumping, value); }
    }

    public bool IsDead
    {
        get => _isDead;
        set { _isDead = value; animator.SetBool(AnimationStrings.deathTrigger, value); }
    }

    /// <summary>
    /// Flips localScale.x when direction changes — single source of truth for facing.
    /// </summary>
    public bool IsFacingRight
    {
        get => _isFacingRight;
        private set
        {
            if (_isFacingRight != value)
                transform.localScale *= new Vector2(-1, 1);
            _isFacingRight = value;
        }
    }

    public bool CanMove => animator.GetBool(AnimationStrings.canMove);

    /// <summary>
    /// Resolves the correct horizontal speed given current state.
    /// </summary>
    public float CurrentMoveSpeed
    {
        get
        {
            if (!CanMove || isPaused || isAttacking || isRolling || isHurt) return 0f;
            if (!IsMoving || touchingDirections.IsOnWall) return 0f;

            if (isCrouching)               return walkSpeed * crouchSpeedMultiplier;
            if (touchingDirections.IsGrounded) return IsRunning ? runSpeed : walkSpeed;
            if (IsRunning) return airRunSpeed;
            else return airWalkSpeed;
        }
    }

    // ─── Unity lifecycle ─────────────────────────────────────────────────────────
    private void Awake()
    {
        rb                  = GetComponent<Rigidbody2D>();
        animator            = GetComponent<Animator>();
        touchingDirections  = GetComponent<TouchingDirections>();
    }

    private void Update()
    {
        // Track airborne time (needed for down-attack delay)
        if (!touchingDirections.IsGrounded)
            jumpTimer += Time.deltaTime;
        else
            jumpTimer = 0f;

        // Cancel down-attack state upon landing
        if (touchingDirections.IsGrounded)
        {
            animator.SetBool(AnimationStrings.downAttack, false);
            if (downAttackHitbox != null)
                downAttackHitbox.GetComponent<Collider2D>().enabled = false;
        }

        HandleAnimations();
        HandleColliders();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * CurrentMoveSpeed, rb.linearVelocity.y);

        // Variable-height jump: apply extra upward force while button is held
        if (isJumpHeld && jumpHoldTimer > 0f && rb.linearVelocity.y > 0f)
        {
            rb.AddForce(Vector2.up * jumpHoldForce, ForceMode2D.Force);
            jumpHoldTimer -= Time.fixedDeltaTime;
        }

        animator.SetFloat(AnimationStrings.yVelocity, rb.linearVelocity.y);
    }

    // ─── Input System callbacks ───────────────────────────────────────────────────

    public void OnMove(InputAction.CallbackContext context)
    {
        if (isPaused) { moveInput = Vector2.zero; IsMoving = false; return; }
        moveInput = context.ReadValue<Vector2>();
        IsMoving  = moveInput != Vector2.zero;
        SetFacingDirection(moveInput);
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)   IsRunning = true;
        if (context.canceled)  IsRunning = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && touchingDirections.IsGrounded && CanMove && !isPaused)
        {
            animator.SetTrigger(AnimationStrings.jumpTrigger);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpImpulse);
            isJumpHeld    = true;
            jumpHoldTimer = maxJumpHoldTime;
            jumpTimer     = 0f;
        }

        // Jump-cut: releasing early makes the jump shorter and snappier
        if (context.canceled)
        {
            isJumpHeld = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    /// <summary>
    /// Crouch (hold, grounded) or Down Attack (tap, airborne after delay).
    /// Bind this to the same "Crouch / Down" action in your Input Actions asset.
    /// </summary>
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (isPaused) return;

        if (context.started)
        {
            if (!touchingDirections.IsGrounded && jumpTimer >= downAttackDelay && !isAttacking && !isRolling)
            {
                // Airborne + delay met → down attack
                animator.SetBool(AnimationStrings.downAttack, true);
                if (downAttackHitbox != null)
                    downAttackHitbox.GetComponent<Collider2D>().enabled = true;
            }
            else if (touchingDirections.IsGrounded && !isAttacking && !isRolling && !isHurt)
            {
                // Grounded → start crouch
                isCrouching = true;
            }
        }

        if (context.canceled)
            isCrouching = false;
    }

    /// <summary>
    /// Slash attack on ground; falls back to animation trigger in the air.
    /// </summary>
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (isPaused || !context.started) return;

        if (!isAttacking && !isRolling && touchingDirections.IsGrounded)
        {
            animator.SetTrigger(AnimationStrings.attackTrigger);
        }
    }

    /// <summary>
    /// Dodge roll. Bind to its own action (e.g. Space / South button).
    /// </summary>
    public void OnRoll(InputAction.CallbackContext context)
    {
        if (isPaused || !context.started) return;
        if (!isAttacking && !isRolling && touchingDirections.IsGrounded)
        {
            isCrouching = false;
            StartCoroutine(Roll());
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        SetPaused(!isPaused);
    }

    // ─── Attack coroutines ───────────────────────────────────────────────────────

    private IEnumerator SlashAttack()
    {
        isAttacking = true;
        animator.SetBool(AnimationStrings.isSlashing, true);

        float dir = IsFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * 1.5f, rb.linearVelocity.y);
        if (slashHitbox != null) slashHitbox.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        rb.linearVelocity = new Vector2(dir * 0.75f, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.1f);

        if (slashHitbox != null) slashHitbox.SetActive(false);
        rb.linearVelocity = new Vector2(dir * 0.25f, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.375f);

        animator.SetBool(AnimationStrings.isSlashing, false);
        isAttacking = false;
    }

    private IEnumerator Roll()
    {
        isRolling = true;
        animator.SetBool(AnimationStrings.isRolling, true);

        float dir = IsFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * 4f, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.3f);

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.2f);

        animator.SetBool(AnimationStrings.isRolling, false);
        isRolling = false;
    }

    /// <summary>
    /// Call this from an enemy/ground collision to bounce the player upward after a down attack.
    /// </summary>
    public void DownAttackBounce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, downAttackBounceForce);
        animator.SetBool(AnimationStrings.downAttack, true);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────────

    private void SetFacingDirection(Vector2 input)
    {
        if (input.x > 0f && !IsFacingRight) IsFacingRight = true;
        else if (input.x < 0f && IsFacingRight) IsFacingRight = false;
    }

    private void HandleAnimations()
    {
        animator.SetFloat(AnimationStrings.speed, Mathf.Abs(moveInput.x));
        animator.SetBool(AnimationStrings.isRunningLegacy, Mathf.Abs(moveInput.x) > 0.1f);
        animator.SetBool(AnimationStrings.isJumping,       !touchingDirections.IsGrounded);
        animator.SetBool(AnimationStrings.isFalling,       rb.linearVelocity.y < 0f);
        animator.SetBool(AnimationStrings.isCrouching,     isCrouching);

        // Keep slash hitbox on the correct side regardless of facing
        if (slashHitbox != null)
        {
            Vector3 pos = slashHitbox.transform.localPosition;
            pos.x = IsFacingRight ? Mathf.Abs(pos.x) : -Mathf.Abs(pos.x);
            slashHitbox.transform.localPosition = pos;
        }
    }

    private void HandleColliders()
    {
        if (standingCollider != null && crouchingCollider != null)
        {
            standingCollider.enabled  = !isCrouching;
            crouchingCollider.enabled =  isCrouching;
        }
    }

    // ─── Pause / Death / Scene management ────────────────────────────────────────

    private void SetPaused(bool paused)
    {
        isPaused = paused;
        if (pausePanel != null) pausePanel.SetActive(paused);
        if (pauseStopsTime)
        {
            if (paused) { prevTimeScale = Time.timeScale; Time.timeScale = 0f; }
            else          Time.timeScale = prevTimeScale;
        }
    }

    public void ResumeGame() => SetPaused(false);

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // always reset before scene switch
        SceneManager.LoadScene("Main Menu");
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnDeath()
    {
        animator.SetBool(AnimationStrings.deathTrigger, true);
        animator.SetBool(AnimationStrings.canMove,      false);
    }
}