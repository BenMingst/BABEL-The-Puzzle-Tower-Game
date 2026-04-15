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

        if (GameManager.hasBomb)
        {
            pc.EquipBomb();
            if (bombChestObject != null) bombChestObject.SetActive(false);
        }

        // unlock fire and ice arrows for level 3
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level_3")
        {
            if (ArrowTypeManager.Instance != null)
            {
                ArrowTypeManager.Instance.UnlockFireArrows();
                ArrowTypeManager.Instance.UnlockIceArrows();
            }
        }

        // restore fire/ice arrows from checkpoint
        if (CheckpointManager.Instance != null)
        {
            if (CheckpointManager.Instance.savedState.hasFireArrows && ArrowTypeManager.Instance != null)
                ArrowTypeManager.Instance.UnlockFireArrows();
            if (CheckpointManager.Instance.savedState.hasIceArrows && ArrowTypeManager.Instance != null)
                ArrowTypeManager.Instance.UnlockIceArrows();
        }
    }
}