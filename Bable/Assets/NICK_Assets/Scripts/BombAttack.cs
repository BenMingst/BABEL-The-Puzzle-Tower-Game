using System.Collections;
using UnityEngine;

public class BombAttack : MonoBehaviour
{
    public static BombAttack Instance;

    [Header("Bomb Prefabs")]
    public GameObject bombPrefab;
    public GameObject remoteBombPrefab;

    public Transform lobSpawnPoint;
    public Transform lobUpSpawnPoint;
    public Transform placeSpawnPoint;

    [Header("Lob Settings")]
    public float normalLobSpeed = 8f;
    public float normalLobAngle = 45f;
    public float upwardLobSpeed = 10f;
    public float upwardLobAngle = 80f;

    [Header("Windup")]
    public float windupDuration = 0.4f;

    public bool bombActive = false;
    public bool isWindingUp = false;
    public bool isCrouchingWhenThrown = false;
    private PlayerController pc;
    private Animator animator;
    public RemoteBomb activeRemoteBomb = null;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        pc = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
    }

    public void StartBombAttack()
    {
        // if remote bomb is active detonate it
        if (activeRemoteBomb != null)
        {
            activeRemoteBomb.Detonate();
            activeRemoteBomb = null;
            return;
        }

        if (bombActive || isWindingUp) return;

        if (isCrouchingWhenThrown)
            StartCoroutine(BombPlace());
        else
            StartCoroutine(BombWindup());
    }

    void SwitchToNoSword()
    {
        animator.runtimeAnimatorController = pc.noSwordAnimator;
        animator.SetBool("FacingRight", pc.facingRight);
        if (!pc.facingRight)
            animator.Play("Idle_Left");
        else
            animator.Play("Idle_Right");
    }

    IEnumerator BombPlace()
    {
        isWindingUp = true;
        bombActive = true;

        animator.SetTrigger("BombPlace");

        yield return new WaitForSeconds(0.3f);

        isWindingUp = false;
        SpawnBomb(ThrowType.Place);

        SwitchToNoSword();
    }

    IEnumerator BombWindup()
    {
        isWindingUp = true;
        bool upwardThrow = false;

        if (pc.facingRight)
            animator.SetTrigger("BombWindUpRight");
        else
            animator.SetTrigger("BombWindUpLeft");

        float elapsed = 0f;
        while (elapsed < windupDuration)
        {
            elapsed += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.W))
            {
                upwardThrow = true;
                break;
            }

            yield return null;
        }

        isWindingUp = false;
        bombActive = true;

        // play throw sound
        SoundManager.instance.PlayWorldRandom(PlayerAudio.instance.bomb.throwSounds, transform, 1f);

        if (upwardThrow)
        {
            animator.SetTrigger("BombLobUp");
            yield return new WaitForSeconds(0.2f);
            SpawnBomb(ThrowType.LobUp);
        }
        else
        {
            animator.SetTrigger("BombLob");
            yield return new WaitForSeconds(0.2f);
            SpawnBomb(ThrowType.Lob);
        }

        SwitchToNoSword();
    }

    void SpawnBomb(ThrowType type)
    {
        bool isRemote = BombTypeManager.Instance != null &&
                        BombTypeManager.Instance.currentBombType == BombTypeManager.BombType.Remote;
                         Debug.Log("SpawnBomb - isRemote: " + isRemote + 
              " BombTypeManager null: " + (BombTypeManager.Instance == null) +
              " currentBombType: " + (BombTypeManager.Instance != null ? BombTypeManager.Instance.currentBombType.ToString() : "N/A") +
              " remoteBombPrefab null: " + (remoteBombPrefab == null));

        GameObject prefab = isRemote ? remoteBombPrefab : bombPrefab;
        if (prefab == null) {
            Debug.Log("Prefab is null - cannot spawn bomb");
            return;
        }

        Transform spawnPoint = lobSpawnPoint;
        if (type == ThrowType.LobUp) spawnPoint = lobUpSpawnPoint;
        if (type == ThrowType.Place) spawnPoint = placeSpawnPoint;
        if (spawnPoint == null) spawnPoint = lobSpawnPoint;

        Vector3 spawnPos = spawnPoint.position;
        if (!pc.facingRight)
        {
            float distFromCenter = spawnPoint.position.x - pc.transform.position.x;
            spawnPos = new Vector3(
                pc.transform.position.x - distFromCenter,
                spawnPoint.position.y,
                spawnPoint.position.z);
        }

        GameObject bombObj = Instantiate(prefab, spawnPos, Quaternion.identity);

        if (isRemote)
        {
            RemoteBomb remoteBomb = bombObj.GetComponent<RemoteBomb>();
            activeRemoteBomb = remoteBomb;

            switch (type)
            {
                case ThrowType.Lob:
                    float angle = normalLobAngle * Mathf.Deg2Rad;
                    float dirX = pc.facingRight ? 1f : -1f;
                    remoteBomb.Launch(new Vector2(
                        dirX * normalLobSpeed * Mathf.Cos(angle),
                        normalLobSpeed * Mathf.Sin(angle)));
                    break;
                case ThrowType.LobUp:
                    float upAngle = upwardLobAngle * Mathf.Deg2Rad;
                    float dirXUp = pc.facingRight ? 1f : -1f;
                    remoteBomb.Launch(new Vector2(
                        dirXUp * upwardLobSpeed * Mathf.Cos(upAngle),
                        upwardLobSpeed * Mathf.Sin(upAngle)));
                    break;
                case ThrowType.Place:
                    remoteBomb.rb.linearVelocity = Vector2.zero;
                    break;
            }
        }
        else
        {
            Bomb bomb = bombObj.GetComponent<Bomb>();

            switch (type)
            {
                case ThrowType.Lob:
                    float angle = normalLobAngle * Mathf.Deg2Rad;
                    float dirX = pc.facingRight ? 1f : -1f;
                    bomb.Launch(new Vector2(
                        dirX * normalLobSpeed * Mathf.Cos(angle),
                        normalLobSpeed * Mathf.Sin(angle)));
                    break;
                case ThrowType.LobUp:
                    float upAngle = upwardLobAngle * Mathf.Deg2Rad;
                    float dirXUp = pc.facingRight ? 1f : -1f;
                    bomb.Launch(new Vector2(
                        dirXUp * upwardLobSpeed * Mathf.Cos(upAngle),
                        upwardLobSpeed * Mathf.Sin(upAngle)));
                    break;
                case ThrowType.Place:
                    bomb.rb.linearVelocity = Vector2.zero;
                    break;
            }
        }
    }

    public void OnBombExploded()
    {
        bombActive = false;
        activeRemoteBomb = null;
        if (InventoryManager.Instance != null && InventoryManager.Instance.IsBombSelected())
            InventoryManager.Instance.SelectCurrentSlot();
    }

    public enum ThrowType { Lob, LobUp, Place }
}