using UnityEngine;

public class PlayOneShotBehavior : StateMachineBehaviour
{
    public AudioClip[] soundsToPlay;
    public float volume = 1f;
    public bool playOnEnter = true, playOnExit = false;

    [Tooltip("How many times to play a clip during the animation, evenly spaced.")]
    public int playCount = 0;
    private int playsTriggered = 0;
    private int lastLoop = 0;

    private void PlayClip(Animator animator)
    {
        if (soundsToPlay == null || soundsToPlay.Length == 0)
        {
            Debug.LogWarning("PlayOneShotBehavior: No audio clips assigned!", animator);
            return;
        }

        AudioClip clip = soundsToPlay[Random.Range(0, soundsToPlay.Length)];

        if (clip == null)
        {
            Debug.LogWarning("PlayOneShotBehavior: Null clip in soundsToPlay array.", animator);
            return;
        }

        AudioSource.PlayClipAtPoint(clip, animator.gameObject.transform.position, volume);
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
{
    if (playOnEnter && soundsToPlay != null && soundsToPlay.Length > 0)
        AudioSource.PlayClipAtPoint(soundsToPlay[Random.Range(0, soundsToPlay.Length)], animator.gameObject.transform.position, volume);

    playsTriggered = 0;
    lastLoop = 0;
}

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playCount <= 0) return;

        int currentLoop = (int)stateInfo.normalizedTime;
        if (currentLoop > lastLoop)
        {
            playsTriggered = 0;
            lastLoop = currentLoop;
        }

        float nextThreshold = (playsTriggered + 1f) / (playCount + 1f);

        if (stateInfo.normalizedTime % 1f >= nextThreshold)
        {
            AudioSource.PlayClipAtPoint(soundsToPlay[Random.Range(0, soundsToPlay.Length)], animator.gameObject.transform.position, volume);
            playsTriggered++;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
{
    if (playOnExit && soundsToPlay != null && soundsToPlay.Length > 0)
        AudioSource.PlayClipAtPoint(soundsToPlay[Random.Range(0, soundsToPlay.Length)], animator.gameObject.transform.position, volume);
}
}