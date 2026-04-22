using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ItemSlotData
{
    public string itemName;
    public RuntimeAnimatorController animatorController;
    public Sprite emptySprite;
    public Sprite filledSprite;
    public Sprite selectedSprite;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Slot UI Images")]
    public Image[] slotImages;

    [Header("Slot Sprites")]
    public Sprite emptySlotSprite;
    public Sprite emptySelectedSprite;

    [Header("Per-Item Sprites (filled/selected per item)")]
    public Sprite[] filledSprites;
    public Sprite[] selectedSprites;

    [Header("Bow UI")]
    public GameObject bowCooldownUI;
    public GameObject arrowTypeUI;

    [Header("Bomb UI")]
    public GameObject bombTypeUI;

    private RuntimeAnimatorController[] slotAnimators = new RuntimeAnimatorController[5];
    private bool[] slotFilled = new bool[5];
    private int currentSlot = -1;

    private PlayerController playerController;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        if (bowCooldownUI != null)
            bowCooldownUI.SetActive(false);
        if (arrowTypeUI != null)
            arrowTypeUI.SetActive(false);
        if (bombTypeUI != null)
            bombTypeUI.SetActive(false);
        RefreshUI();
    }

    void Update()
    {
        // block slot switching during grapple sequence
        GrappleGlove gg = GrappleGlove.Instance;
        if (gg != null && (gg.isGrappling || gg.isBeingPulled)) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
    }

    public void AddItem(int slotIndex, RuntimeAnimatorController animator)
    {
        if (slotIndex < 0 || slotIndex >= 5) return;

        slotAnimators[slotIndex] = animator;
        slotFilled[slotIndex] = true;

        if (slotIndex == 1)
        {
            if (bowCooldownUI != null)
                bowCooldownUI.SetActive(true);
            if (arrowTypeUI != null && ArrowTypeManager.Instance != null)
                ArrowTypeManager.Instance.Initialize();
        }

        if (slotIndex == 2)
        {
            if (BombTypeManager.Instance != null)
                BombTypeManager.Instance.Initialize();
        }

        SelectSlot(slotIndex);
        RefreshUI();
    }

    public bool IsSwordSelected()
    {
        return currentSlot == 0 && slotFilled[0];
    }

    public bool IsBowSelected()
    {
        return currentSlot == 1 && slotFilled[1];
    }

    public bool IsBombSelected()
    {
        return currentSlot == 2 && slotFilled[2];
    }

    public bool IsGrappleSelected()
    {
        return currentSlot == 3 && slotFilled[3];
    }

    public void SelectCurrentSlot()
    {
        SelectSlot(currentSlot);
    }

    public void SelectSlot(int index)
    {
        currentSlot = index;

        if (slotFilled[index] && slotAnimators[index] != null)
        {
            playerController.animator.runtimeAnimatorController = slotAnimators[index];
        }
        else
        {
            playerController.animator.runtimeAnimatorController = playerController.noSwordAnimator;
        }

        playerController.animator.SetBool("FacingRight", playerController.facingRight);

        if (!playerController.facingRight)
            playerController.animator.Play("Idle_Left");
        else
            playerController.animator.Play("Idle_Right");

        RefreshUI();
    }

    void RefreshUI()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] == null) continue;

            if (i == currentSlot)
                slotImages[i].sprite = slotFilled[i] ? selectedSprites[i] : emptySelectedSprite;
            else
                slotImages[i].sprite = slotFilled[i] ? filledSprites[i] : emptySlotSprite;
        }
    }
}