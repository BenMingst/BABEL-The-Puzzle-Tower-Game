using UnityEngine;

public class ArcherAnimationEvents : MonoBehaviour
{
    private ArcherAI archerAI;

    void Start()
    {
        archerAI = GetComponentInParent<ArcherAI>();
    }

    public void SpawnArrow()
    {
        if (archerAI != null)
            archerAI.SpawnArrow();
    }
}