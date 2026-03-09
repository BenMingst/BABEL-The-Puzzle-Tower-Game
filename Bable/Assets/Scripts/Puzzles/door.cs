using UnityEngine;

public class door : MonoBehaviour
{
    private bool isOpened = false;

    [SerializeField] private Sprite closedDoor;
    [SerializeField] private Sprite openedDoor;

    private SpriteRenderer renderer;
    private Collider2D collider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        renderer = this.GetComponent<SpriteRenderer>();
        collider = this.GetComponent<Collider2D>();
        collider.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Changes which sprite to use based on the state of isOpened
    public void toggleState()
    {
        isOpened = !isOpened;
        collider.enabled = !collider.enabled;
        if (isOpened)
        {
            // Close the door
            // isOpened = false;
            renderer.sprite = openedDoor;
            // collider.enabled = true;
        }
        else
        {
            // Open the door
            // isOpened = true;
            renderer.sprite = closedDoor;
            // collider.enabled = false;
        }
        
    }

    public void openDoor()
    {
        collider.enabled = false;
        renderer.sprite = openedDoor;
    }

    public void closeDoor()
    {
        collider.enabled = true;
        renderer.sprite = closedDoor;
    }
}
