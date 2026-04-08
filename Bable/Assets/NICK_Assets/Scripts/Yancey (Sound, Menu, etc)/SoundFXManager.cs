using Unity.VisualScripting;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    [Header("Player Sounds")]    

    [SerializeField] public AudioClip[] player_hurtSounds;
    [SerializeField] public AudioClip[] player_deathSounds;
    [SerializeField] public AudioClip[] player_healSounds;
    [SerializeField] public AudioClip[] player_freezeSounds;
    [SerializeField] public AudioClip[] player_burnSounds;
    [SerializeField] public AudioClip[] player_swordSlashAttackSounds;
    [SerializeField] public AudioClip[] player_swordDownAttackSounds;
    [SerializeField] public AudioClip[] player_bowAttackSounds;
    [SerializeField] public AudioClip normalArrowSpawnSound;
    [SerializeField] public AudioClip iceArrowSpawnSound;
    [SerializeField] public AudioClip fireArrowSpawnSound;
    [SerializeField] public AudioClip[] player_bombThrowSounds;
    [SerializeField] public AudioClip[] player_bombExplosionSounds;
    [SerializeField] public AudioClip[] player_walkSounds;
    [SerializeField] public AudioClip[] player_dropDownSounds;
    [SerializeField] public AudioClip[] player_rollSounds;
    [SerializeField] public AudioClip[] player_jumpSounds;

    [Header("UI Sounds")]
    [SerializeField] public AudioClip[] arrowSwitchTypeSounds;
    [SerializeField] public AudioClip[] hotbarSwitchSounds;

    [Header("Dialogue Sounds")]
    [SerializeField] public AudioClip dialogueBlipSound;
    [SerializeField] public AudioClip dialogueConfirmSound;

    [Header("Menu Sounds")]   
    [SerializeField] public AudioClip gameOverSound;
    [SerializeField] public AudioClip pauseSound;
    [SerializeField] public AudioClip unpauseSound;
    [SerializeField] public AudioClip menuEnterSound;
    [SerializeField] public AudioClip menuExitSound;
    [SerializeField] public AudioClip[] buttonSounds;


    [Header("Prop Sounds")]
    [SerializeField] public AudioClip doorOpenSound;
    [SerializeField] public AudioClip doorEnterSound;
    [SerializeField] public AudioClip doorLockedSound;
    [SerializeField] public AudioClip doorUnlockSound;
    [SerializeField] public AudioClip doorHurtSound;
    [SerializeField] public AudioClip doorBreakSound;
    [SerializeField] public AudioClip chestOpenSound;
    [SerializeField] public AudioClip chestLockedSound;
    [SerializeField] public AudioClip chestUnlockSound;
    

    [Header("Enemy Sounds")]
    [Header("Skeleton Sounds")]
    [SerializeField] public AudioClip skeletonAttackSound;
    [SerializeField] public AudioClip skeletonHurtSound;
    [SerializeField] public AudioClip skeletonDeathSound;

    [Header("Archer Sounds")]
    [SerializeField] public AudioClip archerAttackSound;
    [SerializeField] public AudioClip archerHurtSound;
    [SerializeField] public AudioClip archerDeathSound;

    [Header("EvilEye Sounds")]
    [SerializeField] public AudioClip evilEyeBombLaunchSound;
    [SerializeField] public AudioClip evilEyeBombExplosionSound;
    [SerializeField] public AudioClip evilEyeShieldUpSound;
    [SerializeField] public AudioClip evilEyeShieldDownSound;
    [SerializeField] public AudioClip evilEyeHurtSound;
    [SerializeField] public AudioClip evilEyeDeathSound;

    [Header("Stalker Sounds")]
    [SerializeField] public AudioClip stalkerAttackSound;
    [SerializeField] public AudioClip stalkerHurtSound;
    [SerializeField] public AudioClip stalkerDeathSound;
    [SerializeField] public AudioClip stalkerVanishSound;
    [SerializeField] public AudioClip stalkerAppearSound;

    public static SoundFXManager instance;

    [Header("Sound FX Object")]
    [SerializeField] private AudioSource soundFXObject; 

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume, float delay)
    {
        if(audioClip != null)
        {
            int rand;
            // assign a random index to select a clip from the array
            if(audioClip.Length > 1)
            {
            rand = Random.Range(0, audioClip.Length);
            }
            else
            {
                rand = 0;
            }
            // spawn in gameObject
            AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

            // assign the audioClip
            audioSource.clip = audioClip[rand];
            
            // assign the volume
            audioSource.volume = volume;

            // play sound
            audioSource.PlayDelayed(delay);

            // get length of sound FX clip
            float clipLength = audioSource.clip.length;

            // destroy the clip after it is done playing
            Destroy(audioSource.gameObject, clipLength + delay);
        }
        else
        {
            Debug.LogWarning("No audio clips assigned to play.");
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume, float delay)
    {
        if(audioClip != null)
        {
        // spawn in gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign the audioClip
        audioSource.clip = audioClip;
        
        // assign the volume
        audioSource.volume = volume;

        // play sound
        audioSource.PlayDelayed(delay);

        // get length of sound FX clip
        float clipLength = audioSource.clip.length;

        // destroy the clip after it is done playing
        Destroy(audioSource.gameObject, clipLength + delay);
        }
        else
        {
            Debug.LogWarning("No audio clip assigned to play.");
        }
    }
}
