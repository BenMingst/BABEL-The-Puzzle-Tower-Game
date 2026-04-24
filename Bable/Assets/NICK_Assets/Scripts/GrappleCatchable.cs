using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleCatchable : MonoBehaviour
{
    [Header("References")]
    public Animator enemyAnimator;
    public Rigidbody2D enemyRb;
    public MonoBehaviour[] aiScriptsToDisable;
    public Collider2D[] collidersToDisableDuringPull;

    [Header("Animator States")]
    public string idleStateName = "Idle";

    [Header("Line Attach Point")]
    public Transform grappleAnchor;

    [Header("Slow Settings")]
    public float slowDuration = 2f;
    public float slowMultiplier = 0.1f;

    [Header("Stun Indicator")]
    public StunnedIndicator stunnedIndicator;

    private bool isCaught = false;
    private RigidbodyType2D originalBodyType;
    private List<bool> originalColliderStates = new List<bool>();

    void Awake()
    {
        if (enemyAnimator == null) enemyAnimator = GetComponentInChildren<Animator>();
        if (enemyRb == null) enemyRb = GetComponent<Rigidbody2D>();
    }

    public Vector3 GetAnchorPosition()
    {
        return grappleAnchor != null ? grappleAnchor.position : transform.position;
    }

    public void OnGrappleCaught()
    {
        if (isCaught) return;
        isCaught = true;

        foreach (var script in aiScriptsToDisable)
            if (script != null) script.enabled = false;

        if (enemyAnimator != null)
        {
            enemyAnimator.speed = 1f;
            enemyAnimator.Play(idleStateName, 0, 0f);
        }

        if (enemyRb != null)
        {
            originalBodyType = enemyRb.bodyType;
            enemyRb.linearVelocity = Vector2.zero;
            enemyRb.angularVelocity = 0f;
            enemyRb.bodyType = RigidbodyType2D.Kinematic;
        }

        // disable colliders during pull to avoid physics depenetration fighting the pull
        originalColliderStates.Clear();
        if (collidersToDisableDuringPull != null)
        {
            foreach (var col in collidersToDisableDuringPull)
            {
                if (col != null)
                {
                    originalColliderStates.Add(col.enabled);
                    col.enabled = false;
                }
                else
                {
                    originalColliderStates.Add(false);
                }
            }
        }
    }

    public void OnGrappleReleased()
    {
        if (!isCaught) return;
        StartCoroutine(ReleaseSequence());
    }

    IEnumerator ReleaseSequence()
    {
        if (enemyRb != null)
            enemyRb.bodyType = originalBodyType;

        // re-enable colliders
        if (collidersToDisableDuringPull != null)
        {
            for (int i = 0; i < collidersToDisableDuringPull.Length; i++)
            {
                var col = collidersToDisableDuringPull[i];
                if (col != null && i < originalColliderStates.Count)
                    col.enabled = originalColliderStates[i];
            }
        }

        if (enemyAnimator != null)
            enemyAnimator.speed = slowMultiplier;

        foreach (var script in aiScriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = true;
                var field = script.GetType().GetField("grappleSlowMultiplier");
                if (field != null && field.FieldType == typeof(float))
                    field.SetValue(script, slowMultiplier);
            }
        }

        // show stunned indicator flipped based on enemy facing
        if (stunnedIndicator != null)
        {
            bool enemyFacingRight = transform.localScale.x > 0;
            stunnedIndicator.Show(enemyFacingRight);
        }

        yield return new WaitForSeconds(slowDuration);

        if (enemyAnimator != null)
            enemyAnimator.speed = 1f;

        foreach (var script in aiScriptsToDisable)
        {
            if (script != null)
            {
                var field = script.GetType().GetField("grappleSlowMultiplier");
                if (field != null && field.FieldType == typeof(float))
                    field.SetValue(script, 1f);
            }
        }

        isCaught = false;
    }

    public bool IsCaught() => isCaught;
}