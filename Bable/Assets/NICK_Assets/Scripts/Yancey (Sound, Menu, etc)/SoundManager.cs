using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class SoundManager : MonoBehaviour
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
    [SerializeField] public AudioClip enemyDoorLaugh;
    [SerializeField] public AudioClip doorOpenSound;
    [SerializeField] public AudioClip doorSlideSound;
    [SerializeField] public AudioClip doorSlideLongSound;
    [SerializeField] public AudioClip doorCloseSound;
    [SerializeField] public AudioClip doorEnterSound;
    [SerializeField] public AudioClip[] doorLockedSounds;
    [SerializeField] public AudioClip doorUnlockSound;
    [SerializeField] public AudioClip[] doorHurtSounds;
    [SerializeField] public AudioClip doorBreakSound;
    [SerializeField] public AudioClip iceDoorMeltHalfSound;
    [SerializeField] public AudioClip iceDoorMeltFullSound;
    [SerializeField] public AudioClip chestOpenSound;
    [SerializeField] public AudioClip chestLockedSound;
    [SerializeField] public AudioClip chestUnlockSound;
    [SerializeField] public AudioClip switchSound;
    [SerializeField] public AudioClip switchDoorSound;

    [Header("Other")]
    [SerializeField] public AudioClip doinkSound;
    [SerializeField] public AudioClip itemPickupSound;
    [SerializeField] public AudioClip itemHumSound;
    [SerializeField] public AudioClip detonatorSound;
    [SerializeField] public AudioClip buttonPressSound;



    public static SoundManager instance;

    [Header("Sound FX Object")]
    [SerializeField] public AudioSource soundFXObject;

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private Transform GetPlayerTransform()
{
    GameObject player = GameObject.FindWithTag("Player");
    return player != null ? player.transform : transform;
}

    public AudioSource PlayRandomSoundFXClip(AudioClip[] clips, Transform spawnTransform, float volume, float delay)
{
    if (clips == null || clips.Length == 0 || soundFXObject == null) 
        {
        // debug log to verify why no sound is playing
        Debug.LogWarning("No clips available or soundFXObject is null in PlayRandomSoundFXClip");
        Debug.LogWarning($"Clips array is null: {clips == null}, Clips array length: {clips?.Length}, soundFXObject is null: {soundFXObject == null}");
        // debug log stating where this is called from
        Debug.LogWarning($"PlayRandomSoundFXClip called from: {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name}"); 
        return null;
        }

    int rand = Random.Range(0, clips.Length);

    AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
    audioSource.clip = clips[rand];
    audioSource.volume = volume;
    audioSource.PlayDelayed(delay);
    // debug log to verify which clip is being played
    Debug.Log($"Playing clip: {clips[rand].name} at position {spawnTransform.position} with volume {volume} and delay {delay}");
    Destroy(audioSource.gameObject, audioSource.clip.length + delay);

    return audioSource;
}

    public AudioSource PlaySoundFXClip(AudioClip clip, Transform spawnTransform, float volume, float delay)
    {
        if (clip == null || soundFXObject == null)
            {
            // debug log to verify why no sound is playing
            Debug.Log("Clip is null or soundFXObject is null in PlaySoundFXClip");
            return null;
            }

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.PlayDelayed(delay);
        // debug log to verify which clip is being played
        Debug.Log($"Playing clip: {clip.name} at position {spawnTransform.position} with volume {volume} and delay {delay}");
        Destroy(audioSource.gameObject, clip.length + delay);

        return audioSource;
    }
    public AudioSource PlayWorldClip(AudioClip clip, Transform emitter, float volume = 1f, float delay = 0f)
    {
        return PlaySoundFXClip(clip, emitter, volume, delay);
    }

    public void PlayWorldRandom(AudioClip[] clips, Transform emitter, float volume = 1f, float delay = 0f)
    {
        if (clips != null && clips.Length > 0)
        {
            PlayRandomSoundFXClip(clips, emitter, volume, delay);
        }
    }

    public void PlayUIClip(AudioClip clip, float volume = 1f)
    {
        Transform targetTransform = GetPlayerTransform();
        if (clip != null)
        {
            PlaySoundFXClip(clip, targetTransform, volume, 0f);
        }
        else
        {
            Debug.LogWarning("Attempted to play null UI clip");
        }
    }

    public void PlayUIRandom(AudioClip[] clips, float volume = 1f)
    {
        Transform targetTransform = GetPlayerTransform();
        if (clips != null && clips.Length > 0)
        {
            PlayRandomSoundFXClip(clips, targetTransform, volume, 0f);
        }
        else
        {
            Debug.LogWarning("Attempted to play from null or empty UI clips array");
        }
    }
}
