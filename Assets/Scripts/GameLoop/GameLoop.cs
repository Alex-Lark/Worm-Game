using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameLoop : MonoBehaviour
{
    public static GameLoop Instance;
    public float TimeLeftInScene { get; private set; }
    public bool isGameLoopRunning { get; private set; }
    public List<Player.Player> players;
    public List<GameObject> partCards = new List<GameObject>();
    
    [Header("modifiable game loop settings")] 
    private int numberOfRounds;
    private int numberOfPartsPerRound;
    private int timePerPartSelection;
    private int timePerCreatureBuilding;
    private int timePerMinigame;

    private int timeForLeaderboard;
    private bool skipCreatureBuilding1stRound = false;

    private Coroutine gameLoop;
    private WormGameSceneSwitcher sceneSwitcher;
    private bool sceneReady = false;
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
        sceneSwitcher.OnSceneLoaded += HandleSceneLoaded;
    }

    public void Reset()
    {
        StopCoroutine(gameLoop);
        TimeLeftInScene = 0;
        isGameLoopRunning = false;
    }

    public void StartGame()
    {
        isGameLoopRunning = true;
        gameLoop = StartCoroutine(RunGameLoop());
    }

    private IEnumerator RunGameLoop()
    {
    
        for (int i = 0; i < numberOfRounds; i++)
        {
            if (!skipCreatureBuilding1stRound || (i > 0))
            {
                sceneReady = false;
                sceneSwitcher.LoadPartSelectionScene();
                for (int j = 0; j < numberOfPartsPerRound; j++)
                {
                    yield return StartCoroutine(PartSelectionTimer());
                    PartSelection partSelection = GameObject.FindGameObjectWithTag("PartSelection").GetComponent<PartSelection>();
                    partSelection.EndCardSelection();
                }
                
                sceneSwitcher.LoadCreatureBuilderScene();
                Player.Player.Instance.SetWormInCreatureBuilderScene();
                yield return StartCoroutine(CreatureBuilderTimer());
                CreatureBuilder.CreatureBuilder creatureBuilder = GameObject.Find("CreatureBuilder").GetComponent<CreatureBuilder.CreatureBuilder>();
                creatureBuilder.AttachCreatureParts();
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
    
    private void HandleSceneLoaded()
    {
        sceneReady = true;
    }
}
