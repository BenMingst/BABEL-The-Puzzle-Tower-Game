using UnityEngine;

public class lever : MonoBehaviour
{
    private bool isOn = false;
    private bool playerIsInRange = false;

    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;

    [SerializeField] private GameObject doorObject;
    private door doorScript;

    private SpriteRenderer renderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        doorScript = doorObject.GetComponent<door>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerIsInRange && Input.GetKeyDown(KeyCode.E))
        {
            toggleState();
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Player")
        {
            playerIsInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.tag == "Player")
        {
            playerIsInRange = false;
        }
    }

    // Changes which sprite to use based on the state of isOn
    void toggleState()
    {
        if (isOn)
        {
            // Cut power
            isOn = false;
            renderer.sprite = offSprite;
            doorScript.closeDoor();
        }
        else
        {
            // Turn on power
            isOn = true;
            renderer.sprite = onSprite;
            doorScript.openDoor();
        }
        
    }
}
