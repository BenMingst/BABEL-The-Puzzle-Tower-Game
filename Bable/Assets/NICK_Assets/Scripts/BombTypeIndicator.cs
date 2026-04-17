using System.Collections;
using UnityEngine;

public class BombTypeIndicator : MonoBehaviour
{
    public static BombTypeIndicator Instance;

    [Header("Indicator GameObjects")]
    public GameObject timedIndicator;
    public GameObject remoteIndicator;

    [Header("Settings")]
    public float displayDuration = 1.5f;

    private Coroutine hideCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (timedIndicator != null) timedIndicator.SetActive(false);
        if (remoteIndicator != null) remoteIndicator.SetActive(false);
    }

    public void ShowIndicator(BombTypeManager.BombType bombType)
    {
        if (timedIndicator != null) timedIndicator.SetActive(false);
        if (remoteIndicator != null) remoteIndicator.SetActive(false);

        switch (bombType)
        {
            case BombTypeManager.BombType.Timed:
                if (timedIndicator != null) timedIndicator.SetActive(true);
                break;
            case BombTypeManager.BombType.Remote:
                if (remoteIndicator != null) remoteIndicator.SetActive(true);
                break;
        }

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (timedIndicator != null) timedIndicator.SetActive(false);
        if (remoteIndicator != null) remoteIndicator.SetActive(false);
    }
}