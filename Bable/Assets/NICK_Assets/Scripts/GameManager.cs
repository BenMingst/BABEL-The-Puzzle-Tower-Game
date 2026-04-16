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

    [Header("Current Level Stats")]
    public int currentLevelIndex;
    public int enemiesDefeated;
    public int weaponsFound;
    public int chestsFound;
    public float levelTime;
    public float distanceTraveled;
    public int deaths;

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

        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;

        if (scene.buildIndex == 0)
        {
            hasSword = false;
            hasBow = false;
            hasBomb = false;
            furthestCheckpoint = 0;
            hasCustomSpawn = false;
            respawnWithSword = false;
            spawnPosition = Vector3.zero;
        }

        if (scene.name == "Level_3")
        {
            hasBow = true;
            hasBomb = true;
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

    public void ResetLevelStats()
    {
        enemiesDefeated = 0;
        weaponsFound = 0;
        chestsFound = 0;
        levelTime = 0f;
        distanceTraveled = 0;
        deaths = 0;
        currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
    }
}