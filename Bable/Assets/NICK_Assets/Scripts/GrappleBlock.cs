using System.Collections;
using UnityEngine;

public class GrappleableBlock : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D blockRb;

    [Header("Pull Settings")]
    public float pullSpeed = 8f;
    public float stopDistanceFromPlayer = 1f;

    private bool isBeingPulled = false;
    private Vector3 hitOffset; // offset from block center to where the grapple hit

    void Awake()
    {
        if (blockRb == null) blockRb = GetComponent<Rigidbody2D>();
    }

    public void SetHitPoint(Vector3 worldHitPoint)
    {
        // store the hit as a local offset so it moves with the block
        hitOffset = worldHitPoint - transform.position;
    }

    public Vector3 GetAnchorPosition()
    {
        return transform.position + hitOffset;
    }

    public bool IsBeingPulled() => isBeingPulled;

    public IEnumerator PullTowardPlayer(Transform player, LineRenderer lineRenderer, GrappleHead head, System.Func<Vector3> spawnPointGetter)
    {
        isBeingPulled = true;

        while (true)
        {
            if (player == null) break;

            float dx = player.position.x - transform.position.x;
            float distance = Mathf.Abs(dx);

            if (distance <= stopDistanceFromPlayer) break;

            float direction = dx > 0 ? 1f : -1f;
            blockRb.linearVelocity = new Vector2(direction * pullSpeed, blockRb.linearVelocity.y);

            if (lineRenderer != null && head != null)
            {
                head.transform.position = GetAnchorPosition();
                lineRenderer.SetPosition(0, spawnPointGetter());
                lineRenderer.SetPosition(1, GetAnchorPosition());
            }

            yield return null;
        }

        blockRb.linearVelocity = new Vector2(0f, blockRb.linearVelocity.y);
        isBeingPulled = false;
    }
}