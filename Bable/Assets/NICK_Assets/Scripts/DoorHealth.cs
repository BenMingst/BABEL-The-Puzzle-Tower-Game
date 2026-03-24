using System.Collections;
using UnityEngine;

public class DoorHealth : MonoBehaviour
{

    [SerializeField] private AudioClip[] hurtSounds;
    [SerializeField] private AudioClip[] deathSounds;
    private AudioSource audioSource;
    private int hits = 0;
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool isDestroyed = false;
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
        if (isDestroyed || isHit) return;

        // play hit sound
        SoundFXManager.instance.PlayRandomSoundFXClip(hurtSounds, transform, 1f, 0.1f);

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

        // play destroy sound
        SoundFXManager.instance.PlayRandomSoundFXClip(deathSounds, transform, 1f, 0.1f);

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
