using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Switch : MonoBehaviour
{
    [Header("Animation")]
    public Animator switchAnimator;

    [Header("Linked Doors")]
    public SwitchDoor[] linkedDoors;

    [Header("Settings")]
    public float unpressDelay = 0f;

    public bool isPressed = false;
    private Coroutine unpressCoroutine = null;
    private List<Collider2D> objectsOnSwitch = new List<Collider2D>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy") || IsBombObject(other))
        {
            objectsOnSwitch.Add(other);

            if (unpressCoroutine != null)
            {
                StopCoroutine(unpressCoroutine);
                unpressCoroutine = null;
            }

            if (!isPressed)
                Press();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy") || IsBombObject(other))
        {
            objectsOnSwitch.Remove(other);
            objectsOnSwitch.RemoveAll(o => o == null);

            if (objectsOnSwitch.Count == 0)
            {
                if (unpressDelay > 0f)
                    unpressCoroutine = StartCoroutine(UnpressAfterDelay());
                else
                    Unpress();
            }
        }
    }

    bool IsBombObject(Collider2D col)
    {
        return col.GetComponent<Bomb>() != null || col.GetComponent<RemoteBomb>() != null;
    }

    void Press()
    {
        isPressed = true;
        if (switchAnimator != null)
            switchAnimator.SetTrigger("Pressed");
        foreach (SwitchDoor door in linkedDoors)
            if (door != null) door.OnSwitchPressed();
    }

    void Unpress()
    {
        isPressed = false;
        if (switchAnimator != null)
            switchAnimator.SetTrigger("Unpressed");
        foreach (SwitchDoor door in linkedDoors)
            if (door != null) door.OnSwitchUnpressed();
    }

    IEnumerator UnpressAfterDelay()
    {
        yield return new WaitForSeconds(unpressDelay);
        Unpress();
        unpressCoroutine = null;
    }
}