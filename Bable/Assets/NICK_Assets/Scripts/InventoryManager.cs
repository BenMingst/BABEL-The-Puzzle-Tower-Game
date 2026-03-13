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
    public Sprite filledSlotSprite;
    public Sprite selectedSlotSprite;

    [Header("Per-Item Sprites (filled/selected per item)")]
    public Sprite[] filledSprites;
    public Sprite[] selectedSprites;

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
        RefreshUI();
    }

    void Update()
    {
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

        SelectSlot(slotIndex);
        RefreshUI();
    }

    void SelectSlot(int index)
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

        RefreshUI();
    }

    void RefreshUI()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i == currentSlot)
            {
                slotImages[i].sprite = slotFilled[i] ? selectedSprites[i] : emptySelectedSprite;
            }
            else
            {
                slotImages[i].sprite = slotFilled[i] ? filledSprites[i] : emptySlotSprite;
            }
        }
    }
}