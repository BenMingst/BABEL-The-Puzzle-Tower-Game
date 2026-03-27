using UnityEngine;
using UnityEngine.SceneManagement;

public class TryAgainButton : MonoBehaviour
{
    void Start()
    {
        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        Debug.Log("TryAgain clicked");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}