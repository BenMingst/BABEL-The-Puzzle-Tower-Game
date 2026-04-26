using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Menu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider soundFXSlider;

    [Header("Resolution & Screen")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle; // Add this reference
    private Resolution[] resolutions;

    public void Start()
    {
        SetupResolutionDropdown();
        LoadSettings();
    }


    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void UpdateSoundFXVolume(float volume)
    {
        audioMixer.SetFloat("SoundFXVolume", volume);
    }
    private void SetupResolutionDropdown()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            // Create the string for the current resolution
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + Mathf.Round((float)resolutions[i].refreshRateRatio.value) + "Hz";

            // only add it to the list if we haven't seen this size yet
            if (!options.Contains(option))
            {
                options.Add(option);
            }

            // we still want to find the index of our current screen size
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = options.Count - 1;
            }
        }

        resolutionDropdown.AddOptions(options);

        // Load saved or default index
        int savedRes = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);

        // Safety check: if the monitor changed, the saved index might be out of range
        if (savedRes >= options.Count) savedRes = currentResolutionIndex;

        resolutionDropdown.value = savedRes;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;

        // Save the setting as an integer (1 for true, 0 for false)
        PlayerPrefs.SetInt("FullscreenPreference", isFullscreen ? 1 : 0);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
    }

    public void LoadSettings()
    {
        // Load Volume
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        soundFXSlider.value = PlayerPrefs.GetFloat("SoundFXVolume", 0.75f);

        // Load Fullscreen Toggle
        if (PlayerPrefs.HasKey("FullscreenPreference"))
        {
            bool isFS = PlayerPrefs.GetInt("FullscreenPreference") == 1;
            Screen.fullScreen = isFS;
            fullscreenToggle.isOn = isFS;
        }
        else
        {
            // Default to whatever the current state is
            fullscreenToggle.isOn = Screen.fullScreen;
        }
    }

    // Call this when closing the menu or clicking "Apply"
    public void SaveSettings()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);

        audioMixer.GetFloat("SoundFXVolume", out float SoundFXVolume);
        PlayerPrefs.SetFloat("SoundFXVolume", SoundFXVolume);

        PlayerPrefs.Save();
    }
}