using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;
    public Transform spawnPoint;

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        PlayerController pc = other.GetComponent<PlayerController>();
        bool hasSword = pc != null && pc.hasSword;

        GameManager.Instance.ReachedCheckpoint(checkpointIndex, spawnPoint.position, hasSword);

        Debug.Log("Checkpoint " + checkpointIndex + " reached");
    }
}
