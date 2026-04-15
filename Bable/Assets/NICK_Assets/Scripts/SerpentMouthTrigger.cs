using UnityEngine;

public class SerpentMouthTrigger : MonoBehaviour
{
    private SerpentAI serpentAI;

    void Start()
    {
        serpentAI = GetComponentInParent<SerpentAI>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Bomb bomb = other.GetComponent<Bomb>();
        if (bomb != null)
        {
            // notify player that bomb is done before destroying
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null) pc.OnBombExploded();

            Destroy(bomb.gameObject);
            serpentAI.BombSwallowed();
        }
    }
}