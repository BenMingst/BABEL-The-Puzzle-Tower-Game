using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite defaultSprite;
    public Sprite hoverSprite;
    private Image buttonImage;

    void Awake()
    {
        buttonImage = GetComponent<Image>();
    }

    void Start()
    {
        buttonImage.sprite = defaultSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hover enter: " + gameObject.name);
        buttonImage.sprite = hoverSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Hover exit: " + gameObject.name);
        buttonImage.sprite = defaultSprite;
    }
}