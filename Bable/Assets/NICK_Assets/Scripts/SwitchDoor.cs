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
        if (doorCollider != null) doorCollider.enabled = false;
        doorAnimator.SetTrigger("Open");
        yield return null;
    }

    IEnumerator CloseSequence()
    {
        isOpen = false;
        doorAnimator.SetTrigger("Close");
        yield return new WaitForSeconds(GetAnimationLength("Closing"));
        if (doorCollider != null) doorCollider.enabled = true;
    }

    float GetAnimationLength(string stateName)
    {
        if (doorAnimator == null) return 0.5f;
        RuntimeAnimatorController ac = doorAnimator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == stateName)
                return clip.length;
        }
        return 0.5f;
    }
}