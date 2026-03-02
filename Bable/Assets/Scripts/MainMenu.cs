using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void playLevel1()
    {
        SceneManager.LoadSceneAsync("Level One");
    }
    public void playLevel2()
    {
        SceneManager.LoadSceneAsync("Level Two");
    }
    public void playLevel3()
    {
        SceneManager.LoadSceneAsync("Level Three");
    }
    public void loadTitle()
    {
        SceneManager.LoadSceneAsync("Main Menu");
    }
    public void quitGame()
    {
        Application.Quit();
    }
}