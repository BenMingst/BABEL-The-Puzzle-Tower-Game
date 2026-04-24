using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;


public class SaveSlotUI : MonoBehaviour
{
    [Header("Slot Buttons (assign all 3 in order 0-2)")]
    public Button[] slotButtons;

    [Header("Icon Image inside each button")]
    public Image[] slotIcons;

    [Header("Red-X overlay Image inside each button (disabled by default)")]
    public Image[] slotDeleteOverlays;

    [Header("Optional text labels under each slot")]
    public TextMeshProUGUI[] slotLabels;

    [Header("Delete Button")]
    public Button deleteButton;
    public TextMeshProUGUI deleteButtonLabel; // optional

    [Header("Sprites")]
    public Sprite emptySlotSprite;
    public Sprite plusIconSprite;
    public Sprite swordSprite;
    public Sprite bowSprite;
    public Sprite bombSprite;
    public Sprite grappleSprite;

    [Header("Scene to load for a brand-new game")]
    public string firstSceneName = "another_test";

    // ── State ─────────────────────────────────────────────────────────────────

    private bool deleteMode = false;

    // ─────────────────────────────────────────────────────────

    void Awake()
    {
        // Wire slot buttons
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] == null) continue;
            int captured = i;
            slotButtons[i].onClick.RemoveAllListeners();
            slotButtons[i].onClick.AddListener(() => OnSlotClicked(captured));

            // Add hover listeners for delete overlay
            AddHoverEvents(slotButtons[i], captured);
        }

        // Wire delete button
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }
    }

    void OnEnable()
    {
        // Always exit delete mode when the panel opens
        deleteMode = false;
        HideAllOverlays();
        UpdateDeleteButtonLabel();
        RefreshUI();
    }

    // ── Hover events ──────────────────────────────────────────────────────────

    private void AddHoverEvents(Button btn, int index)
    {
        var trigger = btn.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = btn.gameObject.AddComponent<EventTrigger>();

        // Pointer Enter
        var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((_) => OnSlotHoverEnter(index));
        trigger.triggers.Add(enterEntry);

        // Pointer Exit
        var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((_) => OnSlotHoverExit(index));
        trigger.triggers.Add(exitEntry);
    }

    private void OnSlotHoverEnter(int index)
    {
        if (!deleteMode) return;

        // Only show overlay on filled slots
        SaveSlotData data = SaveSlotManager.LoadSlot(index);
        if (!data.exists) return;

        if (index < slotDeleteOverlays.Length && slotDeleteOverlays[index] != null)
            slotDeleteOverlays[index].gameObject.SetActive(true);
    }

    private void OnSlotHoverExit(int index)
    {
        if (index < slotDeleteOverlays.Length && slotDeleteOverlays[index] != null)
            slotDeleteOverlays[index].gameObject.SetActive(false);
    }

    // ── Delete button ─────────────────────────────────────────────────────────

    private void OnDeleteButtonClicked()
    {
        PlayButtonSound();
        deleteMode = !deleteMode;

        if (!deleteMode)
            HideAllOverlays();

        UpdateDeleteButtonLabel();
    }

    private void UpdateDeleteButtonLabel()
    {
        if (deleteButtonLabel == null) return;
        deleteButtonLabel.text = deleteMode ? "CANCEL" : "DELETE";
    }

    private void HideAllOverlays()
    {
        foreach (var overlay in slotDeleteOverlays)
            if (overlay != null)
                overlay.gameObject.SetActive(false);
    }

    // ── Slot click ────────────────────────────────────────────────────────────

    public void OnSlotClicked(int index)
    {
        if (deleteMode)
        {
            SaveSlotData data = SaveSlotManager.LoadSlot(index);
            if (!data.exists) return; // can't delete an empty slot

            PlayButtonSound();
            SaveSlotManager.DeleteSlot(index);

            // If this was the active slot, clear it
            if (SaveSlotManager.ActiveSlot == index)
                SaveSlotManager.ActiveSlot = -1;

            HideAllOverlays();
            deleteMode = false;
            UpdateDeleteButtonLabel();
            RefreshUI();
            return;
        }

        // Normal load / new game
        Debug.Log($"[SaveSlotUI] Slot {index} clicked.");
        PlayerController.isPaused = false; // ensure player is unpaused when starting/loading game
        Time.timeScale = 1f; // reset in case we're coming from a paused game

        if (string.IsNullOrEmpty(firstSceneName))
        {
            Debug.LogError("[SaveSlotUI] firstSceneName is empty! Set it in the Inspector.");
            return;
        }

        PlayButtonSound();

        if (SoundManager.instance != null)
            SoundManager.instance.PlayUIClip(SoundManager.instance.menuEnterSound, 1f);

        SaveSlotData slotData = SaveSlotManager.LoadSlot(index);
        SaveSlotManager.ActiveSlot = index;

        if (slotData.exists)
        {
            string scene = string.IsNullOrEmpty(slotData.sceneName) ? firstSceneName : slotData.sceneName;
            Debug.Log($"[SaveSlotUI] Loading existing save → scene: {scene}");
            SaveSlotManager.ApplySlotToGameManager(slotData);
            SceneManager.LoadScene(scene);
        }
        else
        {
            Debug.Log($"[SaveSlotUI] Starting new game → scene: {firstSceneName}");
            GameManager.hasSword       = false;
            GameManager.hasBow         = false;
            GameManager.hasBomb        = false;
            GameManager.hasRemoteBomb  = false;
            GameManager.hasGrapple     = false;
            GameManager.furthestCheckpoint = 0;
            GameManager.hasCustomSpawn = false;

            if (CheckpointManager.Instance != null)
                CheckpointManager.Instance.savedState = new CheckpointState();
            // Write a clean slot to disk NOW before LoadScene so AutoSaveHook
            // cannot overwrite it with stale weapon flags from the previous session.
            SaveSlotManager.SaveSlot(index, new SaveSlotData
            {
                sceneName     = firstSceneName,
                lastWeapon    = "",
                checkpoint    = 0,
                hasSword      = false,
                hasBow        = false,
                hasBomb       = false,
                hasRemoteBomb = false,
                hasGrapple    = false,
            });

            SceneManager.LoadScene(firstSceneName);
        }
    }

    // ── UI Refresh ────────────────────────────────────────────────────────────

    public void RefreshUI()
    {
        int leftmostEmpty = -1;
        for (int i = 0; i < SaveSlotManager.SlotCount; i++)
        {
            if (!SaveSlotManager.LoadSlot(i).exists)
            {
                leftmostEmpty = i;
                break;
            }
        }

        for (int i = 0; i < SaveSlotManager.SlotCount; i++)
        {
            SaveSlotData data = SaveSlotManager.LoadSlot(i);

            if (data.exists)
            {
                SetInteractable(i, true);
                SetIcon(i, WeaponSprite(data.lastWeapon), Color.white);
                SetLabel(i, CapWeapon(data.lastWeapon));
            }
            else if (i == leftmostEmpty)
            {
                SetInteractable(i, true);
                SetIcon(i, plusIconSprite, Color.white);
                SetLabel(i, "New Game");
            }
            else
            {
                SetInteractable(i, false);
                SetIcon(i, emptySlotSprite, new Color(1f, 1f, 1f, 0.35f));
                SetLabel(i, "");
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void PlayButtonSound()
    {
        if (SoundManager.instance != null)
            SoundManager.instance.PlayUIRandom(SoundManager.instance.buttonSounds, 1f);
    }

    private void SetInteractable(int i, bool on)
    {
        if (i < slotButtons.Length && slotButtons[i] != null)
            slotButtons[i].interactable = on;
    }

    private void SetIcon(int i, Sprite sprite, Color color)
    {
        if (i >= slotIcons.Length || slotIcons[i] == null) return;
        slotIcons[i].sprite = sprite != null ? sprite : emptySlotSprite;
        slotIcons[i].color  = color;
    }

    private void SetLabel(int i, string text)
    {
        if (slotLabels == null || i >= slotLabels.Length || slotLabels[i] == null) return;
        slotLabels[i].text = text;
    }

    private Sprite WeaponSprite(string weapon) => weapon switch
    {
        "sword"   => swordSprite,
        "bow"     => bowSprite,
        "bomb"    => bombSprite,
        "grapple" => grappleSprite,
        _         => emptySlotSprite,
    };

    private string CapWeapon(string weapon) => weapon switch
    {
        "sword"   => "Sword",
        "bow"     => "Bow",
        "bomb"    => "Bomb",
        "grapple" => "Grapple",
        _         => "",
    };
}