using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float fuseTime = 3f;
    public GameObject explosionChild;
    public float explosionDuration = 0.3f;

    public Rigidbody2D rb;
    private bool hasExploded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (explosionChild != null)
            explosionChild.SetActive(false);
    }

    void Start()
    {
        StartCoroutine(FuseSequence());
    }

    public void Launch(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
    }

    IEnumerator FuseSequence()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    void Explode()
    {
        Debug.Log("Explode called - hasExploded: " + hasExploded + " explosionChild null: " + (explosionChild == null));
        if (hasExploded) return;
        hasExploded = true;

        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null) pc.OnBombExploded();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Debug.Log("Bomb SpriteRenderer null: " + (sr == null) + " enabled before: " + (sr != null ? sr.enabled.ToString() : "N/A"));
        if (sr != null) sr.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (explosionChild != null)
        {
            Debug.Log("Starting explosion sequence");
            StartCoroutine(ExplosionSequence());
        }
        else
        {
            Debug.Log("explosionChild is null, destroying");
            Destroy(gameObject);
        }
    }

    IEnumerator ExplosionSequence()
{
    // play explosion sound
    SoundFXManager.instance.PlayRandomSoundFXClip(SoundFXManager.instance.bombExplosionSounds, transform, 1f, 0f);

    // detach from bomb so it doesn't move with it
    explosionChild.transform.SetParent(null);
    
    // freeze position and rotation
    explosionChild.transform.rotation = Quaternion.identity;
    
    explosionChild.SetActive(true);

    yield return new WaitForSeconds(explosionDuration);

    explosionChild.SetActive(false);
    Destroy(explosionChild);
    Destroy(gameObject);
}
}