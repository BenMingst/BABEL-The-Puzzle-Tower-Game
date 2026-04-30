using UnityEngine;
[System.Serializable]
public class SerpentAudio : EnemyAudio
{
    [Header("Serpent Specific")]
    public AudioClip fireInhale;
    public AudioClip fireExhale;
    public AudioClip[] footsteps;
    public AudioClip serpentTaunt;
    public AudioClip upwardsAttack;
    public AudioClip swallow;
    public AudioClip muffledExplosion;
}
