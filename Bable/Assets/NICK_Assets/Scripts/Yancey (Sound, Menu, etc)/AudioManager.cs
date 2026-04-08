using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public float volume = 1f;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlayButtonSound()
    {
        SoundFXManager.instance.PlayRandomSoundFXClip(SoundFXManager.instance.buttonSounds, transform, 1f, 0f);
    }
    public void PlayMenuEnter()
    {
        SoundFXManager.instance.PlaySoundFXClip(SoundFXManager.instance.menuEnterSound, transform, 1f, 0f);
    }
}