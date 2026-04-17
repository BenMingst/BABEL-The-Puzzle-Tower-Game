using System.Collections;
using UnityEngine;

public class DestructibleBlock : MonoBehaviour
{
    [Header("Animation")]
    public Animator blockAnimator;

    [Header("Settings")]
    public string explosionAnimationName = "Explode";

    private bool isDestroyed = false;

    public void Destroy()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        StartCoroutine(DestroySequence());
    }

    IEnumerator DestroySequence()
    {
        // disable all colliders immediately so player can pass through
        Collider2D[] cols = GetComponents<Collider2D>();
        foreach (Collider2D col in cols)
            col.enabled = false;

        // play explosion animation
        if (blockAnimator != null)
        {
            blockAnimator.SetTrigger("Explode");
            yield return new WaitForSeconds(GetAnimationLength(explosionAnimationName));
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        Destroy(gameObject);
    }

    float GetAnimationLength(string clipName)
    {
        if (blockAnimator == null) return 0.3f;
        RuntimeAnimatorController ac = blockAnimator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 0.3f;
    }
}