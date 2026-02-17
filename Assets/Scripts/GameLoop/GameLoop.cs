using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLoop
{
    public class GameLoop : MonoBehaviour
    {
        #region Public variables
        [Header("Public Variables")] 
        
        public static GameLoop Instance;
        public float TimeLeftInScene { get; private set; }
        public bool IsGameLoopRunning { get; private set; }
        public List<Player.Player> players;
        public Dictionary<Player.Player, Player.Player> networkPlayersDictionary;
        public List<GameObject> partCards = new List<GameObject>();
        
        #endregion
    
        #region Modifiable Loop Settings
        [Header("modifiable game loop settings")] 
        
        private int numberOfRounds;
        private int numberOfPartsPerRound;
        private int timePerPartSelection;
        private int timePerCreatureBuilding;
        private int timePerMinigame;
        private int timeForLeaderboard;
        private bool skipCreatureBuilding1StRound = false;
        
        #endregion
        
        #region Private Variables

        private Coroutine gameLoop;
        private WormGameSceneSwitcher sceneSwitcher;
        private bool sceneReady = false;
        
        #endregion
        
        #region Built-In Methods
        
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
            sceneSwitcher = gameObject.GetComponent<WormGameSceneSwitcher>();
            sceneSwitcher.OnSceneLoaded += HandleSceneLoaded;
            SetDefaultGameLoopSettings();
        }
        
        #endregion
        
        #region Public Methods
        
        public void Reset()
        {
            if (gameLoop == null) return;
            
            StopCoroutine(gameLoop);
            StopAllCoroutines();
            
            TimeLeftInScene = 0;
            IsGameLoopRunning = false;
            sceneReady = false;
            
            foreach (Player.Player player in players)
            {
                player.ResetPlayer();
            }
        }

        public void StartGame()
        {
            IsGameLoopRunning = true;
            gameLoop = StartCoroutine(RunGameLoop());
        }
        
        #endregion
        
        #region Private Methods
        
        private void HandleSceneLoaded()
        {
            sceneReady = true;
        }

        private void SetDefaultGameLoopSettings()
        {
            numberOfRounds = GameParameters.DefaultNumberOfRounds;
            numberOfPartsPerRound = GameParameters.DefaultNumberOfPartsPerRound;
            timePerPartSelection = GameParameters.DefaultTimePerPartSelection;
            timePerCreatureBuilding = GameParameters.DefaultTimePerCreatureBuilding;
            timePerMinigame = GameParameters.DefaultTimePerMinigame;

            timeForLeaderboard = GameParameters.TimeForLeaderboard;
        }
        
        #endregion
        
        #region Game Loop Logic

        private IEnumerator RunGameLoop()
        {
    
            for (int i = 0; i < numberOfRounds; i++)
            {
                if (!skipCreatureBuilding1StRound || (i > 0))
                {
                    yield return StartCoroutine(RunPartSelectionAndCreatureBuilding());
                }
            
                sceneSwitcher.LoadGameScene();
            
                yield return StartCoroutine(MinigameTimer());
            
                sceneSwitcher.LoadLeaderboardScene();
                yield return StartCoroutine(LeaderboardTimer());
            }
        
            sceneSwitcher.LoadGameEndScene();
        }

        private IEnumerator RunPartSelectionAndCreatureBuilding()
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
        
        #endregion
        
        #region Game Loop Timers

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
        
        #endregion
    }
}
