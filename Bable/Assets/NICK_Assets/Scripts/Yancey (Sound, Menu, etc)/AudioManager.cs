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
        SoundFXManager.instance.PlayUIRandom(SoundFXManager.instance.buttonSounds, 1f);
    }
    public void PlayMenuEnter()
    {
        SoundFXManager.instance.PlayUIClip(SoundFXManager.instance.menuEnterSound, 1f);
    }
}