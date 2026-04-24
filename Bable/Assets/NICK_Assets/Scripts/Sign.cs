using System.Collections;
using UnityEngine;
using TMPro;

public class Sign : MonoBehaviour
{
    public string persistentID;

    [Header("Sprites")]
    public SpriteRenderer signRenderer;
    public Sprite normalSprite;
    public Sprite selectedSprite;

    [Header("Animator")]
    public Animator playerAnimator;

    [Header("Cutscene")]
    public GameObject cutscenePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI promptText;
    [TextArea] public string[] dialogueLines;

    private bool playerNearby = false;
    public bool inCutscene = false;

    void Start()
    {
        cutscenePanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = true;
        signRenderer.sprite = selectedSprite;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        signRenderer.sprite = normalSprite;
    }

    void Update()
    {
        if (playerNearby && !inCutscene && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(ReadSign());
        }
    }

    IEnumerator ReadSign()
    {
        inCutscene = true;
        Time.timeScale = 0f;

        if (playerAnimator != null)
        {
            PlayerController pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

            playerAnimator.runtimeAnimatorController = pc.noSwordAnimator;
            playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

            playerAnimator.ResetTrigger("ReadSign");
            playerAnimator.ResetTrigger("ReadSignEnd");
            playerAnimator.SetTrigger("ReadSign");

            yield return new WaitForSecondsRealtime(0.5f);
        }

        cutscenePanel.SetActive(true);
        SoundManager.instance.PlayUIClip(SoundManager.instance.dialogueConfirmSound, 1f);
        int currentLine = 0;
        dialogueText.text = dialogueLines[currentLine];

        Coroutine flashCoroutine = StartCoroutine(FlashPrompt());

        while (currentLine < dialogueLines.Length)
        {
            yield return null;
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentLine++;
                if (currentLine < dialogueLines.Length)
                    dialogueText.text = dialogueLines[currentLine];
                else
                    break;
                // play dialogue confirm sound
                SoundManager.instance.PlayUIClip(SoundManager.instance.dialogueConfirmSound, 1f);
            }
        }

        StopCoroutine(flashCoroutine);
        promptText.enabled = true;
        cutscenePanel.SetActive(false);

        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("ReadSignEnd");
            yield return new WaitForSecondsRealtime(0.3f);
            playerAnimator.updateMode = AnimatorUpdateMode.Normal;
            InventoryManager.Instance.SelectCurrentSlot();
        }

        Time.timeScale = 1f;

        yield return null;
        yield return null;

        inCutscene = false;

        signRenderer.sprite = playerNearby ? selectedSprite : normalSprite;
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
