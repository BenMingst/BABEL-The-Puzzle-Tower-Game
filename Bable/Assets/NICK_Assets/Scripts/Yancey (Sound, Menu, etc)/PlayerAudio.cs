using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public static PlayerAudio instance;

    [Header("Player Sounds")]
    [SerializeField] public AudioClip[] hurtSounds;
    [SerializeField] public AudioClip[] deathSounds;
    [SerializeField] public AudioClip[] healSounds;
    [SerializeField] public AudioClip[] freezeSounds;
    [SerializeField] public AudioClip[] burnSounds;
    [SerializeField] public AudioClip[] swordSlashAttackSounds;
    [SerializeField] public AudioClip[] swordDownAttackSounds;
    [SerializeField] public AudioClip[] bowAttackSounds;
    [SerializeField] public AudioClip normalArrowSpawnSound;
    [SerializeField] public AudioClip iceArrowSpawnSound;
    [SerializeField] public AudioClip fireArrowSpawnSound;
    [SerializeField] public AudioClip[] arrowHitWallSounds;
    [SerializeField] public AudioClip[] bombThrowSounds;
    [SerializeField] public AudioClip[] bombExplosionSounds;
    [SerializeField] public AudioClip[] walkSounds;
    [SerializeField] public AudioClip[] dropDownSounds;
    [SerializeField] public AudioClip[] rollSounds;
    [SerializeField] public AudioClip[] jumpSounds;

    void Awake()
    {
        instance = this;
    }
}
