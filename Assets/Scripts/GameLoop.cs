using System.Collections;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    public static GameLoop Instance;
    public float TimeLeftInScene { get; private set; }
    
    [Header("modifiable game loop settings")] 
    private int numberOfRounds;
    private int numberOfPartsPerRound;
    private int timePerPartSelection;
    private int timePerCreatureBuilding;
    private int timePerMinigame;

    private int timeForLeaderboard;
    private bool skipCreatureBuilding1stRound = false;

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
        Debug.Log(TimeLeftInScene);
    }

    public void StartGame()
    {
        StartCoroutine(RunGameLoop());
    }

    private IEnumerator RunGameLoop()
    {
    
        for (int i = 0; i < numberOfRounds; i++)
        {
            if (i > 0 && skipCreatureBuilding1stRound)
            {
                sceneSwitcher.LoadPartSelectionScene();
                yield return StartCoroutine(PartSelectionTimer());
                
                sceneSwitcher.LoadCreatureBuilderScene();
                yield return StartCoroutine(CreatureBuilderTimer());
            }
            
            sceneSwitcher.LoadGameScene();
            
            yield return StartCoroutine(MinigameTimer());
            
            sceneSwitcher.LoadLeaderboardScene();
            yield return StartCoroutine(LeaderboardTimer());
        }
        
        sceneSwitcher.LoadGameEndScene();
    }

    private IEnumerator PartSelectionTimer()
    {
        TimeLeftInScene = timePerPartSelection;

        while (TimeLeftInScene > 0)
        {
            TimeLeftInScene -= Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator CreatureBuilderTimer()
    {
        TimeLeftInScene = timePerCreatureBuilding;

        while (TimeLeftInScene > 0)
        {
            TimeLeftInScene -= Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator MinigameTimer()
    {
        TimeLeftInScene = timePerMinigame;
        
        while (TimeLeftInScene > 0)
        {
            TimeLeftInScene -= Time.deltaTime;
            yield return null;
        }
        
    }

    private IEnumerator LeaderboardTimer()
    {
        TimeLeftInScene = timeForLeaderboard;

        while (TimeLeftInScene > 0)
        {
            TimeLeftInScene -= Time.deltaTime;
            yield return null;
        }
    }
}
