using System.Collections;
using UnityEngine;
using TMPro;

public class Chest : MonoBehaviour
{
    public enum ChestItemType { Sword, Bow, SmallKey, FireArrows, IceArrows }

    [Header("Sprites")]
    public SpriteRenderer chestRenderer;
    public Sprite closedSprite;
    public Sprite selectedSprite;
    public Sprite openedSprite;

    [Header("Item")]
    public ChestItemType itemType;
    public Animator playerAnimator;

    [Header("Cutscene")]
    public GameObject cutscenePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI promptText;
    [TextArea] public string[] dialogueLines;

    private bool playerNearby = false;
    private bool isOpened = false;
    public bool inCutscene = false;

    void Start()
    {
        cutscenePanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isOpened) return;
        playerNearby = true;
        chestRenderer.sprite = selectedSprite;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isOpened) return;
        playerNearby = false;
        chestRenderer.sprite = closedSprite;
    }

    void Update()
    {
        if (playerNearby && !isOpened && !inCutscene && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(OpenChest());
        }
    }

    IEnumerator OpenChest()
    {
        isOpened = true;
        inCutscene = true;

        chestRenderer.sprite = openedSprite;
        Time.timeScale = 0f;

        if (playerAnimator != null)
        {
            Debug.Log("runtimeAnimatorController null: " + (playerAnimator.runtimeAnimatorController == null));
            playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

            if (itemType == ChestItemType.SmallKey ||
                itemType == ChestItemType.FireArrows ||
                itemType == ChestItemType.IceArrows)
                playerAnimator.SetTrigger("ChestOpen");
            else if (itemType == ChestItemType.Sword)
                playerAnimator.SetTrigger("ItemPickup");
            else if (itemType == ChestItemType.Bow)
                playerAnimator.SetTrigger("ChestOpen");
        }
        else
        {
            Debug.Log("playerAnimator is NULL");
        }

        yield return new WaitForSecondsRealtime(1.4f);

        if (playerAnimator != null)
        {
            if (itemType == ChestItemType.SmallKey)
                playerAnimator.SetTrigger("GotKey");
            else if (itemType == ChestItemType.FireArrows)
                playerAnimator.SetTrigger("GotFireArrows");
            else if (itemType == ChestItemType.IceArrows)
                playerAnimator.SetTrigger("GotIceArrows");
            else if (itemType == ChestItemType.Sword)
                playerAnimator.SetTrigger("ItemPickup");
            else if (itemType == ChestItemType.Bow)
                playerAnimator.SetTrigger("BowPickup");
        }

        cutscenePanel.SetActive(true);
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
            }
        }

        StopCoroutine(flashCoroutine);
        promptText.enabled = true;
        cutscenePanel.SetActive(false);

        PlayerController pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        if (itemType == ChestItemType.Sword)
            pc.EquipSword();
        else if (itemType == ChestItemType.Bow)
            pc.EquipBow();
        else if (itemType == ChestItemType.SmallKey)
            KeyManager.Instance.AddKey();
        else if (itemType == ChestItemType.FireArrows)
            ArrowTypeManager.Instance.UnlockFireArrows();
        else if (itemType == ChestItemType.IceArrows)
            ArrowTypeManager.Instance.UnlockIceArrows();

        if (playerAnimator != null)
        {
            playerAnimator.updateMode = AnimatorUpdateMode.Normal;
            if (itemType == ChestItemType.Sword)
                playerAnimator.SetTrigger("ItemPickupEnd");
            else if (itemType == ChestItemType.Bow)
                playerAnimator.SetTrigger("BowPickupEnd");
            else if (itemType == ChestItemType.SmallKey ||
                     itemType == ChestItemType.FireArrows ||
                     itemType == ChestItemType.IceArrows)
                playerAnimator.SetTrigger("ChestOpenEnd");
        }

        Time.timeScale = 1f;

        yield return null;
        yield return null;

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