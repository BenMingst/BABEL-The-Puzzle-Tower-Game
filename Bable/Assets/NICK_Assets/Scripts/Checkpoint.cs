using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;
    public Transform spawnPoint;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
    Debug.Log("Checkpoint triggered: " + checkpointIndex + " furthestCheckpoint: " + GameManager.furthestCheckpoint + " CheckpointManager null: " + (CheckpointManager.Instance == null));

        PlayerController pc = other.GetComponent<PlayerController>();
        bool hasSword = pc != null && pc.hasSword;

        GameManager.Instance.ReachedCheckpoint(checkpointIndex, spawnPoint.position, hasSword);

        if (GameManager.furthestCheckpoint == checkpointIndex && CheckpointManager.Instance != null)
        {
                Debug.Log("Checkpoint triggered: " + checkpointIndex + " furthestCheckpoint: " + GameManager.furthestCheckpoint + " CheckpointManager null: " + (CheckpointManager.Instance == null));

            CheckpointManager.Instance.SaveState(checkpointIndex);
            Debug.Log("Checkpoint " + checkpointIndex + " state saved");
        }
    }
}