using UnityEngine;

public class MenuButtonFX : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    public void PlayPressAnim()
    {
        if (animator != null)
            animator.SetTrigger("Press");
    }
}