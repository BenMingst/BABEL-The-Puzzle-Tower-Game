using UnityEngine;

public class PlayerStatsTracker : MonoBehaviour
{
    private Vector3 lastPosition;

    void Start()
    {
        // Initialize position so we don't get a huge jump on the first frame
        lastPosition = transform.position;
    }

    void Update()
    {
        // Time.deltaTime is the time in seconds since the last frame.
        GameManager.Instance.levelTime += Time.deltaTime;

        // Calculate how far the player moved since the last frame
        float frameDistance = Vector3.Distance(transform.position, lastPosition);
        
        // Add that distance to our total in the GameManager
        GameManager.Instance.distanceTraveled += frameDistance;

        // Update lastPosition for the next frame
        lastPosition = transform.position;
    }
}