using UnityEngine;

public class DropdownOverlapDetector : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Dropdown"))
            playerController.SetInsideDropdown(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Dropdown"))
            playerController.SetInsideDropdown(false);
    }
}
