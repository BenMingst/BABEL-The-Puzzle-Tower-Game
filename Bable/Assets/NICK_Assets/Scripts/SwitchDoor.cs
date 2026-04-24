using System.Collections;
using UnityEngine;

public class SwitchDoor : MonoBehaviour
{
    public string persistentID;

    [Header("Animation")]
    public Animator doorAnimator;

    [Header("Settings")]
    public bool startsOpen = false;

    private bool isOpen;
    private Collider2D doorCollider;

    void Start()
    {
        isOpen = startsOpen;
        doorCollider = GetComponent<Collider2D>();

        if (CheckpointManager.Instance != null &&
            CheckpointManager.Instance.savedState.permanentlyOpenedDoors.Contains(persistentID))
        {
            RestorePermanentlyOpened();
            return;
        }

        if (doorAnimator != null)
        {
            if (startsOpen)
            {
                doorAnimator.Play("Opened", 0, 1f);
                if (doorCollider != null) doorCollider.enabled = false;
            }
            else
            {
                doorAnimator.Play("Closed", 0, 1f);
                if (doorCollider != null) doorCollider.enabled = true;
            }
        }
    }

    public void RestorePermanentlyOpened()
    {
        isOpen = true;
        if (doorCollider != null) doorCollider.enabled = false;
        if (doorAnimator != null) doorAnimator.Play("Opened", 0, 1f);
    }

    public void OpenPermanently()
    {
        isOpen = true;
        if (doorCollider != null) doorCollider.enabled = false;
        if (doorAnimator != null) doorAnimator.SetTrigger("Open");
    }

    public void OnSwitchPressed()
    {
        if (isOpen)
            StartCoroutine(CloseSequence());
        else
            StartCoroutine(OpenSequence());
    }

    public void OnSwitchUnpressed()
    {
        if (isOpen)
            StartCoroutine(CloseSequence());
        else
            StartCoroutine(OpenSequence());
    }

    IEnumerator OpenSequence()
    {
        isOpen = true;
        doorAnimator.SetTrigger("Open");

        yield return new WaitForSeconds(GetAnimationLength("opening"));
        if (doorCollider != null) doorCollider.enabled = false;
    }

    IEnumerator CloseSequence()
    {
        isOpen = false;
        if (doorCollider != null) doorCollider.enabled = false;
        doorAnimator.SetTrigger("Close");

        // play door close sound
        SoundManager.instance.PlayWorldClip(SoundManager.instance.doorCloseSound, transform, 1f);

        yield return new WaitForSeconds(GetAnimationLength("closing"));
        if (doorCollider != null) doorCollider.enabled = true;
    }

    float GetAnimationLength(string clipName)
    {
        if (doorAnimator == null) return 0.5f;
        RuntimeAnimatorController ac = doorAnimator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 0.5f;
    }
}