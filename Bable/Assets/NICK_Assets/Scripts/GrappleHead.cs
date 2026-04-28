using UnityEngine;

public class GrappleHead : MonoBehaviour
{

    [Header("Audio")]
    public AudioClip grappleRetractLoop;
    public float maxDistanceForPitch = 10f;

    private AudioSource retractLoopSource;

    public float maxPitchDistance = 10f;
    public float travelSpeed = 20f;
    public float maxRange = 10f;
    public LayerMask groundLayer;
    public LayerMask enemyLayer;
    public LayerMask blockLayer;

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

            Vector3 target = owner.grappleSpawnPoint.position;
            transform.position = Vector3.MoveTowards(transform.position, target, travelSpeed * Time.deltaTime);

            if (retractLoopSource != null)
            {
                float t = Vector2.Distance(transform.position, target);
                retractLoopSource.pitch = Mathf.Lerp(0.9f, 1.3f, t / maxDistanceForPitch);
            }

            if (Vector3.Distance(transform.position, target) < 0.1f)
            {
                StopRetractSound();
                owner.OnGrappleRetracted();
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (state != State.Flying) return;

        // enemy layer
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            GrappleCatchable catchable = other.GetComponentInParent<GrappleCatchable>();
            if (catchable != null && !catchable.IsCaught())
            {
                state = State.Hit;
                owner.OnGrappleHitEnemy(catchable);
                return;
            }
        }

        // block layer
        if (((1 << other.gameObject.layer) & blockLayer) != 0)
        {
            GrappleableBlock block = other.GetComponentInParent<GrappleableBlock>();
            if (block != null && !block.IsBeingPulled())
            {
                block.SetHitPoint(transform.position);
                state = State.Hit;
                owner.OnGrappleHitBlock(block);
                return;
            }
        }

        // breakable vase
        BreakableVase vase = other.GetComponentInParent<BreakableVase>();
        if (vase != null && !vase.IsBroken())
        {
            vase.Break();
            state = State.Hit;
            StartRetract();
            return;
        }

        // ground
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            state = State.Hit;
            owner.OnGrappleHit(transform.position);
        }
    }

    public void StartRetract()
    {
        state = State.Retracting;

        if (retractLoopSource == null && grappleRetractLoop != null)
        {
            retractLoopSource = SoundManager.instance.PlayWorldClip(grappleRetractLoop, transform, 1f);
            retractLoopSource.loop = true;
        }
    }

    private void StopRetractSound()
    {
        if (retractLoopSource != null)
        {
            retractLoopSource.Stop();
            Destroy(retractLoopSource.gameObject);
            retractLoopSource = null;
        }
    }
}