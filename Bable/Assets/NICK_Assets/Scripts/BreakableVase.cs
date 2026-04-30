using System.Collections;
using UnityEngine;

public class BreakableVase : MonoBehaviour
{
    [Header("Settings")]
    public int heartDropCount = 2;
    public float dropSpread = 0.5f;
    public float breakAnimationDuration = 0.6f;

    [Header("References")]
    public Animator animator;
    public Collider2D vaseCollider;
    public GameObject heartDropPrefab;

    [Header("Optional Effects")]
    public GameObject breakEffectPrefab;
    [SerializeField] public AudioClip[] breakSound;

    private bool isBroken = false;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (vaseCollider == null) vaseCollider = GetComponent<Collider2D>();
    }

    public void Break()
    {
            Debug.Log("Vase.Break() called on " + gameObject.name);

        if (isBroken) return;
        isBroken = true;

        StartCoroutine(BreakSequence());
    }

    IEnumerator BreakSequence()
    {
        // disable collisions so player/projectiles don't keep interacting with it
        if (vaseCollider != null)
            vaseCollider.enabled = false;

        // disable all child colliders too (in case there are multiple)
        Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in allColliders)
            col.enabled = false;

        // play break animation
        if (animator != null)
            animator.SetTrigger("Break");
            // play break sound
            SoundManager.instance.PlayWorldRandom(breakSound, transform, 1f);

        // optional break effect (particles, dust cloud, etc.)
        if (breakEffectPrefab != null)
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);

        // wait for break animation to finish
        yield return new WaitForSeconds(breakAnimationDuration);

        // drop hearts
        for (int i = 0; i < heartDropCount; i++)
        {
            if (heartDropPrefab != null)
            {
                Vector3 spawnPos = transform.position + new Vector3(
                    Random.Range(-dropSpread, dropSpread),
                    Random.Range(0f, dropSpread),
                    0f);
                Instantiate(heartDropPrefab, spawnPos, Quaternion.identity);
            }
        }
    }

    public bool IsBroken() => isBroken;
}