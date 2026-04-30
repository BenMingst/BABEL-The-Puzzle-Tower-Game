using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GrappleCooldownUI : MonoBehaviour
{
    [Header("References")]
    public Image cooldownOverlay;

    [Header("Settings")]
    public float cooldownDuration = 0.5f;

    private Coroutine cooldownCoroutine;
    private bool onCooldown = false;

    void Start()
    {
        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f;
    }

    public bool IsOnCooldown()
    {
        return onCooldown;
    }

    public void StartCooldown()
    {
        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);
        cooldownCoroutine = StartCoroutine(CooldownSequence());
    }

    IEnumerator CooldownSequence()
    {
        onCooldown = true;

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 1f;

        float elapsed = 0f;
        while (elapsed < cooldownDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cooldownDuration;

            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f;

        onCooldown = false;
        cooldownCoroutine = null;
    }
}