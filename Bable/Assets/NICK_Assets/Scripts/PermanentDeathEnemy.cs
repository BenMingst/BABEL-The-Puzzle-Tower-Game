using UnityEngine;

public class PermanentDeathEnemy : MonoBehaviour
{
    public string persistentID;
    public bool isDead = false;

    public void MarkDead()
    {
        isDead = true;
    }
}
