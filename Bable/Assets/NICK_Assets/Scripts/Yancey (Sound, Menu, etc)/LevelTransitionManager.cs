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

    public static string nextLevelName;
    public List<string> realStats;

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

            float timer = 0f;
            bool skipped = false;
            while (timer < delayBetweenStats)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) ||
                    Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
                {
                    skipped = true;
                    break;
                }
                timer += Time.deltaTime;
                yield return null;
            }

            // if they clicked through, dump all remaining stats instantly
            if (skipped)
            {
                foreach (string remaining in stats)
                    if (!statsTextDisplay.text.Contains(remaining))
                        statsTextDisplay.text += remaining + "\n";
                break;
            }
        }

        if (continueTextDisplay != null)
            continueTextDisplay.gameObject.SetActive(true);

        yield return new WaitUntil(() =>
            Input.GetMouseButtonDown(0) ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.Return)
        );
        SceneManager.LoadScene(nextSceneName);
    }

    public void Start()
    {
        // Example stats to show on the transition screen
        List<string> fakeStats = new List<string>()
        {
            "Enemies Defeated: " + Random.Range(0,10),
            "Weapons Found: " + Random.Range(0,1),
            "Chests Found: " + Random.Range(0,4),
            "Time: " + Random.Range(0,3000),
            "Distance Traveled: " + Random.Range(0,2000),
            "Deaths: " + Random.Range(0,20)
        };

        if (StatManager.Instance != null)
        {
            List<string> realStats = new List<string>()
                {
                    "Time: " + Mathf.FloorToInt(StatManager.Instance.levelTime) + "s",
                    "Enemies Defeated: " + StatManager.Instance.enemiesDefeated,
                    "Weapons Found: " + StatManager.Instance.weaponsFound,
                    "Chests Found: " + StatManager.Instance.chestsFound,
                    "Jumps: " + StatManager.Instance.jumps,
                    "Distance Traveled: " + StatManager.Instance.distanceTraveled + "m",
                    "Damage Taken: " + StatManager.Instance.damageTaken,
                    "Deaths: " + StatManager.Instance.deaths,
                };
            // Start sequence with previous level stats
            StartTransition(realStats, nextLevelName);
        }
        else
        {
            // Start the sequence (with random fake stats)
            StartTransition(fakeStats, nextLevelName);
        }



    }
}