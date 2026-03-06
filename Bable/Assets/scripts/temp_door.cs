using UnityEngine;

public class temp_door : MonoBehaviour
{
    private bool isOpened = false;
    [SerializeField]
    private Sprite closedDoor;
    [SerializeField]
    private Sprite openedDoor;

    private SpriteRenderer renderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isOpened)
        {
            renderer.sprite = openedDoor;
        }
        else
        {
            renderer.sprite = closedDoor;
        }
        
    }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered");
        if(other.gameObject.tag == "Player"/* && Input.GetKeyDown(KeyCode.E)*/)
        {
            isOpened = !isOpened;
            //Do Stuff; 
            //Testing if it works
            Debug.Log("Doored!");
        }
        else
        {
            Debug.Log("close");
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Exited");
    }

}
