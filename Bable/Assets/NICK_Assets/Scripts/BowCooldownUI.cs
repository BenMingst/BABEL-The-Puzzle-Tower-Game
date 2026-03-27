using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BowCooldownUI : MonoBehaviour
{
    public Image[] cooldownOverlays;
    public float cooldownDuration = 1f;

    private int maxArrows = 3;
    private int currentArrows = 3;
    private Coroutine[] cooldownCoroutines;
    private bool[] slotOnCooldown;

    void Start()
    {
        cooldownCoroutines = new Coroutine[maxArrows];
        slotOnCooldown = new bool[maxArrows];
        foreach (Image overlay in cooldownOverlays)
            overlay.fillAmount = 0f;
    }

    public bool HasArrows()
    {
        return currentArrows > 0;
    }

    public void UseArrow()
    {
            Debug.Log("UseArrow called, currentArrows: " + currentArrows);

        if (currentArrows <= 0) return;

        int slotIndex = currentArrows - 1;
        currentArrows--;

        for (int i = slotIndex + 1; i < maxArrows; i++)
        {
            if (slotOnCooldown[i])
            {
                if (cooldownCoroutines[i] != null)
                    StopCoroutine(cooldownCoroutines[i]);
                cooldownOverlays[i].fillAmount = 1f;
            }
        }

        if (cooldownCoroutines[slotIndex] != null)
            StopCoroutine(cooldownCoroutines[slotIndex]);

        cooldownCoroutines[slotIndex] = StartCoroutine(CooldownSlot(slotIndex));
    }

    IEnumerator CooldownSlot(int index)
    {
        slotOnCooldown[index] = true;
        cooldownOverlays[index].fillAmount = 1f;

        float t = 0f;
        while (t < cooldownDuration)
        {
            t += Time.deltaTime;
            cooldownOverlays[index].fillAmount = Mathf.Lerp(1f, 0f, t / cooldownDuration);
            yield return null;
        }

        cooldownOverlays[index].fillAmount = 0f;
        slotOnCooldown[index] = false;
        currentArrows++;
        cooldownCoroutines[index] = null;

        if (index + 1 < maxArrows && slotOnCooldown[index + 1])
        {
            cooldownCoroutines[index + 1] = StartCoroutine(CooldownSlot(index + 1));
        }
    }

    public void ResetCooldown()
    {
        for (int i = 0; i < maxArrows; i++)
        {
            if (cooldownCoroutines[i] != null)
                StopCoroutine(cooldownCoroutines[i]);
            cooldownOverlays[i].fillAmount = 0f;
            slotOnCooldown[i] = false;
            cooldownCoroutines[i] = null;
        }
        currentArrows = maxArrows;
    }
}