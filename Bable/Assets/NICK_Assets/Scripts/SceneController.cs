using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void playLevel1()
    {
        SceneManager.LoadSceneAsync("another_test");
    }
    public void playLevel2()
    {
        SceneManager.LoadSceneAsync("NICK");
    }
    public void playLevel3()
    {
        SceneManager.LoadSceneAsync("Level Three");
    }
    public void loadTitle()
    {
        SceneManager.LoadSceneAsync("Main Menu");
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
    
    public void resumeGame()
    {
        Time.timeScale = 1f;
    }
}