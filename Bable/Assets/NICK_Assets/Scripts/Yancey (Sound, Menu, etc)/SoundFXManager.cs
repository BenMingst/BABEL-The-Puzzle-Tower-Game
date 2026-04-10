using Unity.VisualScripting;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    [Header("-------- UI SOUNDS --------")]
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

    [Header("-------- WORLD SOUNDS --------")]
    [Header("Prop Sounds")]
    [SerializeField] public AudioClip doorOpenSound;
    [SerializeField] public AudioClip doorEnterSound;
    [SerializeField] public AudioClip[] doorLockedSounds;
    [SerializeField] public AudioClip doorUnlockSound;
    [SerializeField] public AudioClip[] doorHurtSounds;
    [SerializeField] public AudioClip doorBreakSound;
    [SerializeField] public AudioClip chestOpenSound;
    [SerializeField] public AudioClip chestLockedSound;
    [SerializeField] public AudioClip chestUnlockSound;


    [Header("Stalker Sounds")]
    [SerializeField] public AudioClip stalkerVanishSound;
    [SerializeField] public AudioClip stalkerAppearSound;

    public static SoundFXManager instance;

    [Header("Sound FX Object")]
    [SerializeField] private AudioSource soundFXObject;

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume, float delay)
    {
        if (audioClip != null)
        {
            int rand;
            // assign a random index to select a clip from the array
            if (audioClip.Length > 1)
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
        if (audioClip != null)
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
    public void PlayWorldClip(AudioClip clip, Transform emitter, float volume = 1f, float delay = 0f)
    {
        PlaySoundFXClip(clip, emitter, volume, delay);
    }

    public void PlayWorldRandom(AudioClip[] clips, Transform emitter, float volume = 1f, float delay = 0f)
    {
        PlayRandomSoundFXClip(clips, emitter, volume, delay);
    }

    public void PlayUIClip(AudioClip clip, float volume = 1f)
    {
        PlaySoundFXClip(clip, transform, volume, 0f);
    }

    public void PlayUIRandom(AudioClip[] clips, float volume = 1f)
    {
        PlayRandomSoundFXClip(clips, transform, volume, 0f);
    }
}
