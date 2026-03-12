using System.Collections;
using UnityEngine;
using TMPro;

public class ItemPickup : MonoBehaviour
{
    [Header("Cutscene")]
    public GameObject cutscenePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI promptText;
    public Animator playerAnimator;

    [Header("Dialogue")]
    [TextArea] public string[] dialogueLines;

    public bool inCutscene = false;
    private int currentLine = 0;

    void Start()
    {
        cutscenePanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !inCutscene)
        {
            StartCoroutine(StartCutscene());
        }
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

    IEnumerator StartCutscene()
    {
        inCutscene = true;

        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        Time.timeScale = 0f;

        if (playerAnimator != null)
        {
            playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            playerAnimator.SetTrigger("ItemPickup");
        }

        cutscenePanel.SetActive(true);

        currentLine = 0;
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
            }
        }

        StopCoroutine(flashCoroutine);
        promptText.enabled = true;

        PlayerController playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        playerController.EquipSword();

        cutscenePanel.SetActive(false);

        if (playerAnimator != null)
        {
            playerAnimator.updateMode = AnimatorUpdateMode.Normal;
            playerAnimator.SetTrigger("ItemPickupEnd");
        }

        Time.timeScale = 1f;

        yield return null;
        yield return null;

        inCutscene = false;
        Destroy(gameObject);
    }
}