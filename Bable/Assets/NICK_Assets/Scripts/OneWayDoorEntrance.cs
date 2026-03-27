using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OneWayDoorEntrance : MonoBehaviour
{
    [Header("Components")]
    public Animator doorAnimator;

    [Header("Exit Door")]
    public OneWayDoorExit exitDoor;

    [Header("Screen Effect")]
    public CanvasGroup fadeCanvas;
    public float blackFadeDuration = 0.2f;
    public float waitAtBlackDuration = 0.5f;

    private bool playerNearby = false;
    private bool isOpened = false;
    private bool isUsable = false;
    private bool hasBeenOpened = false;
    public bool inCutscene = false;

    void Start()
    {
        if (fadeCanvas != null)
            fadeCanvas.alpha = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (inCutscene) return;
        playerNearby = true;
        if (isUsable)
            doorAnimator.SetBool("Select2", true);
        else if (!isOpened)
            doorAnimator.SetBool("Select", true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerNearby = false;
        if (isUsable)
            doorAnimator.SetBool("Select2", false);
        else if (!isOpened)
            doorAnimator.SetBool("Select", false);
    }

    void Update()
    {
        if (!playerNearby || inCutscene) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isOpened)
                StartCoroutine(EnterDoorSequence());
            else if (isUsable)
                StartCoroutine(ReturnToEntranceSequence());
        }
    }

    public void SetUsable()
    {
        isUsable = true;
    }

    IEnumerator EnterDoorSequence()
    {
        inCutscene = true;
        isOpened = true;

        GameObject player = GameObject.FindWithTag("Player");
        PlayerController pc = player.GetComponent<PlayerController>();
        Animator playerAnimator = pc.animator;
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();

        pc.isDead = true;
        pc.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        doorAnimator.SetBool("Select", false);

        if (!hasBeenOpened)
        {
            doorAnimator.SetTrigger("Open");
            hasBeenOpened = true;
            yield return new WaitForSecondsRealtime(0.4f);
        }

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

        if (exitDoor != null)
            yield return StartCoroutine(exitDoor.ExitSequence(player, pc, playerAnimator, playerSprite, fadeCanvas));

        isUsable = true;
        exitDoor.SetUsable();

        inCutscene = false;
    }

    public IEnumerator ReturnExitSequence(GameObject player, PlayerController pc, Animator playerAnimator, SpriteRenderer playerSprite, CanvasGroup fadeCanvas)
    {
        inCutscene = true;

        player.transform.position = transform.position;
        doorAnimator.SetBool("Select2", false);
        playerAnimator.SetBool("ExitDoor", true);

        yield return new WaitForSecondsRealtime(0.4f);

        if (playerSprite != null)
            playerSprite.enabled = true;

        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvas.alpha = Mathf.Lerp(1f, 0f, t / 0.2f);
            yield return null;
        }
        fadeCanvas.alpha = 0f;

        playerAnimator.SetBool("ExitDoor", false);

        yield return new WaitForSecondsRealtime(0.4f);

        playerAnimator.updateMode = AnimatorUpdateMode.Normal;
        pc.isDead = false;
        pc.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        pc.GetComponent<Rigidbody2D>().gravityScale = 1f;

        isUsable = true;

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

        if (exitDoor != null)
            yield return StartCoroutine(exitDoor.ExitSequence(player, pc, playerAnimator, playerSprite, fadeCanvas));

        isUsable = true;

        inCutscene = false;
    }
}