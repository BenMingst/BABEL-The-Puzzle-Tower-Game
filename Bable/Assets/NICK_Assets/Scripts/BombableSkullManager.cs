using UnityEngine;

public class BombableSkullManager : MonoBehaviour
{
    public string persistentID;
    public BombableSkull[] skulls;
    public SwitchDoor linkedDoor;

    public bool doorOpened = false;

    void Update()
    {
        if (doorOpened) return;

        bool allBombed = true;
        foreach (BombableSkull skull in skulls)
        {
            if (!skull.hasBeenBombed)
            {
                allBombed = false;
                break;
            }
        }

        if (allBombed)
        {
            doorOpened = true;
            if (linkedDoor != null)
                linkedDoor.OpenPermanently();
        }
    }
}