using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static Vector3 spawnPosition;
    public static bool hasCustomSpawn = false;
    public static bool respawnWithSword = false;
    public static int furthestCheckpoint = 0;
    public static bool hasSword = false;
    public static bool hasBow = false;
    public static bool hasBomb = false;
    public static bool hasRemoteBomb = false;
    public static bool hasGrapple = false;

    public Transform defaultSpawnPoint;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 2)
        {
            hasSword = false;
            hasBow = false;
            hasBomb = false;
            hasRemoteBomb = false;
            hasGrapple = false;
            furthestCheckpoint = 0;
            hasCustomSpawn = false;
            respawnWithSword = false;
            spawnPosition = Vector3.zero;
        }

        if (scene.buildIndex >= 3)
            hasSword = true;

        if (scene.name == "Level_3")
            hasBow = true;

        if (scene.name == "Level_4")
        {
            hasBow = true;
            hasBomb = true;
            hasGrapple = true;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnApplicationQuit()
    {
        hasSword = false;
        hasBow = false;
        hasBomb = false;
        hasRemoteBomb = false;
        hasGrapple = false;
        furthestCheckpoint = 0;
        hasCustomSpawn = false;
        respawnWithSword = false;
    }

    public bool ReachedCheckpoint(int checkpointIndex, Vector3 position, bool sword)
    {
        if (checkpointIndex > furthestCheckpoint)
        {
            furthestCheckpoint = checkpointIndex;
            spawnPosition = position;
            hasCustomSpawn = true;
            if (sword) respawnWithSword = true;
            return true;
        }
        return false;
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}