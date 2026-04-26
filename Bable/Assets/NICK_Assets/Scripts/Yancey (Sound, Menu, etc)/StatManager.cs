using UnityEngine;
using UnityEngine.SceneManagement;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance;

    [Header("Stats Tracking")]
    public float levelTime = 0f;
    public float distanceTraveled = 0f;
    public int deaths = 0;
    public int enemiesDefeated = 0;
    public int weaponsFound = 0;
    public int chestsFound = 0;
    public int jumps = 0;
    public int damageTaken = 0;

    private Vector3 _lastPlayerPosition;
    private Transform _playerTransform;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe once, persists across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Always unsubscribe to avoid ghost listeners
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 1)
        {
            ResetStats();
            FindPlayer();
        }
    }
    private void FindPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            _lastPlayerPosition = _playerTransform.position;
        }
        else
        {
            _playerTransform = null;
        }
    }
    void Update()
    {
        levelTime += Time.deltaTime;

        if (_playerTransform != null)
        {
            distanceTraveled += Vector3.Distance(_playerTransform.position, _lastPlayerPosition);
            _lastPlayerPosition = _playerTransform.position;
        }
    }

    public void ResetStats()
    {
        levelTime = 0f;
        distanceTraveled = 0f;
        deaths = 0;
        enemiesDefeated = 0;
        weaponsFound = 0;
        chestsFound = 0;
        jumps = 0;
        damageTaken = 0;
    }
}