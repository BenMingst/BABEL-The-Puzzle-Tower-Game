using System.Collections;
using UnityEngine;

public class DoorHealth : MonoBehaviour
{
    public string persistentID;
    [SerializeField] private AudioClip[] hurtSounds;
    [SerializeField] private AudioClip[] deathSounds;
    private PlayerAudio playerAudio;    
    private int hits = 0;
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    public bool isDestroyed = false;
    private bool isHit = false;

    void Start()
    {
        playerAudio = GetComponent<PlayerAudio>();
        if (playerAudio == null) playerAudio = gameObject.AddComponent<PlayerAudio>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void TakeHit()
    {
        SoundFXManager.instance.PlayWorldRandom(SoundFXManager.instance.doorHurtSounds, transform, 1f, 0f);
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

        // play destroy sound
        SoundFXManager.instance.PlayWorldRandom(playerAudio.deathSounds, transform, 1f, 0.1f);
        yield return new WaitForSeconds(0.3f);
        isHit = false;
    }

    IEnumerator DestroyDoor()
    {
        isDestroyed = true;
        animator.SetTrigger("Destroyed");
        SoundFXManager.instance.PlayWorldClip(SoundFXManager.instance.doorBreakSound, transform, 1f);
        col.enabled = false;
        rb.bodyType = RigidbodyType2D.Static;
        yield break;
    }
}