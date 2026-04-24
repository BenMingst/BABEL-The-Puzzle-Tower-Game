using UnityEngine;

public class EnemyAudio : MonoBehaviour
{

    public static EnemyAudio instance;

    [System.Serializable]
    public class UniversalSounds
    {
        public AudioClip[] attackSounds;
        public AudioClip[] hurtSounds;
        public AudioClip deathSound;
    }

    [System.Serializable]
    public class EvilEyeSounds
    {
        public AudioClip bombLaunchSound;
        public AudioClip shieldUpSound;
        public AudioClip shieldDownSound;
    }

    [System.Serializable]
    public class NecromancerSounds
    {
        public AudioClip necromancerSummonSound;
    }

    [System.Serializable]
    public class SerpentSounds
    {
        public AudioClip serpentTauntSound;
    }

    [System.Serializable]
    public class StalkerSounds
    {
        public AudioClip stalkerAppearSound;
        public AudioClip stalkerDisappearSound;
    }

    public UniversalSounds universal;
    public EvilEyeSounds evilEye;
    public NecromancerSounds necromancer;
    public SerpentSounds serpent;

    public StalkerSounds stalker;


    void Awake()
    {
        instance = this;
    }
} 
