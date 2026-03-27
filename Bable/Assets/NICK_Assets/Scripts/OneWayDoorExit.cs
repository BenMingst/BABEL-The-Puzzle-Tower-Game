using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OneWayDoorExit : MonoBehaviour
{
    [Header("Components")]
    public Animator doorAnimator;

    [Header("Player Exit")]
    public Transform exitSpawnPoint;
    public float fadeInDuration = 0.2f;

    [Header("Entrance Door Reference")]
    public OneWayDoorEntrance entranceDoor;

    [Header("Screen Effect")]
    public CanvasGroup fadeCanvas;
    public float blackFadeDuration = 0.2f;
    public float waitAtBlackDuration = 0.5f;

    [Header("Cutscene")]
    public GameObject cutscenePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI promptText;
    [TextArea] public string[] dialogue;

    private bool playerNearby = false;
    public bool isUsable = false;
    private bool hasBeenOpened = false;
    public bool inCutscene = false;

    void Start()
    {
        cutscenePanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (inCutscene) return;
        playerNearby = true;
        if (isUsable)
            doorAnimator.SetBool("Select2", true);
        else
            doorAnimator.SetBool("Select", true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        if (isUsable)
            doorAnimator.SetBool("Select2", false);
        else
            doorAnimator.SetBool("Select", false);
    }

    void Update()
    {
        if (!playerNearby || inCutscene) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isUsable)
                StartCoroutine(ReturnToEntranceSequence());
            else
                StartCoroutine(ShowPrompt());
        }
    }

    public void SetUsable()
    {
        isUsable = true;
    }

    public IEnumerator ExitSequence(GameObject player, PlayerController pc, Animator playerAnimator, SpriteRenderer playerSprite, CanvasGroup fadeCanvas)
    {
        inCutscene = true;

        player.transform.position = exitSpawnPoint.position;

        if (!hasBeenOpened)
        {
            doorAnimator.SetTrigger("Open");
            hasBeenOpened = true;
        }

        playerAnimator.SetBool("ExitDoor", true);

        yield return new WaitForSecondsRealtime(0.4f);

        if (playerSprite != null)
            playerSprite.enabled = true;

        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvas.alpha = Mathf.Lerp(1f, 0f, t / fadeInDuration);
            yield return null;
        }
        fadeCanvas.alpha = 0f;

        playerAnimator.SetBool("ExitDoor", false);

        yield return new WaitForSecondsRealtime(0.4f);

        playerAnimator.updateMode = AnimatorUpdateMode.Normal;
        pc.isDead = false;
        pc.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        pc.GetComponent<Rigidbody2D>().gravityScale = 1f;

        inCutscene = false;
    }

    IEnumerator ReturnToEntranceSequence()
    {
        inCutscene = true;

        GameObject player = GameObject.FindWithTag("Player");
        PlayerController pc = player.GetComponent<PlayerController>();
        Animator playerAnimator = pc.animator;
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();

        pc.isDead = true;
        pc.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        doorAnimator.SetBool("Select2", false);
        playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        playerAnimator.SetTrigger("EnterDoor");

        yield return new WaitForSecondsRealtime(0.4f);

        if (playerSprite != null)
            playerSprite.enabled = false;

        float t = 0f;
        while (t < blackFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, t / blackFadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = 1f;

        yield return new WaitForSecondsRealtime(waitAtBlackDuration);

        if (entranceDoor != null)
            yield return StartCoroutine(entranceDoor.ReturnExitSequence(player, pc, playerAnimator, playerSprite, fadeCanvas));

        inCutscene = false;
    }

    IEnumerator ShowPrompt()
    {
        inCutscene = true;
        Time.timeScale = 0f;

        cutscenePanel.SetActive(true);
        int currentLine = 0;
        dialogueText.text = dialogue[currentLine];

        IEnumerator flashCoroutine = FlashPrompt();
        StartCoroutine(flashCoroutine);

        while (currentLine < dialogue.Length)
        {
            yield return null;
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentLine++;
                if (currentLine < dialogue.Length)
                    dialogueText.text = dialogue[currentLine];
                else
                    break;
            }
        }

        StopCoroutine(flashCoroutine);
        promptText.enabled = true;
        cutscenePanel.SetActive(false);

        Time.timeScale = 1f;

        yield return null;
        yield return null;

        inCutscene = false;
    }

    IEnumerator FlashPrompt()
    {
        while (true)
        {
            promptText.enabled = true;
            yield return new WaitForSecondsRealtime(0.5f);
            promptText.enabled = false;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
}