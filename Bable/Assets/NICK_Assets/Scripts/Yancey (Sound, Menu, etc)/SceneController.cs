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
        SceneManager.LoadSceneAsync("Level1");
    }
    public void playLevel2()
    {
        SceneManager.LoadSceneAsync("Level2");
    }
    public void playLevel3()
    {
        SceneManager.LoadSceneAsync("Level3");
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
        Time.timeScale = 1f;
        SceneManager.LoadScene("TransitionScene");
    }
}