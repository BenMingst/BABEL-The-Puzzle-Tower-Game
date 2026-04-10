using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer instance;

    public AudioSource loopSource;

    void Awake()
    {
        if (instance != null)
        {
            // A music player already exists — switch track if the clip is different
            if (instance.loopSource.clip != loopSource.clip)
            {
                instance.loopSource.clip = loopSource.clip;
                instance.loopSource.Play();
            }
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (instance == this)
            loopSource.Play();
    }
}
