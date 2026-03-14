using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeathScreenEffect : MonoBehaviour
{
    public static DeathScreenEffect Instance;

    public Image deathOverlay;
    public TMP_Text gameOverText;
    public CanvasGroup buttonContainer;
    public float flashDuration = 0.1f;
    public float holdDuration = 0.3f;
    public float fadeDuration = 2f;
    public float gameOverFadeDelay = 0.5f;
    public float gameOverFadeDuration = 1f;
    public float buttonFadeDelay = 1f;
    public float buttonFadeDuration = 0.5f;
    public float deathAnimationLength = 3.4f;

    private Color gameOverColor = new Color(94f / 255f, 11f / 255f, 11f / 255f);

    void Awake()
    {
        Instance = this;
    }

    public void PlayDeathEffect()
    {
        deathOverlay.gameObject.SetActive(true);
        deathOverlay.color = new Color(1f, 1f, 1f, 1f);
        StartCoroutine(DeathEffect());
    }

    IEnumerator DeathEffect()
    {
        // flash white
        deathOverlay.gameObject.SetActive(true);
        deathOverlay.color = new Color(1f, 1f, 1f, 1f);

        yield return new WaitForSeconds(holdDuration);

        // fade white out
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / flashDuration);
            deathOverlay.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        deathOverlay.color = new Color(1f, 1f, 1f, 0f);

        // wait for death animation
        yield return new WaitForSeconds(deathAnimationLength);

        // fade to black
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            deathOverlay.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        deathOverlay.color = new Color(0f, 0f, 0f, 1f);

        // wait then fade in game over text
        yield return new WaitForSeconds(gameOverFadeDelay);

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.color = new Color(gameOverColor.r, gameOverColor.g, gameOverColor.b, 0f);

            t = 0f;
            while (t < gameOverFadeDuration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, t / gameOverFadeDuration);
                gameOverText.color = new Color(gameOverColor.r, gameOverColor.g, gameOverColor.b, alpha);
                yield return null;
            }

            gameOverText.color = new Color(gameOverColor.r, gameOverColor.g, gameOverColor.b, 1f);
        }

        // wait then fade in buttons
        yield return new WaitForSeconds(buttonFadeDelay);

        if (buttonContainer != null)
        {
            buttonContainer.gameObject.SetActive(true);
            buttonContainer.alpha = 0f;

            t = 0f;
            while (t < buttonFadeDuration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, t / buttonFadeDuration);
                buttonContainer.alpha = alpha;
                yield return null;
            }

            buttonContainer.alpha = 1f;
        }
    }
}