using System.Collections;
using UnityEngine;

public class DoorHealth : MonoBehaviour
{
    private int hits = 0;
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool isDestroyed = false;
    private bool isHit = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void TakeHit()
    {
        if (isDestroyed || isHit) return;

        hits++;

        if (hits >= 2)
        {
            StartCoroutine(DestroyDoor());
        }
        else
        {
            StartCoroutine(HitSequence());
        }
    }

    IEnumerator HitSequence()
    {
        isHit = true;
        animator.SetTrigger("Hit");

        // wait for hit animation to finish
        yield return new WaitForSeconds(0.3f);

        isHit = false;
    }

    IEnumerator DestroyDoor()
    {
        isDestroyed = true;
        animator.SetTrigger("Destroyed");

        // remove collision immediately so player can pass through
        col.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;

        // debris animation plays and stays — no Destroy() call
        yield break;
    }
}
