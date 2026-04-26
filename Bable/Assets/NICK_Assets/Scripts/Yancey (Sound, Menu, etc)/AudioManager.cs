using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static SoundManager soundManager;
    public float volume = 1f;
    private void Awake()
    {
        if (soundManager == null)
        {
            soundManager = GetComponent<SoundManager>();
        }
    }

    public void PlayButtonSound()
    {
        SoundManager.instance.PlayUIRandom(SoundManager.instance.buttonSounds, 1f);
    }
    public void PlayMenuEnter()
    {
        SoundManager.instance.PlayUIClip(SoundManager.instance.menuEnterSound, 1f);
    }
}