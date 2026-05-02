using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// End-of-game cutscene triggered when the Stalker dies.
/// 1) Stops music immediately
/// 2) Fades in babel logo
/// 3) Waits, then fades screen to black
/// 4) Waits, then loads MainMenu scene
/// </summary>
public class EndCutscene : MonoBehaviour
{
    public static EndCutscene Instance;

    [Header("References")]
    public Image babelLogoImage;
    public Image fadeToBlackImage;
    public AudioSource musicSource;

    [Header("Timing")]
    [Tooltip("Delay after stalker dies before logo starts fading in.")]
    public float preLogoDelay = 2f;
    [Tooltip("How long the babel logo takes to fade in.")]
    public float logoFadeInDuration = 3f;
    [Tooltip("How long the logo stays fully visible before fade-to-black starts.")]
    public float logoHoldDuration = 10f;
    [Tooltip("How long the fade-to-black takes.")]
    public float fadeToBlackDuration = 3f;
    [Tooltip("How long to hold on the black screen before transitioning to MainMenu.")]
    public float blackHoldDuration = 10f;

    [Header("Scene")]
    public string mainMenuSceneName = "MainMenu";

    bool cutsceneStarted;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // make sure both images start fully transparent
        if (babelLogoImage != null)
        {
            Color c = babelLogoImage.color;
            c.a = 0f;
            babelLogoImage.color = c;
        }
        if (fadeToBlackImage != null)
        {
            Color c = fadeToBlackImage.color;
            c.a = 0f;
            fadeToBlackImage.color = c;
        }
    }

    /// <summary>
    /// Called by StalkerAI when the stalker dies.
    /// </summary>
    public void StartCutscene()
    {
        if (cutsceneStarted) return;
        cutsceneStarted = true;
        StartCoroutine(CutsceneSequence());
    }

    IEnumerator CutsceneSequence()
    {
        // 1. stop music immediately
        if (musicSource != null)
            musicSource.Stop();

        // 2. wait before logo appears
        yield return new WaitForSeconds(preLogoDelay);

        // 3. fade in babel logo
        if (babelLogoImage != null)
        {
            float elapsed = 0f;
            Color c = babelLogoImage.color;
            while (elapsed < logoFadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / logoFadeInDuration;
                c.a = Mathf.Lerp(0f, 1f, t);
                babelLogoImage.color = c;
                yield return null;
            }
            c.a = 1f;
            babelLogoImage.color = c;
        }

        // 4. hold on the logo
        yield return new WaitForSeconds(logoHoldDuration);

        // 5. fade to black
        if (fadeToBlackImage != null)
        {
            float elapsed = 0f;
            Color c = fadeToBlackImage.color;
            while (elapsed < fadeToBlackDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeToBlackDuration;
                c.a = Mathf.Lerp(0f, 1f, t);
                fadeToBlackImage.color = c;
                yield return null;
            }
            c.a = 1f;
            fadeToBlackImage.color = c;
        }

        // 6. hold on black
        yield return new WaitForSeconds(blackHoldDuration);

        // 7. transition to main menu
        SceneManager.LoadScene(mainMenuSceneName);
    }
}