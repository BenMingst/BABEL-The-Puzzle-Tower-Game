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

    [Header("Attack")]
    public float slashDuration = 0.575f;
    public GameObject slashHitbox;
    public GameObject downAttackHitbox;
    public float downAttackBounceForce = 8f;

    [Header("Down Attack")]
    public float downAttackDelay = 0.15f;
    private float jumpTimer = 0f;

    private Rigidbody2D rb;
    private Animator animator;
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
    }

    void Update()
    {
        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A)) horizontalInput = -1f;
        if (Input.GetKey(KeyCode.D)) horizontalInput =  1f;

        isCrouching = Input.GetKey(KeyCode.S) && isGrounded;

        if (Input.GetKeyDown(KeyCode.W) && isGrounded && !isCrouching)
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

        if (Input.GetKeyDown(KeyCode.S) && !isGrounded && jumpTimer >= downAttackDelay)
        {
                Debug.Log("Down attack activated");

            animator.SetBool("DownAttack", true);
            downAttackHitbox.GetComponent<Collider2D>().enabled = true;
        }
        else if (isGrounded)
        {
            animator.SetBool("DownAttack", false);
            downAttackHitbox.GetComponent<Collider2D>().enabled = false;
        }

        if (Input.GetMouseButtonDown(0) && !isAttacking && !isRolling && isGrounded)
        {
            StartCoroutine(SlashAttack());
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking && !isRolling && isGrounded)
        {
            StartCoroutine(Roll());
        }

        HandleAnimations();
        HandleColliders();
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float speed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;

        if (horizontalInput != 0 && !isAttacking && !isRolling && !isHurt)
        {
            rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
        }
        else if (!isAttacking && !isRolling && !isHurt)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    IEnumerator SlashAttack()
{
    isAttacking = true;
    animator.SetBool("IsSlashing", true);

    float lungeDirection = facingRight ? 1f : -1f;
    rb.linearVelocity = new Vector2(lungeDirection * 1.5f, rb.linearVelocity.y);

    slashHitbox.SetActive(true);

    yield return new WaitForSeconds(0.1f);

    // check for breakable vases in slash hitbox area
    Collider2D hitboxCol = slashHitbox.GetComponent<Collider2D>();
    if (hitboxCol != null)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            slashHitbox.transform.position,
            hitboxCol.bounds.size,
            0f);

        foreach (var hit in hits)
        {
            BreakableVase vase = hit.GetComponentInParent<BreakableVase>();
            if (vase != null && !vase.IsBroken())
                vase.Break();
        }
    }

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
    }

    public void DownAttackBounce()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, downAttackBounceForce);
        animator.SetBool("DownAttack", true);
    }

    void HandleAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
        animator.SetBool("Is_running", Mathf.Abs(horizontalInput) > 0.1f);
        animator.SetFloat("Horizontal_Velocity", horizontalInput);
        animator.SetBool("FacingRight", facingRight);
        animator.SetBool("IsJumping", !isGrounded);
        animator.SetBool("IsFalling", rb.linearVelocity.y < 0);

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
        if (standingCollider != null && crouchingCollider != null)
        {
            standingCollider.enabled  = !isCrouching;
            crouchingCollider.enabled =  isCrouching;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}