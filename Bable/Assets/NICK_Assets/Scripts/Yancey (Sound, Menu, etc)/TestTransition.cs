using System.Collections.Generic;
using UnityEngine;

public class TestTransition : MonoBehaviour
{
    void Start()
    {
        LevelTransitionManager transitionManager = GetComponent<LevelTransitionManager>();
        
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
        transitionManager.StartTransition(fakeStats, "Level2"); 
    }
}