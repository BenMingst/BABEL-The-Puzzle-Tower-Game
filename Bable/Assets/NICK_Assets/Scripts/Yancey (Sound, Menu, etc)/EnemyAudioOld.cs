using UnityEngine;

public class EnemyAudioOld : MonoBehaviour
{

    public static EnemyAudioOld instance;

    public EvilEyeSounds evilEye;
    public NecromancerSounds necromancer;
    public SerpentSounds serpent;
    public StalkerSounds stalker;
    public SpiderSounds spider;
    public SkellySounds skelly;
    public ArmoredSkellySounds armoredSkelly;
    public ArcherSounds archer;

    [SerializeField] public AudioClip[] attackSounds;
    [SerializeField] public AudioClip[] hurtSounds;
    [SerializeField] public AudioClip deathSound;
    [SerializeField] public AudioClip[] ineffectiveSounds;
    [SerializeField] public AudioClip[] blockedSounds;



    [System.Serializable]
    public class EvilEyeSounds
    {
        public AudioClip bombLaunchSound;
        public AudioClip shieldUpSound;
        public AudioClip shieldDownSound;
        public AudioClip[] attackSounds;
        public AudioClip[] hurtSounds;
        public AudioClip deathSound;
        public AudioClip[] ineffectiveSounds;
        public AudioClip[] blockedSounds;
    }

    [System.Serializable]
    public class NecromancerSounds
    {
        public AudioClip[] summonWindupSounds;
        public AudioClip[] summonCastSounds;
        public AudioClip[] spawnImpactSounds;
        public AudioClip[] barrierBreakSounds;
        public AudioClip[] staggerSounds;
        public AudioClip[] teleportOutSounds;
        public AudioClip[] teleportInSounds;
        public AudioClip[] vulnerableHitSounds;
        public AudioClip[] barrierUpSounds;
        public AudioClip[] attackSounds;
        public AudioClip[] hurtSounds;
        public AudioClip deathSound;
        public AudioClip[] ineffectiveSounds;
        public AudioClip[] blockedSounds;
    }

    [System.Serializable]
    public class SerpentSounds
    {
        public AudioClip serpentTauntSound;
        public AudioClip[] attackSounds;
        public AudioClip[] hurtSounds;
        public AudioClip deathSound;
        public AudioClip[] ineffectiveSounds;
        public AudioClip[] blockedSounds;
    }

    [System.Serializable]
    public class SpiderSounds
    {
        public AudioClip spiderAppearSound;
        public AudioClip spiderDisappearSound;
        public AudioClip webDropSound;
        public AudioClip[] attackSounds;
        public AudioClip[] hurtSounds;
        public AudioClip deathSound;
        public AudioClip[] ineffectiveSounds;
        public AudioClip[] blockedSounds;
    }

    [System.Serializable]
    public class StalkerSounds
    {
        public AudioClip stalkerAppearSound;
        public AudioClip stalkerDisappearSound;
        public AudioClip[] attackSounds;
        public AudioClip[] hurtSounds;
        public AudioClip deathSound;
        public AudioClip[] ineffectiveSounds;
        public AudioClip[] blockedSounds;
    }

    [System.Serializable]
    public class SkellySounds
    {
        public AudioClip[] attackSounds;
        public AudioClip[] hurtSounds;
        public AudioClip deathSound;
        public AudioClip[] ineffectiveSounds;
        public AudioClip[] blockedSounds;
    }

    [System.Serializable]
    public class ArmoredSkellySounds
    {
        public AudioClip[] attackSounds;
        public AudioClip[] hurtSounds;
        public AudioClip deathSound;
        public AudioClip[] ineffectiveSounds;
        public AudioClip[] blockedSounds;
    }

    [System.Serializable]
    public class ArcherSounds
    {
        public AudioClip[] attackSounds;
        public AudioClip[] hurtSounds;
        public AudioClip deathSound;
        public AudioClip[] ineffectiveSounds;
        public AudioClip[] blockedSounds;
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
    }
} 
