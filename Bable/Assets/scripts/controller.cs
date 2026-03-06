using UnityEngine;

public class controller : MonoBehaviour
{
    public float baseSpeed = 5f;
    private float moveSpeed;
    public float jumpForce = 10f;
    // private bool isGrounded;
    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = baseSpeed;
    }

/*
    public IEnumerator SpeedChange(float newSpeed, float timeInSecs)
    {
        moveSpeed = newSpeed;
        yield return new WaitForSeconds(timeInSecs);
        moveSpeed = baseSpeed;
    }
    */

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        rb.linearVelocity = Time.deltaTime * new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        if (vertical > 0 && Mathf.Approximately(rb.linearVelocity.y, 0))
        {
            rb.AddRelativeForce(Time.deltaTime * new Vector2(0, jumpForce), ForceMode2D.Impulse);
            // rb.linearVelocity = Time.deltaTime * new Vector2(horizontal * moveSpeed, jumpForce);
        }
        /*
        if (isGrounded) {
            if (Input.GetButtonDown("Jump")) {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            }
        }
        */
    }
    /*
    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            isGrounded = false;
        }
    }
    */
}
