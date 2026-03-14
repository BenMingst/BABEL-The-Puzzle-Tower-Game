using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void playLevel1()
    {
        SceneManager.LoadSceneAsync("NICK");
    }
    public void playLevel2()
    {
        SceneManager.LoadSceneAsync("another_test");
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