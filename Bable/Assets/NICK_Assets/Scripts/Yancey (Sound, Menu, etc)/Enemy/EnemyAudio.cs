using UnityEngine;

// Base class - attach this directly to enemies that don't need extra sounds
// (Skelly, ArmoredSkelly, Archer), or extend it for enemies with unique sounds.
[System.Serializable]
public class EnemyAudio : MonoBehaviour
{
    [Header("Common Sounds")]
    public AudioClip[] attackSounds;
    public AudioClip[] hurtSounds;
    public AudioClip deathSound;
    public AudioClip[] ineffectiveSounds;
    public AudioClip[] blockedSounds;
}
