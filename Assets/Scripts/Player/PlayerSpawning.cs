using System;
using System.Collections;
using System.Collections.Generic;
using CreatureBuilder;
using CreatureParts;
using PurrNet;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Player
{
    public class PlayerSpawning : NetworkBehaviour
    {
        #region public variables

        public Player player;
        public bool canRespawn = true;
        public DeathScreenUI deathScreenUI;

        public Vector3 spawnPoint = new Vector3(); //default
        private Vector3 CreatureBuildingSpawnPoint = new Vector3(0, 2, 0);
        private Quaternion spawnRotation = Quaternion.Euler(0, 90, 0); // default

        private Coroutine respawnCoroutine;

        private bool isRegistered = false;
        private bool hasBeenSetup = false;
        private bool spawnPointSet = false;

        private float TimeToWaitForSpawnpointSet = 5f;

        public event Action OnWormRespawn;

        #endregion

        #region Built-In Methods

        void Start()
        {
            player = GetComponent<Player>();
            player.playerSpawning = this;

            SceneManager.sceneLoaded += OnSceneLoaded;

            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                deathScreenUI = FindFirstObjectByType<DeathScreenUI>();
                LocalPlayer.Instance.canDie = true;
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        protected override void OnDespawned()
        {
            LocalPlayer.Unregister(player);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        #endregion
        
        #region Public Methods

        public void SetSpawnPoint(GameObject inputSpawnpoint)
        {
            spawnPoint = inputSpawnpoint.transform.position;
            spawnRotation = inputSpawnpoint.transform.rotation;
            spawnPointSet = true;
            Debug.Log("spawnpoint set with position: " + spawnPoint + " ,rotation: " + spawnRotation);
            SyncSpawnPointServer(spawnPoint, spawnRotation, player);
        }
        
        [ServerRpc(runLocally: true)]
        private void SyncSpawnPointServer(Vector3 position, Quaternion rotation, Player syncPlayer)
        {
            SyncSpawnPointObserver(position, rotation, syncPlayer);
        }
        
        [ObserversRpc(runLocally: true)]
        private void SyncSpawnPointObserver(Vector3 position, Quaternion rotation, Player syncPlayer)
        {
            if (syncPlayer != player) return;
            spawnPoint = position;
            spawnRotation = rotation;
        }
        
        public void SetWormInGameScene()
        {
            if (player.isOwner)
            {
                SetWormInGameSceneAsOwner();
            }
            else
            {
                SetWormInGameSceneAsNonOwner();
            }
            
        }
        
        [ServerRpc]
        public void SetKinematicStateServer(bool isKinematic, Player playerToUpdate)
        {
            SetKinematicStateObserver(isKinematic, playerToUpdate);
        }

        private void SetWormInGameSceneAsOwner()
        {
            Debug.Log($"Setting worm {player.PlayerName} in game scene as owner");
            StartCoroutine(SpawnAtSpawnPoint());
            player.ActivatePlayer();
        }

        private void SetWormInGameSceneAsNonOwner()
        {
            Debug.Log($"Setting worm {player.PlayerName} in game scene as non owner");
            GetComponent<WormRenderer>().EnableRendering();
            player.wormHead.GetComponent<WormHead>().visualHead.GetComponent<MeshRenderer>().enabled = true;
        }
        
        public IEnumerator SetWormInCreatureBuilderScene()
        {
            LocalPlayer.Instance.canDie = false;
            Debug.Log("setting worm in creature builder");
            yield return null;
    
            var wormPhysics = GetComponent<WormPhysics>();
            wormPhysics.MakeWormKinematic();
            yield return null;
            wormPhysics.ResetWormOrientation();
            wormPhysics.PositionWormSegments(CreatureBuildingSpawnPoint);
            yield return null;
            yield return null;
    
            player.DeactivatePlayer();

            if (owner != localPlayer)
            {
                GetComponent<WormRenderer>().DisableRendering();
                player.wormHead.GetComponent<WormHead>().visualHead.GetComponent<MeshRenderer>().enabled = false;
            }

            GetComponent<PlayerPartAttachment>().AddAlreadyAttachedParts();
        }
        
        #endregion
        
        #region Private Methods

        [ObserversRpc]
        private void SetKinematicStateObserver(bool isKinematic, Player playertoUpdate)
        {
            if (playertoUpdate != player) return;
            
            GetComponent<WormPhysics>().ToggleWormKinematics(isKinematic);
        }
        
        private void SetWormSpawnRotation(Quaternion orientation)
        {
            player.wormHead.rotation = orientation;
            player.wormVisualHead.localRotation = Quaternion.identity;
            foreach (Transform segment in player.wormBodySegments)
            {
                segment.rotation = orientation;
            }
        }
        
        private void SetWormSpawnPosition(Vector3 spawnPosition)
        {
            Debug.Log("spawning with position: " + spawnPosition);
            if (player.wormHead == null) return;
        
            player.wormHead.position = spawnPosition;

            Vector3 currentPos = player.wormHead.position;
            Vector3 backDir = -player.wormHead.forward;

            for (int i = 0; i < player.wormBodySegments.Count; i++)
            {
                currentPos += backDir * GameParameters.SegmentMaxPartDistance;
                Transform segment = player.wormBodySegments[i];
                segment.position = currentPos;
                segment.rotation = player.wormHead.rotation;
            }
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "CreatureBuilderScene")
            {
                StartCoroutine(SetWormInCreatureBuilderScene());
            }
            else if (GameSceneList.IsSceneAGameScene(scene.name))
            {
                
                SetWormInGameScene();
            }
            else
            {
                GetComponent<Player>().DeactivatePlayer();
            }
        }
        
        #endregion
        
        #region Initial Spawning
        
        protected override void OnSpawned(bool asServer)
        {
            if (player == null) player = GetComponent<Player>();
            isRegistered = true;
            Debug.Log($"Player spawned | owner: {owner} | isOwner: {isOwner} | localPlayer: {localPlayer} | asServer: {asServer}");
            
            if (owner == localPlayer && !asServer)
            {
                StartCoroutine(InitialSpawnAsOwner());
            }
            else //already spawned worm
            {
                //GetComponent<WormConstructor>().AddSegmentJoints();
            }
        }

        private IEnumerator InitialSpawnAsOwner()
        {
            Debug.Log("setting up as owner == localPlayer");
            LocalPlayer.Register(player);
                
            player.CurrentState = WormState.Idle;
            player.IsWormGrounded = false;
            player.MaxVelocity = GameParameters.WormMaxVelocity;

            player.wormForwardMovement = GetComponent<WormForwardMovement>();
            player.wormJump = GetComponent<WormJump>();
            player.wormHeadBut = GetComponent<WormHeadBut>();
                
            yield return StartCoroutine(SpawnAsServer(player));
            
            GetComponent<WormConstructor>().ConstructWorm();
            GetComponent<WormPhysics>().AddCollidersToSegments();
            //GetComponent<WormConstructor>().AddSegmentJointsAsServer(player);
            GetComponent<WormConstructor>().AddSegmentJoints();
            SetKinematicStateServer(true, player);

            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
                SetWormInGameScene();
            else if (SceneManager.GetActiveScene().name == "CreatureBuilderScene" && gameObject.activeSelf)
                StartCoroutine(SetWormInCreatureBuilderScene());
            
            yield return null;
        }
        
        [ServerRpc]
        private IEnumerator SpawnAsServer(Player playerToSpawn)
        {
            if (playerToSpawn != player) yield break;
            
            Debug.Log($"Spawning player {player.PlayerName} as server");
            
            player.wormBodySegments.Clear();
            GetComponent<WormConstructor>().CreateWormSegments();

            yield return null;
        }
        
        #endregion
        
        #region Respawning
        
        public void TryToRespawn()
        {
            if (canRespawn && GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                respawnCoroutine = StartCoroutine(RespawnTimer());
            }
        }
        
        public IEnumerator RespawnTimer()
        {
            if (!GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                yield break;
            }
            
            float timeLeft = GameParameters.PlayerRespawnTimeInSeconds;
    
            while (timeLeft > 0)
            {
                deathScreenUI.respawnText.text = "Respawning in " + Mathf.Ceil(timeLeft);
                yield return new WaitForSeconds(1f);
                timeLeft -= 1f;
            }
    
            deathScreenUI.respawnText.text = "Respawning...";
            StartCoroutine(RespawnPlayer());
        }

        private IEnumerator RespawnPlayer()
        {
            if (!GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                yield break;
            }

            OnWormRespawn?.Invoke();

            if (player == LocalPlayer.Instance)
            {
                yield return StartCoroutine(RespawnPlayerAsOwner());
            }

            RespawnPlayerAsNonOwnerServerRPC(player);
        }
        
        private IEnumerator RespawnPlayerAsOwner()
        {
            Debug.Log("Respawning player as Owner" + player.PlayerName);
            player.CurrentState = WormState.Idle;
            player.currentPlayerHealth = GameParameters.DefaultPlayerHealth;
            player.thirdPersonCamera.GetComponent<CinemachineBrain>().enabled = true;
            deathScreenUI.DisableDeathUI();
            
            yield return StartCoroutine(SpawnAtSpawnPoint());
        }

        [ServerRpc]
        private void RespawnPlayerAsNonOwnerServerRPC(Player playerToRespawn)
        {
            RespawnPlayerAsNonOwnerObserversRPC(playerToRespawn);
        }
        
        [ObserversRpc]
        private void RespawnPlayerAsNonOwnerObserversRPC(Player playerToRespawn)
        {
            if (playerToRespawn != player) return;
            
            RespawnPlayerAsNonOwner();
        }

        private void RespawnPlayerAsNonOwner()
        {
            Debug.Log("Respawning player as Nonowner" + player.PlayerName);
            GetComponent<WormRenderer>().enabled = true;
            GetComponent<WormRenderer>().Restart();
            
            player.EnablePartForRespawn(player.wormHead.gameObject);
            foreach (Transform bodySegment in player.wormBodySegments)
                player.EnablePartForRespawn(bodySegment.gameObject);
            
            StartCoroutine(GetComponent<PlayerPartAttachment>().ReactivateAttachedParts());
        }
        
        private IEnumerator SpawnAtSpawnPoint()
        {
            float elapsed = 0f;
            while (!spawnPointSet && elapsed < TimeToWaitForSpawnpointSet)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (!spawnPointSet)
            {
                Debug.LogWarning("SpawnAtSpawnPoint timed out waiting for spawn point to be set.");
                yield break;
            }
            
            yield return null;

            deathScreenUI = FindFirstObjectByType<DeathScreenUI>();
            player.thirdPersonCamera = Camera.main?.gameObject;
            player.canDie = true;
            
            SetKinematicStateServer(true, player);
            player.GetComponent<WormConstructor>().ConstructWorm();
            GetComponent<WormPhysics>().AddCollidersToSegments();

            yield return new WaitForFixedUpdate();

            SetWormSpawnRotation(spawnRotation);
            SetWormSpawnPosition(spawnPoint);
            
            SetKinematicStateServer(false, player);
        }

        #endregion
    }
}
