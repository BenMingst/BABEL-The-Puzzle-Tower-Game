using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject swordPickupObject;
    public GameObject swordEventObject;
    public GameObject bowChestObject;
    public GameObject bombChestObject;
    public GameObject remoteBombChestObject;
    public GameObject grappleChestObject;

    void Start()
    {
        StartCoroutine(InitializePlayer());
    }

    IEnumerator InitializePlayer()
    {
        yield return null;

        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.RestoreState();

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // re-apply level-specific auto-equips after checkpoint restore (overrides saved state)
        if (sceneName == "Level_3")
        {
            GameManager.hasBow = true;
        }
        else if (sceneName == "Level_4")
        {
            GameManager.hasBow = true;
            GameManager.hasBomb = true;
            GameManager.hasRemoteBomb = true;
        }
        else if (sceneName == "Level_5")
        {
            GameManager.hasBow = true;
            GameManager.hasBomb = true;
            GameManager.hasRemoteBomb = true;
            GameManager.hasGrapple = true;
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

        if (GameManager.hasRemoteBomb)
        {
            pc.EquipRemoteBomb();
            if (bombChestObject != null) bombChestObject.SetActive(false);
            if (remoteBombChestObject != null) remoteBombChestObject.SetActive(false);
        }
        else if (GameManager.hasBomb)
        {
            pc.EquipBomb();
            if (bombChestObject != null) bombChestObject.SetActive(false);
        }

        if (GameManager.hasGrapple)
        {
            pc.EquipGrapple();
            if (grappleChestObject != null) grappleChestObject.SetActive(false);
        }

        // unlock fire and ice arrows for level 3, 4, and 5
        if (sceneName == "Level_3" || sceneName == "Level_4" || sceneName == "Level_5")
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