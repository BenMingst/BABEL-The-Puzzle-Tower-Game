using System.Collections;
using UnityEngine;

public class BombAttack : MonoBehaviour
{
    public static BombAttack Instance;

    [Header("Bomb")]
    public GameObject bombPrefab;
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
    private PlayerAudio playerAudio;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        pc = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        playerAudio = GetComponent<PlayerAudio>();
        if (playerAudio == null) playerAudio = gameObject.AddComponent<PlayerAudio>();
    }

    public void StartBombAttack()
    {
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
        SpawnBomb(BombType.Place);

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

        if (upwardThrow)
        {
            animator.SetTrigger("BombLobUp");
            yield return new WaitForSeconds(0.2f);
            SpawnBomb(BombType.LobUp);
        }
        else
        {
            animator.SetTrigger("BombLob");
            yield return new WaitForSeconds(0.2f);
            SpawnBomb(BombType.Lob);
        }

        SwitchToNoSword();
    }

    void SpawnBomb(BombType type)
    {
        if (bombPrefab == null) return;

        Transform spawnPoint = lobSpawnPoint;
        if (type == BombType.LobUp) spawnPoint = lobUpSpawnPoint;
        if (type == BombType.Place) spawnPoint = placeSpawnPoint;
        if (spawnPoint == null) spawnPoint = lobSpawnPoint;

        // play throw sound
        SoundManager.instance.PlayWorldRandom(playerAudio.bombThrowSounds, transform, 1f, 0f);

        // mirror spawn position if facing left
        Vector3 spawnPos = spawnPoint.position;
        if (!pc.facingRight)
        {
            float distFromCenter = spawnPoint.position.x - pc.transform.position.x;
            spawnPos = new Vector3(
                pc.transform.position.x - distFromCenter,
                spawnPoint.position.y,
                spawnPoint.position.z);
        }

        GameObject bombObj = Instantiate(bombPrefab, spawnPos, Quaternion.identity);
        Bomb bomb = bombObj.GetComponent<Bomb>();

        switch (type)
        {
            case BombType.Lob:
                float angle = normalLobAngle * Mathf.Deg2Rad;
                float dirX = pc.facingRight ? 1f : -1f;
                Vector2 lobVelocity = new Vector2(
                    dirX * normalLobSpeed * Mathf.Cos(angle),
                    normalLobSpeed * Mathf.Sin(angle));
                bomb.Launch(lobVelocity);
                break;

            case BombType.LobUp:
                float upAngle = upwardLobAngle * Mathf.Deg2Rad;
                float dirXUp = pc.facingRight ? 1f : -1f;
                Vector2 upVelocity = new Vector2(
                    dirXUp * upwardLobSpeed * Mathf.Cos(upAngle),
                    upwardLobSpeed * Mathf.Sin(upAngle));
                bomb.Launch(upVelocity);
                break;

            case BombType.Place:
                bomb.rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    public void OnBombExploded()
    {
        bombActive = false;
        if (InventoryManager.Instance != null && InventoryManager.Instance.IsBombSelected())
            InventoryManager.Instance.SelectCurrentSlot();
    }

    public enum BombType { Lob, LobUp, Place }
}