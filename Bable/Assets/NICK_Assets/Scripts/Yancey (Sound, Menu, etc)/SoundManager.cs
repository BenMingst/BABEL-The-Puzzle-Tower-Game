using Unity.VisualScripting;
using UnityEngine;

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

    public static SoundManager instance;

    [Header("Sound FX Object")]
    [SerializeField] private AudioSource soundFXObject;

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

    public void PlayRandomSoundFXClip(AudioClip[] clips, Transform spawnTransform, float volume, float delay)
    {
        if (clips == null || clips.Length == 0 || soundFXObject == null) return;

        int rand = Random.Range(0, clips.Length);

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = clips[rand];
        audioSource.volume = volume;
        audioSource.PlayDelayed(delay);

        Destroy(audioSource.gameObject, audioSource.clip.length + delay);
    }

    public void PlaySoundFXClip(AudioClip clip, Transform spawnTransform, float volume, float delay)
    {
        if (clip == null || soundFXObject == null) return;

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.PlayDelayed(delay);

        Destroy(audioSource.gameObject, clip.length + delay);
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
        Transform targetTransform = GetPlayerTransform();
        PlaySoundFXClip(clip, targetTransform, volume, 0f);
    }

    public void PlayUIRandom(AudioClip[] clips, float volume = 1f)
    {
        Transform targetTransform = GetPlayerTransform();
        PlayRandomSoundFXClip(clips, targetTransform, volume, 0f);
    }
}
