using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioClip buttonSound;
    public float volume = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayButtonSound()
    {
        if (buttonSound != null)
            AudioSource.PlayClipAtPoint(buttonSound, Camera.main.transform.position, volume);
        
    }
}