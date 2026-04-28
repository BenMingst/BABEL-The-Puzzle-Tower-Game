using UnityEngine;
using UnityEngine.Audio;

public class PlayerAudio : MonoBehaviour
{
    public static PlayerAudio instance;

    public WalkingSounds walking;
    public MovementSounds movement;
    public CombatSounds combat;
    public HealthSounds health;
    public BombSounds bomb;
    public ArrowSounds arrow;
    public GrappleGloveSounds grappleGlove;

    [System.Serializable]
    public class WalkingSounds
    {
        public AudioClip[] gravelSounds;
        public AudioClip[] stoneSounds;
        public AudioClip[] woodSounds;
        public AudioClip[] metalSounds;
    }

    [System.Serializable]
    public class MovementSounds
    {
        public AudioClip[] jumpSounds;
        public AudioClip[] rollSounds;
        public AudioClip[] dropDownSounds;
    }

    [System.Serializable]
    public class CombatSounds
    {
        public AudioClip[] swordSlashAttackSounds;
        public AudioClip[] swordDownAttackSounds;
        public AudioClip[] swordDownAttackBounceSounds;
        public AudioClip swordDoinkSound;
    }

    [System.Serializable]
    public class HealthSounds
    {
        public AudioClip[] freezeSounds;
        public AudioClip[] burnSounds;
        public AudioClip[] healSounds;
        public AudioClip[] hurtSounds;
        public AudioClip[] deathSounds;
    }

    [System.Serializable]
    public class BombSounds
    {
        public AudioClip[] throwSounds;
        public AudioClip[] explosionSounds;
        public AudioClip detonatorClickSound;
        public AudioClip fuseSound;
    }

    [System.Serializable]
    public class ArrowSounds
    {
        public AudioClip normalShotSound;
        public AudioClip iceShotSound;
        public AudioClip fireShotSound;
        public AudioClip[] hitWallSounds;
        public AudioClip bounceOffSound;
    }

    [System.Serializable]
    public class GrappleGloveSounds
    {
        public AudioClip shootSound;
        public AudioClip retractSound;
        public AudioClip hitSound;
    }
    void Awake()
    {
        instance = this;
    }
}
