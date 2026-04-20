using UnityEngine;

public class GrappleHead : MonoBehaviour
{
    public float travelSpeed = 20f;
    public float maxRange = 10f;
    public LayerMask groundLayer;

    public enum State { Flying, Hit, Retracting }
    public State state = State.Flying;

    private Vector2 direction;
    private Vector3 startPos;
    private GrappleGlove owner;

    public void Launch(Vector2 dir, GrappleGlove ownerGlove)
    {
        direction = dir.normalized;
        owner = ownerGlove;
        startPos = transform.position;

        // rotate sprite to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        if (state == State.Flying)
        {
            transform.position += (Vector3)(direction * travelSpeed * Time.deltaTime);

            if (Vector3.Distance(startPos, transform.position) >= maxRange)
                StartRetract();
        }
        else if (state == State.Retracting)
        {
            if (owner == null || owner.grappleSpawnPoint == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 target = owner.grappleSpawnPoint.position;
            transform.position = Vector3.MoveTowards(transform.position, target, travelSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.1f)
            {
                owner.OnGrappleRetracted();
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (state != State.Flying) return;

        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            state = State.Hit;
            transform.position = transform.position; // lock in place
            owner.OnGrappleHit(transform.position);
        }
    }

    public void StartRetract()
    {
        state = State.Retracting;
    }
}
