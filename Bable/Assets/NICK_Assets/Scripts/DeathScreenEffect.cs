using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenEffect : MonoBehaviour
{
    public static DeathScreenEffect Instance;

    public Image deathOverlay;
    public float flashDuration = 0.1f;
    public float holdDuration = 0.2f;
    public float fadeDuration = 1.5f;
    public float deathAnimationLength = 3.4f;

    void Awake()
    {
        Instance = this;
    }

public void PlayDeathEffect()
{
    Debug.Log("PlayDeathEffect called");
    deathOverlay.gameObject.SetActive(true);
    deathOverlay.color = new Color(1f, 1f, 1f, 1f);
    Debug.Log("Overlay should be white now, active: " + deathOverlay.gameObject.activeSelf);
    StartCoroutine(DeathEffect());
}
    IEnumerator DeathEffect()
    {
            Debug.Log("DeathEffect started, overlay: " + deathOverlay.gameObject.name);
    Debug.Log("DeathEffect started");

        // flash white instantly
        deathOverlay.gameObject.SetActive(true);
        deathOverlay.color = new Color(1f, 1f, 1f, 1f);

        // hold white briefly
        yield return new WaitForSeconds(holdDuration);

        // fade white out so player is visible on white background
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / flashDuration);
            deathOverlay.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        deathOverlay.color = new Color(1f, 1f, 1f, 0f);

        // wait for death animation to finish
        yield return new WaitForSeconds(deathAnimationLength);

        // fade entire screen to black
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            deathOverlay.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        deathOverlay.color = new Color(0f, 0f, 0f, 1f);

        // TODO: show game over screen here
        Debug.Log("Game Over");
    }
}
