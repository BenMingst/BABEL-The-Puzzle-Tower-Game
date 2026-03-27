using System.Collections;
using UnityEngine;

public class IceWall : MonoBehaviour
{
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
        {
            StartCoroutine(TransitionToFirstMelt());
        }
        else if (currentState == IceWallState.FirstMelt)
        {
            StartCoroutine(TransitionToMelted());
        }
    }

    IEnumerator TransitionToFirstMelt()
    {
        isTransitioning = true;
        currentState = IceWallState.FirstMelt;

        wallAnimator.SetTrigger("FirstMelt");

        // wait for first melt animation to finish then loop drip
       yield return new WaitForSeconds(0.9f); // match your first melt animation length

        wallAnimator.SetTrigger("Drip");
        isTransitioning = false;
    }

    IEnumerator TransitionToMelted()
    {
        isTransitioning = true;
        currentState = IceWallState.Melted;

        wallAnimator.SetTrigger("Melt");

        // wait for melt animation to finish
        yield return new WaitForSeconds(0.4f); // match your melt animation length

        // remove collider so player can pass through
        if (wallCollider != null)
            wallCollider.enabled = false;

        isTransitioning = false;
    }
}
