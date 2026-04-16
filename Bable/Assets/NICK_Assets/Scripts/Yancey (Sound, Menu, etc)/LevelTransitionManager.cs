using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelTransitionManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI completeTextDisplay;
    public TextMeshProUGUI statsTextDisplay;
    public TextMeshProUGUI continueTextDisplay; 

    [Header("Timing Settings")]
    public float delayBetweenStats = 1.0f;

    [Header("Animation Hooks")]
    public Animator playerAnimator; 

    public string nextLevelName;

    public void StartTransition(List<string> levelStats, string nextSceneName)
    {

        if (continueTextDisplay != null)
        {
            continueTextDisplay.gameObject.SetActive(false);
        }


        // Trigger player animation to walk towards the right (or walk up stairs later)
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("Is_running", true);
            playerAnimator.SetBool("FacingRight", true);
            playerAnimator.SetFloat("Speed", 1.0f);
            playerAnimator.SetFloat("Horizontal_Velocity", 1.0f);
        }
        
        StartCoroutine(DisplayStatsSequence(levelStats, nextSceneName));
    }

    private IEnumerator DisplayStatsSequence(List<string> stats, string nextSceneName)
    {
        
        completeTextDisplay.gameObject.SetActive(true);
        yield return new WaitForSeconds(delayBetweenStats);

        foreach (string stat in stats)
        {
            statsTextDisplay.text += stat + "\n";
            yield return new WaitForSeconds(delayBetweenStats);
        }

        if (continueTextDisplay != null)
        {
            continueTextDisplay.gameObject.SetActive(true);
        }


        yield return new WaitUntil(() => 
            Input.GetMouseButtonDown(0) || 
            Input.GetKeyDown(KeyCode.Space) || 
            Input.GetKeyDown(KeyCode.E) || 
            Input.GetKeyDown(KeyCode.Return)
        );

        SceneManager.LoadScene(nextSceneName);
    }

    void Start()
    {
        nextLevelName = SceneManager.GetSceneByBuildIndex(GameManager.Instance.currentLevelIndex + 1).name;
        // Example stats to show on the transition screen
        List<string> fakeStats = new List<string>()
        {
            "Enemies Defeated: 10",
            "Weapons Found: 3",
            "Chests Found: 2",
            "Time: 120s",
            "Distance Traveled: 500m",
            "Deaths: 1"
        };

        if(GameManager.Instance != null)
            {
                List<string> realStats = new List<string>()
                {
                    "Enemies Defeated: " + GameManager.Instance.enemiesDefeated,
                    "Weapons Found: " + GameManager.Instance.weaponsFound,
                    "Chests Found: " + GameManager.Instance.chestsFound,
                    "Time: " + Mathf.FloorToInt(GameManager.Instance.levelTimer) + "s",
                    "Distance Traveled: " + GameManager.Instance.distanceTraveled + "m",
                    "Deaths: " + GameManager.Instance.deaths
                };
            }

        // Start the sequence
        StartTransition(fakeStats, nextLevelName); 
    }
}