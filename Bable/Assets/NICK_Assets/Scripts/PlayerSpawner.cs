using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject swordPickupObject;
    public GameObject swordEventObject;
    public GameObject bowChestObject;
    public GameObject bombChestObject;

    void Start()
    {
        StartCoroutine(InitializePlayer());
    }

    IEnumerator InitializePlayer()
    {
        yield return null;

        Debug.Log("InitializePlayer - hasBomb: " + GameManager.hasBomb + " scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.RestoreState();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex >= 1)
            GameManager.hasSword = true;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level_3")
        {
            GameManager.hasBow = true;
            GameManager.hasBomb = true;
        }

        if (GameManager.hasCustomSpawn)
        {
            transform.position = GameManager.spawnPosition;
            GameManager.hasCustomSpawn = false;
        }

        PlayerController pc = GetComponent<PlayerController>();

        if (GameManager.hasSword)
        {
            pc.EquipSword();
            if (swordPickupObject != null) swordPickupObject.SetActive(false);
            if (swordEventObject != null) swordEventObject.SetActive(false);
        }

        if (GameManager.hasBow)
        {
            pc.EquipBow();
            if (bowChestObject != null) bowChestObject.SetActive(false);
        }

        Debug.Log("About to check hasBomb: " + GameManager.hasBomb);
        if (GameManager.hasBomb)
        {
            Debug.Log("Calling EquipBomb");
            pc.EquipBomb();
            if (bombChestObject != null) bombChestObject.SetActive(false);
        }
    }
}