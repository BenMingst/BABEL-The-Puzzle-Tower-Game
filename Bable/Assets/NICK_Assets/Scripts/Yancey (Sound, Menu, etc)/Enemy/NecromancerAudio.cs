using UnityEngine;

[System.Serializable]
public class NecromancerAudio : EnemyAudio
{
    [Header("Necromancer Specific")]
    public AudioClip[] summonWindupSounds;
    public AudioClip[] summonCastSounds;
    public AudioClip[] spawnImpactSounds;
    public AudioClip[] barrierBreakSounds;
    public AudioClip[] staggerSounds;
    public AudioClip[] teleportOutSounds;
    public AudioClip[] teleportInSounds;
    public AudioClip[] vulnerableHitSounds;
    public AudioClip[] barrierUpSounds;
}
