using System.Collections;
using UnityEngine;
using TMPro;

public class Chest : MonoBehaviour
{
    public enum ChestItemType { Sword, Bow, SmallKey, FireArrows, IceArrows, Bomb, RemoteBomb, Grapple }

    public string persistentID;

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
    public bool isOpened = false;
    public bool inCutscene = false;

    void Start()
    {
        cutscenePanel.SetActive(false);
    }

    public void RestoreOpened()
    {
        isOpened = true;
        chestRenderer.sprite = openedSprite;
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
            PlayerController pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

            playerAnimator.ResetTrigger("ChestOpen");
            playerAnimator.ResetTrigger("GotKey");
            playerAnimator.ResetTrigger("GotFireArrows");
            playerAnimator.ResetTrigger("GotIceArrows");
            playerAnimator.ResetTrigger("BowPickup");
            playerAnimator.ResetTrigger("BowPickupEnd");
            playerAnimator.ResetTrigger("BombPickup");
            playerAnimator.ResetTrigger("BombPickupEnd");
            playerAnimator.ResetTrigger("RemoteBombPickup");
            playerAnimator.ResetTrigger("RemoteBombPickupEnd");
            playerAnimator.ResetTrigger("GrapplePickup");
            playerAnimator.ResetTrigger("GrapplePickupEnd");
            playerAnimator.ResetTrigger("ItemPickup");
            playerAnimator.ResetTrigger("ItemPickupEnd");
            playerAnimator.ResetTrigger("ChestOpenEnd");

            playerAnimator.runtimeAnimatorController = pc.noSwordAnimator;
            playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

            if (itemType == ChestItemType.SmallKey ||
                itemType == ChestItemType.FireArrows ||
                itemType == ChestItemType.IceArrows)
                playerAnimator.SetTrigger("ChestOpen");
            else if (itemType == ChestItemType.Sword)
                playerAnimator.SetTrigger("ItemPickup");
            else if (itemType == ChestItemType.Bow)
                playerAnimator.SetTrigger("ChestOpen");
            else if (itemType == ChestItemType.Bomb)
                playerAnimator.SetTrigger("ChestOpen");
            else if (itemType == ChestItemType.RemoteBomb)
                playerAnimator.SetTrigger("ChestOpen");
            else if (itemType == ChestItemType.Grapple)
                playerAnimator.SetTrigger("ChestOpen");

            yield return new WaitForSecondsRealtime(1.4f);

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
            else if (itemType == ChestItemType.Bomb)
            {
                Debug.Log("Setting BombPickup trigger - animator controller: " + playerAnimator.runtimeAnimatorController.name);
                playerAnimator.SetTrigger("BombPickup");
            }
            else if (itemType == ChestItemType.RemoteBomb)
            {
                Debug.Log("Setting RemoteBombPickup trigger - animator controller: " + playerAnimator.runtimeAnimatorController.name);
                playerAnimator.SetTrigger("RemoteBombPickup");
            }
            else if (itemType == ChestItemType.Grapple)
            {
                Debug.Log("Setting GrapplePickup trigger - animator controller: " + playerAnimator.runtimeAnimatorController.name);
                playerAnimator.SetTrigger("GrapplePickup");
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
            cutscenePanel.SetActive(false);

            PlayerController pcEquip = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            if (itemType == ChestItemType.Sword)
                pcEquip.EquipSword();
            else if (itemType == ChestItemType.Bow)
                pcEquip.EquipBow();
            else if (itemType == ChestItemType.Bomb)
                pcEquip.EquipBomb();
            else if (itemType == ChestItemType.RemoteBomb)
                pcEquip.EquipRemoteBomb();
            else if (itemType == ChestItemType.Grapple)
                pcEquip.EquipGrapple();
            else if (itemType == ChestItemType.SmallKey)
                KeyManager.Instance.AddKey();
            else if (itemType == ChestItemType.FireArrows)
                ArrowTypeManager.Instance.UnlockFireArrows();
            else if (itemType == ChestItemType.IceArrows)
                ArrowTypeManager.Instance.UnlockIceArrows();

            if (itemType == ChestItemType.Sword)
                playerAnimator.SetTrigger("ItemPickupEnd");
            else if (itemType == ChestItemType.Bow)
                playerAnimator.SetTrigger("BowPickupEnd");
            else if (itemType == ChestItemType.Bomb)
                playerAnimator.SetTrigger("BombPickupEnd");
            else if (itemType == ChestItemType.RemoteBomb)
                playerAnimator.SetTrigger("RemoteBombPickupEnd");
            else if (itemType == ChestItemType.Grapple)
                playerAnimator.SetTrigger("GrapplePickupEnd");
            else if (itemType == ChestItemType.SmallKey ||
                     itemType == ChestItemType.FireArrows ||
                     itemType == ChestItemType.IceArrows)
                playerAnimator.SetTrigger("ChestOpenEnd");

            playerAnimator.updateMode = AnimatorUpdateMode.Normal;

            if (itemType != ChestItemType.Sword &&
                itemType != ChestItemType.Bow &&
                itemType != ChestItemType.Bomb &&
                itemType != ChestItemType.RemoteBomb &&
                itemType != ChestItemType.Grapple)
                InventoryManager.Instance.SelectCurrentSlot();
        }

        Time.timeScale = 1f;

        yield return null;
        yield return null;

        inCutscene = false;

        // update stats
        if (StatManager.Instance != null)
            StatManager.Instance.chestsFound++;
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