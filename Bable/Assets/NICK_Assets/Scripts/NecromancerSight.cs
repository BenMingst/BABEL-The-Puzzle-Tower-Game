using UnityEngine;

public class NecromancerSight : MonoBehaviour
{
    private NecromancerAI necromancerAI;

    void Start()
    {
        necromancerAI = GetComponentInParent<NecromancerAI>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            necromancerAI.playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            necromancerAI.playerInRange = false;
    }
}