using UnityEngine;

public class BombableSkullTrigger : MonoBehaviour
{
    private BombableSkull bombableSkull;

    void Start()
    {
        bombableSkull = GetComponentInParent<BombableSkull>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Bomb bomb = other.GetComponent<Bomb>();
        if (bomb != null)
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null) pc.OnBombExploded();

            Destroy(bomb.gameObject);
            bombableSkull.BombHit();
        }
    }
}