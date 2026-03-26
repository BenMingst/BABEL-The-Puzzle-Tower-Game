using Unity.VisualScripting;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;

    [SerializeField] private AudioSource soundFXObject; 

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume, float delay)
    {
        // assign a random index to select a clip from the array
        int rand = Random.Range(0, audioClip.Length);

        // spawn in gameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // assign the audioClip
        audioSource.clip = audioClip[rand];
        
        // assign the volume
        audioSource.volume = volume;

        // play sound
        audioSource.Play();

        // get length of sound FX clip

        float clipLength = audioSource.clip.length;

        // destroy the clip after it is done playing
        Destroy(audioSource.gameObject, clipLength);
    }
}
