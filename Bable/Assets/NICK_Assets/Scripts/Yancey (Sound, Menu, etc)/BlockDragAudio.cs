using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BlockDragAudio : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Audio")]
    public AudioClip blockPushLoop;
    public float minSpeedForSound = 0.15f;
    public float maxSpeedForPitch = 8f;

    [Header("Volume")]
    public float maxVolume = 1f;

    [Header("Pitch")]
    public float pitchRange = 0.25f;
    public float pitchJitterAmount = 0.04f;
    public float pitchJitterSpeed = 2f;

    private AudioSource activeSource;
    private bool isPlaying;

    private float pitchNoiseTime;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        pitchNoiseTime = Random.Range(0f, 100f);
    }

    void Update()
    {
        float speed = rb.linearVelocity.magnitude;

        if (speed > minSpeedForSound)
        {
            if (!isPlaying)
            {
                activeSource = SoundManager.instance.PlayWorldLoop(
                    blockPushLoop,
                    transform,
                    maxVolume
                );

                isPlaying = true;
            }

            UpdateAudio(speed);
        }
        else
        {
            StopAudio();
        }
    }

    void UpdateAudio(float speed)
    {
        if (activeSource == null) return;

        float t = Mathf.InverseLerp(minSpeedForSound, maxSpeedForPitch, speed);

        // volume scaling (optional override if SoundManager doesn't handle it)
        activeSource.volume = Mathf.Lerp(0.1f, maxVolume, t);

        // smooth pitch variation
        pitchNoiseTime += Time.deltaTime * pitchJitterSpeed;
        float noise = Mathf.PerlinNoise(pitchNoiseTime, 0f) - 0.5f;

        float basePitch = 1f + t * pitchRange;
        activeSource.pitch = basePitch + noise * pitchJitterAmount;
    }

    void StopAudio()
    {
        if (!isPlaying) return;
        if (activeSource != null)
        {
            activeSource.Stop();
            Destroy(activeSource.gameObject);
        }
        activeSource = null;
        isPlaying = false;
    }
}