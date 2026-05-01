using UnityEngine;
using TMPro;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance;

    [Header("UI")]
    public GameObject keyUI;
    public TextMeshProUGUI keyCountText;

    [SerializeField] private int keyCount = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        keyUI.SetActive(false);
    }

    public void AddKey()
    {
        keyCount++;
        if (keyCount > 0)
            keyUI.SetActive(true);
        UpdateUI();
    }

    public bool UseKey()
    {
        if (keyCount <= 0) return false;
        keyCount--;
        UpdateUI();
        return true;
    }

    public int GetKeyCount()
    {
        return keyCount;
    }

    public void RestoreKeys(int count)
    {
        keyCount = count;
        if (keyCount > 0)
            keyUI.SetActive(true);
        UpdateUI();
    }

    void UpdateUI()
    {
        keyCountText.text = keyCount.ToString();
    }
}