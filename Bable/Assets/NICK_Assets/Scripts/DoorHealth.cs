using System.Collections;
using UnityEngine;

public class DoorHealth : MonoBehaviour
{
    public string persistentID;

    [SerializeField] private AudioClip[] hurtSounds;
    [SerializeField] private AudioClip[] deathSounds;
    private AudioSource audioSource;
    private int hits = 0;
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    public bool isDestroyed = false;
    private bool isHit = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void TakeHit()
    {
        SoundManager.instance.PlayWorldRandom(hurtSounds, transform, 1f);
        if (isDestroyed || isHit) return;

        hits++;

        if (hits >= 2)
            StartCoroutine(DestroyDoor());
        else
            StartCoroutine(HitSequence());
    }

    public void RestoreDestroyed()
    {
        isDestroyed = true;
        hits = 2;
        if (col != null) col.enabled = false;
        if (rb != null) rb.bodyType = RigidbodyType2D.Static;
        if (animator != null) animator.SetTrigger("Destroyed");
    }

    IEnumerator HitSequence()
    {
        isHit = true;
        animator.SetTrigger("Hit");
        yield return new WaitForSeconds(0.3f);
        isHit = false;
    }

    IEnumerator DestroyDoor()
    {
        isDestroyed = true;
        animator.SetTrigger("Destroyed");
        // play death sound
        SoundManager.instance.PlayWorldRandom(deathSounds, transform, 1f);
        col.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
        yield break;
    }
}