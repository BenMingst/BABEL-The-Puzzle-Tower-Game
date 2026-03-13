using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
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

    [Header("Down Attack")]
    public float downAttackDelay = 0.15f;
    private float jumpTimer = 0f;

    [Header("Animator Controllers")]
    public RuntimeAnimatorController noSwordAnimator;
    public RuntimeAnimatorController swordAnimator;
    public bool hasSword = false;

    public bool isDead = false;
    private Rigidbody2D rb;
    public Animator animator;
    private bool isGrounded;
    private bool isCrouching;
    private float horizontalInput;
    private bool facingRight = true;
    private bool isAttacking = false;
    public bool isRolling = false;
    public bool isHurt = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = noSwordAnimator;
    }

    public void EquipSword()
    {
        hasSword = true;
        InventoryManager.Instance.AddItem(0, swordAnimator);
    }

    public void SetInsideDropdown(bool value)
    {
        isInsideDropdown = value;
    }

    public void OnDeath()
{
    isDead = true;
    rb.linearVelocity = Vector2.zero;
    rb.gravityScale = 0f;
    rb.constraints = RigidbodyConstraints2D.FreezeAll;
    transform.position = new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z);
    if (facingRight)
        animator.SetTrigger("DeathRight");
        if (DeathScreenEffect.Instance == null)
        Debug.Log("DeathScreenEffect Instance is NULL");
    else
        animator.SetTrigger("DeathLeft");
}

    bool HasRoomToStand()
    {
        if (ceilingCheck == null) return true;
        return !Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, ceilingLayer);
    }

    void Update()
    {
        if (isDead) return;
        if (isHurt) return;

        ItemPickup pickup = FindFirstObjectByType<ItemPickup>();
        if (pickup != null && pickup.inCutscene) return;

        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A)) horizontalInput = -1f;
        if (Input.GetKey(KeyCode.D)) horizontalInput = 1f;

        isCrouching = (Input.GetKey(KeyCode.S) || (!HasRoomToStand() && isGrounded)) && isGrounded && !isAttacking && !isRolling && !isHurt;

        if (Input.GetKeyDown(KeyCode.W) && isGrounded && HasRoomToStand())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpTimer = 0f;
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
            if (!isGrounded && jumpTimer >= downAttackDelay)
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

        if (Input.GetMouseButtonDown(0) && !isAttacking && !isRolling && isGrounded && hasSword)
        {
            isCrouching = false;
            StartCoroutine(SlashAttack());
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
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
        else if (!isAttacking && !isRolling && !isHurt)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    IEnumerator DropDown()
    {
        isDropping = true;

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

    IEnumerator Roll()
    {
        isRolling = true;
        animator.SetBool("IsRolling", true);

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
    }

    void HandleAnimations()
    {
        //animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
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
}