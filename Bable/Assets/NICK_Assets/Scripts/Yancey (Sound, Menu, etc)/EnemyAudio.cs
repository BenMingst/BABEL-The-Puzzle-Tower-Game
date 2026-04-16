using UnityEngine;

public class EnemyAudio : MonoBehaviour
{
    public  static EnemyAudio instance;
    public AudioClip[] attackSounds;
    public AudioClip[] hurtSounds;
    public AudioClip deathSound;
    public AudioClip bombLaunchSound;
    public AudioClip bombExplosionSound;
    public AudioClip shieldUpSound;
    public AudioClip shieldDownSound;
    void Awake()
    {
        instance = this;
    }
}
