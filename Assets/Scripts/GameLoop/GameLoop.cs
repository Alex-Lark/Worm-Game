using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using PurrNet;
using PurrNet.Packing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameLoop
{
    public class GameLoop : MonoBehaviour
    {
        #region Public variables

        [Header("Public Variables")] public static GameLoop Instance;
        public static float TimeLeftInScene;
        public bool IsGameLoopRunning { get; private set; }
        public List<Player.Player> players;
        public Dictionary<Player.Player, Player.Player> networkPlayersDictionary;
        public List<GameObject> partCards = new List<GameObject>();
        public static List<GameObject> partCardsStatic;

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

        private bool readyForGame = false;

        #endregion

        #region Private Variables

        private Coroutine gameLoop;
        private WormGameSceneSwitcher sceneSwitcher;
        private bool sceneReady = false;

        #endregion

        #region Built-In Methods

        private void Awake()
        {
            partCardsStatic = partCards;
            gameObject.AddComponent<GameLoopTimeSyncer>();
            if (!Network.instance.manager.isServer && !Network.instance.manager.isHost)
            {
                Destroy(this);
                return;
            }

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            sceneSwitcher = gameObject.GetComponent<WormGameSceneSwitcher>();
            sceneSwitcher.OnSceneLoaded += HandleSceneLoaded;
            SetDefaultGameLoopSettings();
            StartCoroutine(gameObject.GetComponent<GameLoopTimeSyncer>().SendTimePacket());
        }

        private void OnDestroy()
        {
            ///Destroy(GameLoopTimeSyncer.Instance);
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

                yield return new WaitUntil(() => readyForGame);
                readyForGame = false;
                Network.instance.manager.sceneModule.LoadSceneAsync(GameSceneList.GetRandomGameScene());

                yield return StartCoroutine(MinigameTimer());

                sceneSwitcher.LoadLeaderboardScene();
                yield return StartCoroutine(LeaderboardTimer());
            }

            sceneSwitcher.LoadGameEndScene();
        }

        private IEnumerator RunPartSelectionAndCreatureBuilding()
        {
            sceneReady = false;
            Network.instance.manager.sceneModule.LoadSceneAsync("PartSelectionScene");
            for (int j = 0; j < numberOfPartsPerRound; j++)
            {
                yield return StartCoroutine(PartSelectionTimer());
                
                //PartSelection partSelection = GameObject.FindGameObjectWithTag("PartSelection").GetComponent<PartSelection>();
                //partSelection.EndCardSelection();
            }
        }

        public IEnumerator StartCreatureBuilding()
        {
            yield return Network.instance.manager.sceneModule.LoadSceneAsync("CreatureBuilderScene");
            StartCoroutine(LocalPlayer.Instance.SetWormInCreatureBuilderScene());
            yield return StartCoroutine(CreatureBuilderTimer());
            CreatureBuilder.CreatureBuilder creatureBuilder = GameObject.Find("CreatureBuilder").GetComponent<CreatureBuilder.CreatureBuilder>();
            creatureBuilder.AttachCreatureParts();

            readyForGame = true;
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
    
    public class GameLoopTimeSyncer : PurrMonoBehaviour
    {
        public static GameLoopTimeSyncer Instance;
        void Start()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        public struct TimePacket : IPackedAuto
        {
            public float time;
        }

        public IEnumerator SendTimePacket()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                Network.instance.manager.SendToAll<TimePacket>(new TimePacket { time = GameLoop.TimeLeftInScene });
            }
        }
        public override void Subscribe(NetworkManager manager, bool asServer)
        {
            manager.Subscribe<TimePacket>(SyncClock, asServer);
        }

        public override void Unsubscribe(NetworkManager manager, bool asServer)
        {
            manager.Unsubscribe<TimePacket>(SyncClock, asServer);
        }

        void SyncClock(PlayerID playerID, TimePacket timePacket, bool asServer)
        {
            if(Network.instance.manager.isServer || Network.instance.manager.isHost) return;
            GameLoop.TimeLeftInScene = timePacket.time;
        }
    }
}



