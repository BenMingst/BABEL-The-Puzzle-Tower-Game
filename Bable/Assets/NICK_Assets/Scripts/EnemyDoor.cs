using System.Collections;
using UnityEngine;
using Cinemachine;

public class EnemyDoor : MonoBehaviour
{
    public string persistentID;

    [Header("Components")]
    public Animator doorAnimator;
    public Collider2D doorCollider;

    [Header("Enemies")]
    public GameObject[] linkedEnemies;

    [Header("Camera Pan")]
    public Camera mainCamera;
    public CinemachineVirtualCamera virtualCamera;
    public float panSpeed = 5f;
    public float waitAtDoorDuration = 1f;
    public float zoomSize = 3f;

    public bool isOpened = false;
    private int enemiesRemaining;
    private float defaultCameraSize;

    void Start()
    {
        enemiesRemaining = linkedEnemies.Length;
        defaultCameraSize = mainCamera.orthographicSize;
    }

    void Update()
    {
        if (isOpened) return;

        int aliveCount = 0;
        foreach (GameObject enemy in linkedEnemies)
        {
            if (enemy != null)
                aliveCount++;
        }

        if (aliveCount == 0 && enemiesRemaining > 0)
        {
            enemiesRemaining = 0;
            StartCoroutine(OpenDoorSequence());
        }
    }

    public void RestoreOpened()
{
    isOpened = true;
    if (doorCollider != null) doorCollider.enabled = false;
    if (doorAnimator != null)
        doorAnimator.Play("Opened");
}

    IEnumerator OpenDoorSequence()
    {
        isOpened = true;

        GameObject player = GameObject.FindWithTag("Player");
        PlayerController pc = player.GetComponent<PlayerController>();

        Time.timeScale = 0f;
        pc.isDead = true;

        if (virtualCamera != null)
            virtualCamera.gameObject.SetActive(false);

        Vector3 playerCameraPos = mainCamera.transform.position;
        Vector3 doorCameraPos = new Vector3(
            transform.position.x,
            transform.position.y,
            mainCamera.transform.position.z);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * panSpeed;
            mainCamera.transform.position = Vector3.Lerp(playerCameraPos, doorCameraPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(defaultCameraSize, zoomSize, t);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(waitAtDoorDuration);

        if (doorAnimator != null)
        {
            doorAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            doorAnimator.SetTrigger("Unlock");
        }
        
        // wait 2 seconds for unlock animation before playing evil laugh
        yield return new WaitForSecondsRealtime(1f);
            if (SoundManager.instance != null)
            {
                    SoundManager.instance.PlayWorldClip(SoundManager.instance.enemyDoorLaugh, transform, 1f);
            }

        yield return new WaitForSecondsRealtime(3f);

        if (doorCollider != null)
            doorCollider.enabled = false;

        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * panSpeed;
            mainCamera.transform.position = Vector3.Lerp(doorCameraPos, playerCameraPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(zoomSize, defaultCameraSize, t);
            yield return null;
        }

        if (virtualCamera != null)
            virtualCamera.gameObject.SetActive(true);

        if (doorAnimator != null)
            doorAnimator.updateMode = AnimatorUpdateMode.Normal;

        Time.timeScale = 1f;

        // play door slide sound
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayWorldClip(SoundManager.instance.doorSlideLongSound, transform, 1f);
        }

        pc.isDead = false;
    }
}