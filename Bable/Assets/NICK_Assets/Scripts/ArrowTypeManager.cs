using UnityEngine;

public class ArrowTypeManager : MonoBehaviour
{
    public static ArrowTypeManager Instance;

    public enum ArrowType { Normal, Ice, Fire }
    public ArrowType currentArrowType = ArrowType.Normal;

    [Header("Arrow Type UI")]
    public GameObject arrowTypeUIParent;
    public GameObject normalArrowUI;
    public GameObject fireArrowUI;
    public GameObject iceArrowUI;

    [Header("Selected Indicators")]
    public GameObject normalSelected;
    public GameObject iceSelected;
    public GameObject fireSelected;

    public bool isShooting = false;
    public bool hasFireArrows = false;
    public bool hasIceArrows = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (arrowTypeUIParent != null) arrowTypeUIParent.SetActive(false);
        if (normalArrowUI != null) normalArrowUI.SetActive(false);
        if (fireArrowUI != null) fireArrowUI.SetActive(false);
        if (iceArrowUI != null) iceArrowUI.SetActive(false);
    }

    public void Initialize()
    {
        currentArrowType = ArrowType.Normal;
        RefreshUI();
    }

    public void UnlockFireArrows()
    {
        hasFireArrows = true;
        if (arrowTypeUIParent != null) arrowTypeUIParent.SetActive(true);
        if (normalArrowUI != null) normalArrowUI.SetActive(true);
        if (fireArrowUI != null) fireArrowUI.SetActive(true);
        currentArrowType = ArrowType.Normal;
        RefreshUI();
    }

    public void UnlockIceArrows()
    {
        hasIceArrows = true;
        if (iceArrowUI != null) iceArrowUI.SetActive(true);
        RefreshUI();
    }

    void Update()
    {
        if (InventoryManager.Instance == null) return;
        if (!InventoryManager.Instance.IsBowSelected()) return;
        if (isShooting) return;
        if (!hasFireArrows) return;

        if (Input.GetMouseButtonDown(1))
        {
            CycleArrowType();
        }
    }

    void CycleArrowType()
    {
        if (hasFireArrows && !hasIceArrows)
        {
            currentArrowType = currentArrowType == ArrowType.Normal ? ArrowType.Fire : ArrowType.Normal;
        }
        else if (hasFireArrows && hasIceArrows)
        {
            switch (currentArrowType)
            {
                case ArrowType.Normal:
                    currentArrowType = ArrowType.Fire;
                    break;
                case ArrowType.Fire:
                    currentArrowType = ArrowType.Ice;
                    break;
                case ArrowType.Ice:
                    currentArrowType = ArrowType.Normal;
                    break;
            }
            SoundManager.instance.PlayUIRandom(SoundManager.instance.arrowSwitchTypeSounds, 1f);
        }

        if (ArrowTypeIndicator.Instance != null)
            ArrowTypeIndicator.Instance.ShowIndicator(currentArrowType);

        RefreshUI();
    }

    void RefreshUI()
    {
        if (normalSelected != null) normalSelected.SetActive(currentArrowType == ArrowType.Normal);
        if (iceSelected != null) iceSelected.SetActive(currentArrowType == ArrowType.Ice);
        if (fireSelected != null) fireSelected.SetActive(currentArrowType == ArrowType.Fire);
    }
}