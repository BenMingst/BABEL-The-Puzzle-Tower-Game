using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSaveHook : MonoBehaviour
{
    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Don't auto-save on the main menu or title scenes (build index 0 and 1)
        if (scene.buildIndex <= 1) return;

        // Write progress for the scene we just entered
        SaveSlotManager.WriteActiveSlot(scene.name, GameManager.furthestCheckpoint);
    }
}
