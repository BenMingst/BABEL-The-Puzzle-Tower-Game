using System.Collections;
using UnityEngine;

public class BombableSkull : MonoBehaviour
{
    public string persistentID;

    [Header("Animation")]
    public Animator skullAnimator;

    [Header("Sprites")]
    public SpriteRenderer skullSpriteRenderer;
    public Sprite bombledState;

    public bool hasBeenBombed = false;

    void Start()
    {
        if (CheckpointManager.Instance != null &&
            CheckpointManager.Instance.savedState.bombledSkulls.Contains(persistentID))
            RestoreBombed();
    }

    public void BombHit()
    {
        if (hasBeenBombed) return;
        hasBeenBombed = true;
        StartCoroutine(BombSequence());
    }

    public void RestoreBombed()
    {
        hasBeenBombed = true;

        if (skullSpriteRenderer != null && bombledState != null)
            skullSpriteRenderer.sprite = bombledState;

        if (skullAnimator != null)
            skullAnimator.enabled = false;

        BombableSkullTrigger trigger = GetComponentInChildren<BombableSkullTrigger>();
        if (trigger != null)
            trigger.GetComponent<Collider2D>().enabled = false;
    }

    IEnumerator BombSequence()
    {
        BombableSkullTrigger trigger = GetComponentInChildren<BombableSkullTrigger>();
        if (trigger != null)
            trigger.GetComponent<Collider2D>().enabled = false;

        if (skullAnimator != null)
            skullAnimator.SetTrigger("Bombed");

        yield return new WaitForSeconds(1f);

        if (skullSpriteRenderer != null && bombledState != null)
            skullSpriteRenderer.sprite = bombledState;

        if (skullAnimator != null)
            skullAnimator.enabled = false;
    }
}