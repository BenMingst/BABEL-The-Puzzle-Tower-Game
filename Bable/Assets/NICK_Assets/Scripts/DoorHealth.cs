using System.Collections;
using UnityEngine;

public class DoorHealth : MonoBehaviour
{
<<<<<<< HEAD
=======
    public string persistentID;
>>>>>>> bd3c9aba7b4cd087c1b0889cfc11d03d329e0a8f

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
        if (isDestroyed || isHit) return;

        // play hit sound
        SoundFXManager.instance.PlayRandomSoundFXClip(hurtSounds, transform, 1f, 0.1f);

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
<<<<<<< HEAD

        // play destroy sound
        SoundFXManager.instance.PlayRandomSoundFXClip(deathSounds, transform, 1f, 0.1f);

        // wait for hit animation to finish
=======
>>>>>>> bd3c9aba7b4cd087c1b0889cfc11d03d329e0a8f
        yield return new WaitForSeconds(0.3f);
        isHit = false;
    }

    IEnumerator DestroyDoor()
    {
        isDestroyed = true;
        animator.SetTrigger("Destroyed");
        col.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
        yield break;
    }
}