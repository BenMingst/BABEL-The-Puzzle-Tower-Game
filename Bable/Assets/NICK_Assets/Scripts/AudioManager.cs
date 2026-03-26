using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioClip[] buttonSounds;

    public float volume = 1f;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlayButtonSound()
    {
        if (buttonSounds != null)
        {
            SoundFXManager.instance.PlayRandomSoundFXClip(buttonSounds, transform, 1f, 0f);
        }
    }
}