using UnityEngine;

public class PlayOneShotBehavior : StateMachineBehaviour
{
    public AudioClip[] soundsToPlay;
    [Range(0f, 1f)]
    public float volume = 1f;

    public bool playOnEnter = true;
    public bool playOnExit = false;

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
            Debug.LogWarning("PlayOneShotBehavior: Null clip found in soundsToPlay.", animator);
            return;
        }

        AudioSource.PlayClipAtPoint(clip, animator.transform.position, volume);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playsTriggered = 0;
        lastLoop = 0;

        if (playOnEnter)
            PlayClip(animator);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playCount <= 0)
            return;

        int currentLoop = Mathf.FloorToInt(stateInfo.normalizedTime);

        if (currentLoop > lastLoop)
        {
            playsTriggered = 0;
            lastLoop = currentLoop;
        }

        if (playsTriggered >= playCount)
            return;

        float loopProgress = stateInfo.normalizedTime % 1f;
        float nextThreshold = (playsTriggered + 1f) / (playCount + 1f);

        if (loopProgress >= nextThreshold)
        {
            PlayClip(animator);
            playsTriggered++;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playOnExit)
            PlayClip(animator);
    }
}