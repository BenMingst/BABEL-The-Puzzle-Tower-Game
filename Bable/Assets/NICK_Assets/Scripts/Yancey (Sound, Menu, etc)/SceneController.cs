using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    void Start()
    {
        // dont destory on load so we can call these functions from anywhere
        DontDestroyOnLoad(gameObject);
    }
    public void playLevel1()
    {
        SceneManager.LoadSceneAsync("another_test");
    }
    public void playLevel2()
    {
        SceneManager.LoadSceneAsync("new_level2");
    }
    public void playLevel3()
    {
        SceneManager.LoadSceneAsync("Level_3");
    }
    public void playLevel4()
    {
        SceneManager.LoadSceneAsync("Level_4");
    }
    public void playLevel5()
    {
        SceneManager.LoadSceneAsync("Level_5");
    }
    public void loadTitle()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void restartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void loadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels to load.");
        }
    }

    public void loadPreviousLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = currentSceneIndex - 1;

        if (previousSceneIndex >= 0)
        {
            SceneManager.LoadScene(previousSceneIndex);
        }
        else
        {
            Debug.Log("No previous levels to load.");
        }
    }
    public void quitGame()
    {
        Application.Quit();
    }
    public void loadTransitionScene(string nextSceneName)
    {
        // Store the next scene name in a static variable or a singleton
        LevelTransitionManager.nextLevelName = nextSceneName;
        // Load the transition scene
        SceneManager.LoadScene("TransitionScene");
    }
}