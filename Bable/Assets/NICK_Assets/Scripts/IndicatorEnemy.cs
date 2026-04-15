using UnityEngine;

public class IndicatorEnemy : MonoBehaviour
{
    private NecromancerAI necroAI;

    public void Initialize(NecromancerAI necro)
    {
        necroAI = necro;
    }

    void OnDestroy()
    {
        if (necroAI != null)
            necroAI.TriggerStagger();
    }
}