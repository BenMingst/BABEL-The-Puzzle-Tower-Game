using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NecromancerAI : MonoBehaviour
{
    [Header("Detection")]
    public float sightRange = 8f;

    [Header("Enemy Prefabs")]
    public GameObject[] regularEnemyPrefabs;
    public GameObject normalEvilEyePrefab;
    public GameObject iceEvilEyePrefab;
    public GameObject fireEvilEyePrefab;
    public int maxEvilEyeCount = 2;

    [Header("Spawning")]
    public BoxCollider2D spawnZone;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public GameObject spawnEffectPrefab;
    public float spawnEffectDuration = 2f;
    public float spawnHeightOffset = 0f;
    public float neckroCooldown = 5f;
    public int spawnCap = 10;
    public float minSpawnDistance = 3f;
    public float minDistanceBetweenSpawns = 2f;

    [Header("Indicator")]
    public GameObject indicatorPrefab;

    [Header("Stagger")]
    public float staggerDuration = 5f;
    public GameObject necroBarrier;

    [Header("Teleport")]
    public GameObject teleportOutPrefab;
    public GameObject teleportInPrefab;
    public float teleportMinDistance = 5f;
    public float teleportMaxDistance = 15f;
    public float groundCheckWidth = 5f;
    public float teleportDisappearDuration = 0.5f;

    [Header("Animation")]
    public Animator animator;

    public bool playerInRange = false;
    public bool isStaggered = false;
    private bool isSpawning = false;
    private int staggerHitCount = 0;
    private Coroutine staggerCoroutine = null;
    public List<GameObject> spawnedEnemies = new List<GameObject>();
    private List<GameObject> spawnedEvilEyes = new List<GameObject>();
    private GameObject indicatorEnemy = null;
    private EnemyHealth necroHealth;
    private GameObject player;
    private CapsuleCollider2D capsuleCollider;

    void Start()
    {
        animator = GetComponent<Animator>();
        necroHealth = GetComponent<EnemyHealth>();
        player = GameObject.FindWithTag("Player");
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        if (necroBarrier != null)
            necroBarrier.SetActive(true);
    }

    void Update()
    {
        if (necroHealth != null && necroHealth.isDead) return;
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        playerInRange = distanceToPlayer <= sightRange;

        if (!playerInRange) return;
        if (isSpawning) return;
        if (isStaggered) return;

        spawnedEnemies.RemoveAll(e => e == null);
        spawnedEvilEyes.RemoveAll(e => e == null);

        if (spawnedEnemies.Count < spawnCap)
        {
            isSpawning = true;
            StartCoroutine(SpawnSequence());
        }
    }

    public bool IsVulnerable()
    {
        return isStaggered;
    }

    public void TriggerStagger()
    {
        if (necroHealth != null && necroHealth.isDead) return;
        if (staggerCoroutine != null)
            StopCoroutine(staggerCoroutine);
        staggerCoroutine = StartCoroutine(StaggerSequence());
    }

    IEnumerator StaggerSequence()
    {
        isStaggered = true;
        isSpawning = false;
        staggerHitCount = 0;

        if (necroBarrier != null)
            necroBarrier.SetActive(false);

        animator.SetTrigger("Stagger");

        float elapsed = 0f;
        while (elapsed < staggerDuration)
        {
            elapsed += Time.deltaTime;

            if (staggerHitCount >= 2)
                break;

            yield return null;
        }

        EndStagger();
    }

    public void NotifyStaggerHit(int damage)
    {
        if (!isStaggered) return;

        staggerHitCount++;

        if (damage > 1 || staggerHitCount >= 2)
        {
            if (staggerCoroutine != null)
                StopCoroutine(staggerCoroutine);
            EndStagger();
        }
    }

    void EndStagger()
    {
        isStaggered = false;
        staggerHitCount = 0;
        staggerCoroutine = null;
        StartCoroutine(EndStaggerSequence());
    }
    IEnumerator EndStaggerSequence()
{
    // wait for hurt animation to finish
    yield return new WaitForSeconds(0.75f);
    StartCoroutine(TeleportSequence());
}

    IEnumerator TeleportSequence()
{
    if (teleportOutPrefab != null)
        Instantiate(teleportOutPrefab, transform.position, Quaternion.identity);

    SpriteRenderer sr = GetComponent<SpriteRenderer>();
    CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
    Rigidbody2D rb = GetComponent<Rigidbody2D>();

    if (sr != null) sr.enabled = false;
    if (col != null) col.enabled = false;

    // freeze position so gravity doesn't pull it down while invisible
    if (rb != null)
    {
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    yield return new WaitForSeconds(teleportDisappearDuration);

    Vector2? newPos = GetTeleportPosition();
    if (newPos != null)
        transform.position = newPos.Value;

    if (teleportInPrefab != null)
        Instantiate(teleportInPrefab, transform.position, Quaternion.identity);

    yield return new WaitForSeconds(teleportDisappearDuration);

    // restore constraints
    if (rb != null)
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearVelocity = Vector2.zero;
    }

    if (sr != null) sr.enabled = true;
    if (col != null) col.enabled = true;

    if (necroBarrier != null)
        necroBarrier.SetActive(true);

    animator.SetTrigger("StaggerEnd");
}

    Vector2? GetTeleportPosition()
    {
        if (player == null) return null;

        int maxAttempts = 20;

        for (int i = 0; i < maxAttempts; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(teleportMinDistance, teleportMaxDistance);

            Vector2 candidatePos = new Vector2(
                transform.position.x + Mathf.Cos(angle) * distance,
                transform.position.y + Mathf.Sin(angle) * distance);

            RaycastHit2D hit = Physics2D.Raycast(candidatePos, Vector2.down, 20f, groundLayer);
            if (hit.collider == null) continue;

            Vector2 groundPos = new Vector2(candidatePos.x, hit.point.y + 1f);

            if (!IsGroundWideEnough(groundPos)) continue;

            float distToPlayer = Vector2.Distance(groundPos, player.transform.position);
            if (distToPlayer < teleportMinDistance) continue;

            return groundPos;
        }

        return null;
    }

    bool IsGroundWideEnough(Vector2 position)
    {
        float halfWidth = groundCheckWidth * 0.5f;
        int checks = 5;

        for (int i = 0; i < checks; i++)
        {
            float checkX = position.x - halfWidth + (groundCheckWidth / (checks - 1)) * i;
            Vector2 checkPos = new Vector2(checkX, position.y);
            RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, 1.5f, groundLayer);
            if (hit.collider == null) return false;
        }

        return true;
    }

    IEnumerator SpawnSequence()
    {
        spawnedEnemies.RemoveAll(e => e == null);
        spawnedEvilEyes.RemoveAll(e => e == null);
        int currentCount = spawnedEnemies.Count;

        int spawnCount;
        if (currentCount == 0)
            spawnCount = 3;
        else
            spawnCount = 2;

        spawnCount = Mathf.Min(spawnCount, spawnCap - currentCount);

        if (spawnCount <= 0)
        {
            isSpawning = false;
            yield break;
        }

        List<Vector2> spawnPositions = GetMultipleGroundPositions(spawnCount);
        if (spawnPositions.Count == 0)
        {
            isSpawning = false;
            yield break;
        }

        animator.SetTrigger("SummonWindup");
        yield return new WaitForSeconds(GetAnimationLength("SummonWindup"));

        animator.SetTrigger("Summon");

        foreach (Vector2 pos in spawnPositions)
        {
            if (spawnEffectPrefab != null)
                Instantiate(spawnEffectPrefab, pos, Quaternion.identity);
        }

        yield return new WaitForSeconds(spawnEffectDuration);

        bool needsIndicator = indicatorEnemy == null;
        int indicatorIndex = needsIndicator ? Random.Range(0, spawnPositions.Count) : -1;

        for (int i = 0; i < spawnPositions.Count; i++)
        {
            GameObject enemy = SpawnEnemy(spawnPositions[i]);
            if (needsIndicator && i == indicatorIndex && enemy != null)
            {
                AttachIndicator(enemy);
                indicatorEnemy = enemy;
            }
        }

        float summonLength = GetAnimationLength("Summon");
        float remainingTime = summonLength - spawnEffectDuration;
        if (remainingTime > 0)
            yield return new WaitForSeconds(remainingTime);

        animator.SetTrigger("Idle");

        yield return new WaitForSeconds(neckroCooldown);

        isSpawning = false;
    }

    void AttachIndicator(GameObject enemy)
    {
        if (indicatorPrefab == null) return;

        EvilEye evilEye = enemy.GetComponent<EvilEye>();
        float yOffset = evilEye != null ? 6f : 3.5f;

        GameObject indicator = Instantiate(indicatorPrefab, enemy.transform);
        indicator.transform.localPosition = new Vector3(0f, yOffset, 0f);

        IndicatorEnemy indicatorScript = indicator.AddComponent<IndicatorEnemy>();
        indicatorScript.Initialize(this);
    }

    GameObject SpawnEnemy(Vector2 position)
    {
        spawnedEvilEyes.RemoveAll(e => e == null);

        GameObject prefabToSpawn = null;

        if (spawnedEvilEyes.Count < maxEvilEyeCount && Random.value <= 0.3f)
        {
            float eyeRoll = Random.value;
            if (eyeRoll <= 0.5f && normalEvilEyePrefab != null)
                prefabToSpawn = normalEvilEyePrefab;
            else if (eyeRoll <= 0.75f && iceEvilEyePrefab != null)
                prefabToSpawn = iceEvilEyePrefab;
            else if (fireEvilEyePrefab != null)
                prefabToSpawn = fireEvilEyePrefab;
        }

        if (prefabToSpawn == null)
        {
            if (regularEnemyPrefabs.Length == 0) return null;
            prefabToSpawn = regularEnemyPrefabs[Random.Range(0, regularEnemyPrefabs.Length)];
        }

        GameObject enemy = Instantiate(prefabToSpawn, position, Quaternion.identity);
        spawnedEnemies.Add(enemy);

        if (prefabToSpawn == normalEvilEyePrefab ||
            prefabToSpawn == iceEvilEyePrefab ||
            prefabToSpawn == fireEvilEyePrefab)
            spawnedEvilEyes.Add(enemy);

        return enemy;
    }

    List<Vector2> GetMultipleGroundPositions(int count)
    {
        List<Vector2> positions = new List<Vector2>();
        int maxAttempts = 50;
        int attempts = 0;

        while (positions.Count < count && attempts < maxAttempts)
        {
            attempts++;
            Vector2? pos = GetRandomGroundPosition();
            if (pos == null) continue;

            bool tooClose = false;
            foreach (Vector2 existingPos in positions)
            {
                if (Vector2.Distance(pos.Value, existingPos) < minDistanceBetweenSpawns)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
                positions.Add(pos.Value);
        }

        return positions;
    }

    Vector2? GetRandomGroundPosition()
    {
        if (spawnZone == null) return null;

        Bounds bounds = spawnZone.bounds;
        int maxAttempts = 20;

        float minDist = minSpawnDistance;
        if (capsuleCollider != null)
            minDist += capsuleCollider.size.x * 0.5f;

        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(bounds.min.x, bounds.max.x);

            float distFromNecro = Mathf.Abs(randomX - transform.position.x);
            if (distFromNecro < minDist) continue;

            Vector2 rayOrigin = new Vector2(randomX, bounds.max.y);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, bounds.size.y, groundLayer);

            if (hit.collider != null)
                return new Vector2(randomX, hit.point.y + spawnHeightOffset);
        }

        return null;
    }

    float GetAnimationLength(string stateName)
    {
        if (animator == null) return 1f;
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == stateName)
                return clip.length;
        }
        return 1f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        if (capsuleCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minSpawnDistance + capsuleCollider.size.x * 0.5f);
        }
    }
}