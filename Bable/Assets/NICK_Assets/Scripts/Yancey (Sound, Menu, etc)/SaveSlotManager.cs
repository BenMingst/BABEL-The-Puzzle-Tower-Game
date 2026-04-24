using System;
using UnityEngine;

[Serializable]
public class SaveSlotData
{
    public bool exists;          // true if this slot has been used
    public string lastWeapon;    // "sword" | "bow" | "bomb" | "grapple" | ""
    public string sceneName;     // scene to resume on load
    public int checkpoint;       // furthest checkpoint index
    public bool hasSword;
    public bool hasBow;
    public bool hasBomb;
    public bool hasRemoteBomb;
    public bool hasGrapple;
    public long lastSavedTicks;  // DateTime.Ticks for display
}

public static class SaveSlotManager
{
    public const int SlotCount = 3;
    private const string ActiveSlotKey = "ActiveSaveSlot";


    private static string SlotKey(int index) => $"SaveSlot_{index}";

    public static int ActiveSlot
    {
        get => PlayerPrefs.GetInt(ActiveSlotKey, -1);
        set => PlayerPrefs.SetInt(ActiveSlotKey, value);
    }


    public static SaveSlotData LoadSlot(int index)
    {
        if (index < 0 || index >= SlotCount)
            return new SaveSlotData();

        string json = PlayerPrefs.GetString(SlotKey(index), "");
        if (string.IsNullOrEmpty(json))
            return new SaveSlotData();          // empty slot

        try { return JsonUtility.FromJson<SaveSlotData>(json); }
        catch { return new SaveSlotData(); }
    }

    public static void SaveSlot(int index, SaveSlotData data)
    {
        if (index < 0 || index >= SlotCount) return;
        data.exists = true;
        data.lastSavedTicks = DateTime.Now.Ticks;
        PlayerPrefs.SetString(SlotKey(index), JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static void DeleteSlot(int index)
    {
        if (index < 0 || index >= SlotCount) return;
        PlayerPrefs.DeleteKey(SlotKey(index));
        PlayerPrefs.Save();
    }

    public static void WriteActiveSlot(string sceneName, int checkpoint)
    {
        int slot = ActiveSlot;
        if (slot < 0 || slot >= SlotCount) return;

        // Figure out which weapon was most recently obtained
        string lastWeapon = "";
        if (GameManager.hasGrapple) lastWeapon = "grapple";
        else if (GameManager.hasBomb) lastWeapon = "bomb";
        else if (GameManager.hasBow) lastWeapon = "bow";
        else if (GameManager.hasSword) lastWeapon = "sword";

        var data = new SaveSlotData
        {
            sceneName = sceneName,
            checkpoint = checkpoint,
            lastWeapon = lastWeapon,
            hasSword = GameManager.hasSword,
            hasBow = GameManager.hasBow,
            hasBomb = GameManager.hasBomb,
            hasRemoteBomb = GameManager.hasRemoteBomb,
            hasGrapple = GameManager.hasGrapple,
        };

        SaveSlot(slot, data);
    }

    public static void ApplySlotToGameManager(SaveSlotData data)
    {
        GameManager.hasSword = data.hasSword;
        GameManager.hasBow = data.hasBow;
        GameManager.hasBomb = data.hasBomb;
        GameManager.hasRemoteBomb = data.hasRemoteBomb;
        GameManager.hasGrapple = data.hasGrapple;
        GameManager.furthestCheckpoint = data.checkpoint;
    }
}
