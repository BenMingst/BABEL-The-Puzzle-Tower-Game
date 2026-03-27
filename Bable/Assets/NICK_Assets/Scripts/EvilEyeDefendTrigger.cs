using UnityEngine;

public class EvilEyeDefendTrigger : MonoBehaviour
{
    private EvilEye evilEye;

    void Start()
    {
        evilEye = GetComponentInParent<EvilEye>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
            Debug.Log("DefendTrigger hit by: " + other.gameObject.name + " tag: " + other.tag);

        if (other.CompareTag("Player"))
            evilEye.PlayerEnteredDefendRange();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            evilEye.PlayerExitedDefendRange();
    }
}
