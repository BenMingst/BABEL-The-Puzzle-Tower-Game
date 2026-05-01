using System.Collections;
using UnityEngine;

public class IceWall : MonoBehaviour
{
    public string persistentID;

    public enum IceWallState { Default, FirstMelt, Melted }
    public IceWallState currentState = IceWallState.Default;

    [Header("Components")]
    public Animator wallAnimator;
    public Collider2D wallCollider;

    private bool isTransitioning = false;

    public void HitByFireArrow()
    {
        if (isTransitioning) return;

        if (currentState == IceWallState.Default)
            StartCoroutine(TransitionToFirstMelt());
        else if (currentState == IceWallState.FirstMelt)
            StartCoroutine(TransitionToMelted());
    }

    public void RestoreMelted()
{
    currentState = IceWallState.Melted;
    if (wallCollider != null) wallCollider.enabled = false;
    if (wallAnimator != null)
        wallAnimator.Play("puddle");
}

    IEnumerator TransitionToFirstMelt()
    {
        isTransitioning = true;
        currentState = IceWallState.FirstMelt;
        wallAnimator.SetTrigger("FirstMelt");
        // play half melt sound
            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlayWorldClip(SoundManager.instance.iceDoorMeltHalfSound, transform, 1f);
            }
        yield return new WaitForSeconds(0.9f);
        wallAnimator.SetTrigger("Drip");
        isTransitioning = false;
    }

    IEnumerator TransitionToMelted()
    {
        isTransitioning = true;
        currentState = IceWallState.Melted;
        wallAnimator.SetTrigger("Melt");
        // play full melt sound
            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlayWorldClip(SoundManager.instance.iceDoorMeltFullSound, transform, 1f);
            }
        yield return new WaitForSeconds(0.4f);
        if (wallCollider != null) wallCollider.enabled = false;
        isTransitioning = false;
    }
}
