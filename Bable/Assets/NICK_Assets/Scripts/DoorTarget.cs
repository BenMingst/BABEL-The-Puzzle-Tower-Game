using System.Collections;
using UnityEngine;

public class DoorTarget : MonoBehaviour
{
    [Header("Animation")]
    public Animator targetAnimator;

    [Header("Linked Door")]
    public SwitchDoor linkedDoor;

    [Header("Settings")]
    public float doorOpenDuration = 3f;

    private bool isHit = false;
    private Coroutine closeCoroutine = null;

    void OnTriggerEnter2D(Collider2D other)
    {
        Arrow arrow = other.GetComponent<Arrow>();
        if (arrow != null && arrow.isPlayerArrow)
        {
            Hit();
        }
    }

    void Hit()
    {
        // reset timer if already open
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        if (!isHit)
        {
            isHit = true;
            if (targetAnimator != null)
                targetAnimator.SetTrigger("Hit");
            if (linkedDoor != null)
                linkedDoor.OnSwitchPressed();
        }

        closeCoroutine = StartCoroutine(CloseAfterDelay());
    }

    IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(doorOpenDuration);

        isHit = false;
        if (targetAnimator != null)
            targetAnimator.SetTrigger("Reset");
        if (linkedDoor != null)
            linkedDoor.OnSwitchUnpressed();

        closeCoroutine = null;
    }
}