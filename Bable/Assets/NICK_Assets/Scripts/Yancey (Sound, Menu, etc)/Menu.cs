using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Slider musicSlider;
    public Slider soundFXSlider;

    public void UpdateMusicVolume(float volume)
    {
        // Update the music volume in the audio manager
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void UpdateSoundFXVolume(float volume)
    {
        // Update the sound effects volume in the audio manager
        audioMixer.SetFloat("SoundFXVolume", volume);
    }

    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);

        audioMixer.GetFloat("SoundFXVolume", out float SoundFXVolume);
        PlayerPrefs.SetFloat("SoundFXVolume", SoundFXVolume);
    }

    public void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        soundFXSlider.value = PlayerPrefs.GetFloat("SoundFXVolume");
    }

}
