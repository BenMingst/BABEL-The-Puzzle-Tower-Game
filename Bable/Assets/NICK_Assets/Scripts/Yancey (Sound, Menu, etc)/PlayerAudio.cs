using UnityEngine;
using UnityEngine.Audio;

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

    [System.Serializable]
    public class WalkingSounds
    {
        public static AudioClip[] gravelSounds;
        public static AudioClip[] stoneSounds;
        public static AudioClip[] woodSounds;
        public static AudioClip[] metalSounds;
    }

    [System.Serializable]
    public class MovementSounds
    {
        public static AudioClip[] jumpSounds;
        public static AudioClip[] rollSounds;
        public static AudioClip[] dropDownSounds;
    }

    [System.Serializable]
    public class CombatSounds
    {
        public static AudioClip[] swordSlashAttackSounds;
        public static AudioClip[] swordDownAttackSounds;
        public static AudioClip[] swordDownAttackBounceSounds;
    }

    [System.Serializable]
    public class HealthSounds
    {
        public static AudioClip[] freezeSounds;
        public static AudioClip[] burnSounds;
        public static AudioClip[] healSounds;
        public static AudioClip[] hurtSounds;
        public static AudioClip[] deathSounds;
    }

    [System.Serializable]
    public class BombSounds
    {
        public static AudioClip[] throwSounds;
        public static AudioClip[] explosionSounds;
    }

    [System.Serializable]
    public class ArrowSounds
    {
        public static AudioClip normalShotSound;
        public static AudioClip iceShotSound;
        public static AudioClip fireShotSound;
        public static AudioClip[] hitWallSounds;
    }

    [System.Serializable]
    public class GrappleGloveSounds
    {
        public static AudioClip shootSound;
        public static AudioClip retractSound;
        public static AudioClip hitSound;
    }
    void Awake()
    {
        instance = this;
    }
}
