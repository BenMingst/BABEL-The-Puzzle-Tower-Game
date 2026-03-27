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
        Debug.Log("OnSceneLoaded fired, scene index: " + scene.buildIndex);

        if (scene.buildIndex == 0)
        {
            hasSword = false;
            hasBow = false;
            furthestCheckpoint = 0;
            hasCustomSpawn = false;
            respawnWithSword = false;
            spawnPosition = Vector3.zero;
        }

        if (scene.buildIndex >= 1)
            hasSword = true;
        // bow is never auto equipped - only from chest
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnApplicationQuit()
    {
        hasSword = false;
        hasBow = false;
        furthestCheckpoint = 0;
        hasCustomSpawn = false;
        respawnWithSword = false;
    }

    public void ReachedCheckpoint(int checkpointIndex, Vector3 position, bool sword)
    {
        if (checkpointIndex > furthestCheckpoint)
        {
            furthestCheckpoint = checkpointIndex;
            spawnPosition = position;
            hasCustomSpawn = true;
            if (sword) respawnWithSword = true;
        }
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}