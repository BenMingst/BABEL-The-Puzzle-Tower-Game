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
        // play door locked sound
        SoundManager.instance.PlayWorldRandom(SoundManager.instance.doorLockedSounds, transform, 1f);
        int currentLine = 0;
        dialogueText.text = noKeyDialogue[currentLine];
        
        Coroutine flashCoroutine = StartCoroutine(FlashPrompt());

        // play dialogue blip sound
        SoundManager.instance.PlayWorldClip(SoundManager.instance.dialogueBlipSound, transform, 1f);

        while (currentLine < noKeyDialogue.Length)
        {
            yield return null;
            if (Input.GetKeyDown(KeyCode.E))
            {
                // play dialogue confirm sound
                SoundManager.instance.PlayWorldClip(SoundManager.instance.dialogueConfirmSound, transform, 1f);
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
            // play door unlock/open sound
            SoundManager.instance.PlayWorldClip(SoundManager.instance.doorUnlockSound, transform, 1f);
            doorAnimator.SetBool("Select", false);
            doorAnimator.SetTrigger("Unlock");
            // play door slide sound
            SoundManager.instance.PlayWorldClip(SoundManager.instance.doorSlideSound, transform, 1f, 1.4f);
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