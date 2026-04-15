using System.Collections;
using UnityEngine;
using TMPro;

public class LockedDoor : MonoBehaviour
{
    public string persistentID;

    [Header("Animator")]
    public Animator doorAnimator;

    [Header("Collider")]
    public Collider2D doorCollider;

    [Header("Cutscene")]
    public GameObject cutscenePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI promptText;
    [TextArea] public string[] noKeyDialogue;

    private bool playerNearby = false;
    public bool isUnlocked = false;
    public bool inCutscene = false;

    void Start()
    {
        cutscenePanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isUnlocked) return;
        playerNearby = true;
        if (doorAnimator != null)
            doorAnimator.SetBool("Select", true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isUnlocked) return;
        playerNearby = false;
        if (doorAnimator != null)
            doorAnimator.SetBool("Select", false);
    }

    void Update()
    {
        if (!playerNearby || isUnlocked || inCutscene) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (KeyManager.Instance.GetKeyCount() <= 0)
                StartCoroutine(NoKeySequence());
            else
                StartCoroutine(UnlockDoor());
        }
    }

   public void RestoreUnlocked()
{
    isUnlocked = true;
    if (doorAnimator != null)
        doorAnimator.Play("Opened");
    if (doorCollider != null)
        doorCollider.enabled = false;
}

    IEnumerator NoKeySequence()
    {
        inCutscene = true;
        Time.timeScale = 0f;

        cutscenePanel.SetActive(true);
        int currentLine = 0;
        dialogueText.text = noKeyDialogue[currentLine];

        Coroutine flashCoroutine = StartCoroutine(FlashPrompt());

        while (currentLine < noKeyDialogue.Length)
        {
            yield return null;
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentLine++;
                if (currentLine < noKeyDialogue.Length)
                    dialogueText.text = noKeyDialogue[currentLine];
                else
                    break;
            }
        }

        StopCoroutine(flashCoroutine);
        promptText.enabled = true;
        cutscenePanel.SetActive(false);

        Time.timeScale = 1f;

        yield return null;
        yield return null;

        inCutscene = false;
    }

    IEnumerator UnlockDoor()
    {
        isUnlocked = true;
        inCutscene = true;

        KeyManager.Instance.UseKey();

        if (doorAnimator != null)
        {
            doorAnimator.SetBool("Select", false);
            doorAnimator.SetTrigger("Unlock");
        }

        yield return new WaitForSeconds(1f);

        if (doorCollider != null)
            doorCollider.enabled = false;

        inCutscene = false;
    }

    IEnumerator FlashPrompt()
    {
        while (true)
        {
            promptText.enabled = true;
            yield return new WaitForSecondsRealtime(0.5f);
            promptText.enabled = false;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
}