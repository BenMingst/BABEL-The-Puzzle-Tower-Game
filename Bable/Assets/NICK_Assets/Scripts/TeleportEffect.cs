using System.Collections;
using UnityEngine;

public class TeleportEffect : MonoBehaviour
{
    public float duration = 0.25f;

    void Start()
    {
        StartCoroutine(DestroyAfterDuration());
    }

    IEnumerator DestroyAfterDuration()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}