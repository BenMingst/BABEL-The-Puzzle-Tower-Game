using System.Collections;
using UnityEngine;

public class SpawnEffect : MonoBehaviour
{
    public float duration = 2f;

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