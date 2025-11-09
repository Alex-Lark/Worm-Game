using System;
using System.Collections;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    public static GameLoop Instance;
    
    [Header("modifiable game loop settings")] 
    private int numberOfRounds;
    private int numberOfPartsPerRound;
    private int timePerPartSelection;
    private int timePerCreatureBuilding;
    private int timePerMinigame;

    private int timeForLeaderboard;

    private WormGameSceneSwitcher sceneSwitcher;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        numberOfRounds = GameParameters.defaultNumberOfRounds;
        numberOfPartsPerRound = GameParameters.defaultNumberOfPartsPerRound;
        timePerPartSelection = GameParameters.defaultTimePerPartSelection;
        timePerCreatureBuilding = GameParameters.defaultTimePerCreatureBuilding;
        timePerMinigame = GameParameters.defaultTimePerMinigame;

        timeForLeaderboard = GameParameters.timeForLeaderboard;

        sceneSwitcher = gameObject.GetComponent<WormGameSceneSwitcher>();
    }

    private void Update()
    {
        throw new NotImplementedException();
    }

    public void StartGame()
    {
        StartCoroutine(RunGameLoop());
    }

    private IEnumerator RunGameLoop()
    {
        for (int i = 0; i < numberOfRounds; i++)
        {
            sceneSwitcher.LoadPartSelectionScene();
            yield return StartCoroutine(PartSelectionTimer());

            sceneSwitcher.LoadCreatureBuilderScene();
            yield return StartCoroutine(CreatureBuilderTimer());

            sceneSwitcher.LoadGameScene();
            yield return StartCoroutine(MinigameTimer());

            // Switch to leaderboard scene
            yield return StartCoroutine(LeaderboardTimer());
        }

        Debug.Log("All rounds completed!");
    }

    private IEnumerator PartSelectionTimer()
    {
        // Switch to part selection scene
        Debug.Log("Part Selection phase started");
        yield return new WaitForSeconds(timePerPartSelection);
    }

    private IEnumerator CreatureBuilderTimer()
    {
        // Switch to creature builder
        Debug.Log("Creature Builder phase started");
        yield return new WaitForSeconds(timePerCreatureBuilding);
    }

    private IEnumerator MinigameTimer()
    {
        // Switch to minigame
        Debug.Log("Minigame phase started");
        yield return new WaitForSeconds(timePerMinigame);
    }

    private IEnumerator LeaderboardTimer()
    {
        // Switch to leaderboard
        Debug.Log("Leaderboard phase started");
        yield return new WaitForSeconds(timeForLeaderboard);
    }
    
}
