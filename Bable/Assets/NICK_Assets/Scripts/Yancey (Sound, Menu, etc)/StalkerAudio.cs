using UnityEngine;

public class StalkerAudio : MonoBehaviour
{
    public static StalkerAudio instance;
    public AudioClip[] attackSounds;
    public AudioClip[] hurtSounds;
    public AudioClip deathSound;
    public AudioClip bombLaunchSound;
    public AudioClip bombExplosionSound;
    public AudioClip cloakUpSound;
    public AudioClip cloakDownSound;
    void Awake()
    {
        instance = this;
    }
}
