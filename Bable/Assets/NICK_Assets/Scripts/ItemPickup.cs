using System.Collections;
using UnityEngine;
using TMPro;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType { Sword, Bow }
    public ItemType itemType;

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
    if (itemType == ItemType.Sword)
    {
        Debug.Log("Firing ItemPickup trigger");
        playerAnimator.SetTrigger("ItemPickup");
                // update weaponsFound stat
        GameManager.Instance.weaponsFound++;
    }
    else if (itemType == ItemType.Bow)
    {
        Debug.Log("Firing BowPickup trigger");
        playerAnimator.SetTrigger("BowPickup");
                // update weaponsFound stat
        GameManager.Instance.weaponsFound++;
    }
}

        cutscenePanel.SetActive(true);

        currentLine = 0;
        dialogueText.text = dialogueLines[currentLine];

        SoundManager.instance.PlayUIClip(SoundManager.instance.dialogueBlipSound, 1f);

        Coroutine flashCoroutine = StartCoroutine(FlashPrompt());

        while (currentLine < dialogueLines.Length)
        {
            yield return null;
            if (Input.GetKeyDown(KeyCode.E))
            {
                SoundManager.instance.PlayUIClip(SoundManager.instance.dialogueConfirmSound, 1f);
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
        if (itemType == ItemType.Sword)
            playerController.EquipSword();
        else if (itemType == ItemType.Bow)
            playerController.EquipBow();

        cutscenePanel.SetActive(false);

        if (playerAnimator != null)
        {
            playerAnimator.updateMode = AnimatorUpdateMode.Normal;
            if (itemType == ItemType.Sword)
                playerAnimator.SetTrigger("ItemPickupEnd");
            else if (itemType == ItemType.Bow)
                playerAnimator.SetTrigger("BowPickupEnd");
        }

        Time.timeScale = 1f;

        yield return null;
        yield return null;

        inCutscene = false;
        Destroy(gameObject);
    }
}