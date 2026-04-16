using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndDoor : MonoBehaviour
{
    [Header("Sprites")]
    public SpriteRenderer doorRenderer;
    public Sprite normalSprite;
    public Sprite interactableSprite;

    [Header("Scene")]
    public string nextSceneName;

    [Header("Player")]
    public float walkDuration = 0.5f;

    private bool playerNearby = false;
    private bool isTransitioning = false;
    private PlayerController playerController;

    private SceneController sceneController;

    void Start()
    {
        sceneController = FindFirstObjectByType<SceneController>(); 
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = true;
        playerController = other.GetComponent<PlayerController>();
        doorRenderer.sprite = interactableSprite;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        playerController = null;
        doorRenderer.sprite = normalSprite;
    }

    void Update()
    {
        if (playerNearby && !isTransitioning && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(EnterDoor());
        }
    }

    IEnumerator EnterDoor()
    {
        isTransitioning = true;

        // lock player input
        playerController.isDead = true;

        // play door enter sound
        SoundManager.instance.PlayUIClip(SoundManager.instance.doorEnterSound, 1f);

        yield return new WaitForSeconds(walkDuration);

        // reset GameManager checkpoint for new level
        GameManager.furthestCheckpoint = 0;
        GameManager.hasCustomSpawn = false;
        GameManager.spawnPosition = Vector3.zero;

        Time.timeScale = 1f;
        sceneController.loadTransitionScene(nextSceneName);
    }
}