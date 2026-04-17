using System.Collections;
using UnityEngine;

public class BombTypeManager : MonoBehaviour
{
    public static BombTypeManager Instance;

    public enum BombType { Timed, Remote }
    public BombType currentBombType = BombType.Timed;

    [Header("Bomb Type UI")]
    public GameObject bombTypeUIParent;
    public GameObject timedBombUI;
    public GameObject remoteBombUI;

    [Header("Selected Indicators")]
    public GameObject timedSelected;
    public GameObject remoteSelected;

    public bool hasRemoteBomb = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (bombTypeUIParent != null) bombTypeUIParent.SetActive(false);
        if (timedBombUI != null) timedBombUI.SetActive(false);
        if (remoteBombUI != null) remoteBombUI.SetActive(false);
    }

    public void Initialize()
    {
        currentBombType = BombType.Timed;
        RefreshUI();
    }

    public void UnlockRemoteBomb()
    {
        hasRemoteBomb = true;
        if (bombTypeUIParent != null) bombTypeUIParent.SetActive(true);
        if (timedBombUI != null) timedBombUI.SetActive(true);
        if (remoteBombUI != null) remoteBombUI.SetActive(true);
        currentBombType = BombType.Timed;
        RefreshUI();
    }

    void Update()
    {
        if (InventoryManager.Instance == null) return;
        if (!InventoryManager.Instance.IsBombSelected()) return;
        if (!hasRemoteBomb) return;

        if (Input.GetMouseButtonDown(1))
            CycleBombType();
    }

    void CycleBombType()
    {
        currentBombType = currentBombType == BombType.Timed ? BombType.Remote : BombType.Timed;
            Debug.Log("Cycled bomb type to: " + currentBombType);


        if (BombTypeIndicator.Instance != null)
            BombTypeIndicator.Instance.ShowIndicator(currentBombType);

        RefreshUI();
    }

    void RefreshUI()
    {
        if (timedSelected != null) timedSelected.SetActive(currentBombType == BombType.Timed);
        if (remoteSelected != null) remoteSelected.SetActive(currentBombType == BombType.Remote);
    }
}