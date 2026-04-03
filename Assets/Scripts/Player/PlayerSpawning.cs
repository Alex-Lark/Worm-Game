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

        private Vector3 spawnPoint = new Vector3(0, 2, 0); //default
        private Vector3 CreatureBuildingSpawnPoint = new Vector3(0, 2, 0);
        private Quaternion spawnRotation = Quaternion.Euler(0, 90, 0); // default

        private Coroutine respawnCoroutine;

        private bool isRegistered = false;
        private bool hasBeenSetup = false;
        private bool spawnPointSet = false;

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

        protected override void OnSpawned(bool asServer)
        {
            if (asServer)
            {
                if (player == null) player = GetComponent<Player>();
                player.wormBodySegments.Clear();
                player.wormConstructor = new WormConstructor(
                    player.wormHead, player.wormBodySegments, player.wormSegmentPrefab,
                    transform, player.WormSegmentCount, player.MaxPartDistance);
                player.wormConstructor.CreateWormSegments();
                return;
            }

            isRegistered = true;
            Debug.Log($"Player spawned | owner: {owner} | isOwner: {isOwner} | localPlayer: {localPlayer}");

            if (isOwner)
            {
                if (player == null) player = GetComponent<Player>();
                LocalPlayer.Register(player);
                OwnerSetup();
            }
            else if (!hasBeenSetup)
            {
                StartCoroutine(FindAndSetupRemoteWorm());
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
        }

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
            RespawnPlayer();
        }
        
        public void SetWormInGameScene()
        {
            Debug.Log("setting worm in game scene");
            
            StartCoroutine(SpawnAtSpawnPoint());
            player.ActivatePlayer();
            if (owner != localPlayer)
            {
                GetComponent<WormRenderer>().EnableRendering();
                player.wormHead.GetComponent<WormHead>().visualHead.GetComponent<MeshRenderer>().enabled = true;
            }
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
        
        private void OwnerSetup()
        {
            Debug.Log("player owner setup");
            player.CurrentState = WormState.Idle;
            player.IsWormGrounded = false;
            player.MaxVelocity = GameParameters.WormMaxVelocity;

            player.wormForwardMovement = GetComponent<WormForwardMovement>();
            player.wormJump = GetComponent<WormJump>();
            player.wormHeadBut = GetComponent<WormHeadBut>();
            
            StartCoroutine(WaitForSegmentsThenSetup());
        }
        
        private IEnumerator WaitForSegmentsThenSetup()
        {
            Debug.Log("waiting for segments then setting up");
            
            float elapsed = 0f;
            while (player.wormBodySegments.Count < player.WormSegmentCount && elapsed < 3f)
            {
                RefreshSegmentsFromChildren();
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (player.wormBodySegments.Count < player.WormSegmentCount)
            {
                Debug.LogError("WaitForSegmentsThenSetup timed out.");
                yield break;
            }

            player.wormConstructor = new WormConstructor(player.wormHead, player.wormBodySegments, player.wormSegmentPrefab, transform, player.WormSegmentCount, player.MaxPartDistance);
            player.wormConstructor.ConstructWorm();
            GetComponent<WormPhysics>().AddCollidersToSegments();
            GetComponent<WormPhysics>().MakeWormKinematic();

            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
                SetWormInGameScene();
            else if (SceneManager.GetActiveScene().name == "CreatureBuilderScene" && gameObject.activeSelf)
                StartCoroutine(SetWormInCreatureBuilderScene());
        }

        private IEnumerator SpawnAtSpawnPoint()
        {
            yield return null;

            deathScreenUI = FindFirstObjectByType<DeathScreenUI>();
            player.thirdPersonCamera = Camera.main?.gameObject;
            LocalPlayer.Instance.canDie = true;
            
            GetComponent<WormPhysics>().MakeWormKinematic();
            player.wormConstructor.ConstructWorm();
            GetComponent<WormPhysics>().AddCollidersToSegments();

            yield return new WaitForFixedUpdate();

            SetWormSpawnPosition(spawnPoint);
            SetWormSpawnRotation(spawnRotation);
            
            GetComponent<WormPhysics>().MakeWormUnkinematic();
            
            
        }
        
        private void RespawnPlayer()
        {
            if (!GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                return;
            }
            
            OnWormRespawn?.Invoke();
            
            player.CurrentState = WormState.Idle;
            if (LocalPlayer.Instance == gameObject.GetComponent<Player>()) player.currentPlayerHealth = GameParameters.DefaultPlayerHealth;
            player.thirdPersonCamera.GetComponent<CinemachineBrain>().enabled = true;
            deathScreenUI.DisableDeathUI();
            
            ServerSideRespawn();
        }
        
        [ServerRpc(requireOwnership: true)]
        private void ServerSideRespawn()
        {
            ObserverSideRespawn();
        }
        
        [ObserversRpc(runLocally: true)]
        private void ObserverSideRespawn()
        {
            player.wormHead.gameObject.SetActive(true);
    
            foreach (Transform bodySegment in player.wormBodySegments)
                bodySegment.gameObject.SetActive(true);
    
            GetComponent<WormRenderer>().enabled = true;
            GetComponent<WormRenderer>().Restart();
    
            StartCoroutine(RespawnSequence());
        }

        private IEnumerator RespawnSequence()
        {
            yield return StartCoroutine(SpawnAtSpawnPoint());

            yield return new WaitForFixedUpdate();

            StartCoroutine(GetComponent<PlayerPartAttachment>().ReactivateAttachedParts());
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

            foreach (GameObject attachedPart in player.attachedWormParts)
            {
                attachedPart.GetComponent<AttachablePart>().ResetJoint();
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
        
        private IEnumerator FindAndSetupRemoteWorm()
        {
            Debug.Log("setting up remote worm");
            hasBeenSetup = true;
            
            yield return new WaitForSeconds(0.5f);
            RefreshSegmentsFromChildren();
            
            float elapsed = 0.5f;
            while (player.wormBodySegments.Count < player.WormSegmentCount && elapsed < 3f)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
                if (player.wormBodySegments.Count == 0) RefreshSegmentsFromChildren();
            }

            if (player.wormBodySegments.Count < player.WormSegmentCount)
            {
                Debug.LogError("FindAndSetupRemoteWorm timed out waiting for segments.");
                yield break;
            }

            SetSegmentsKinematic(true);
            yield return null;

            RebuildSegmentReferences();
            var physics = GetComponent<WormPhysics>();
            physics.AddCollidersToSegments();

            player.wormConstructor = new WormConstructor(player.wormHead, player.wormBodySegments, player.wormSegmentPrefab, transform, player.WormSegmentCount, player.MaxPartDistance);
            player.wormConstructor.ConstructWorm();
            yield return null;

            physics.ResetWormPosition();
            SetSegmentsKinematic(false);
        }
        
        private void RefreshSegmentsFromChildren()
        {
            player.wormBodySegments.Clear();
            foreach (Transform child in transform)
            {
                if (child.GetComponent<CreatureBodySegment>() != null)
                    player.wormBodySegments.Add(child);
            }
        }
        
        private void SetSegmentsKinematic(bool kinematic)
        {
            var headRb = player.wormHead.GetComponent<Rigidbody>();
            headRb.isKinematic = kinematic;
            headRb.useGravity = !kinematic;

            foreach (Transform segment in player.wormBodySegments)
            {
                var rb = segment.GetComponent<Rigidbody>();
                rb.isKinematic = kinematic;
                rb.useGravity = !kinematic;
            }
        }

        private void RebuildSegmentReferences()
        {
            CreaturePart previous = player.wormHead.GetComponent<CreaturePart>();

            for (int i = 0; i < player.wormBodySegments.Count; i++)
            {
                var seg = player.wormBodySegments[i].GetComponent<CreatureBodySegment>();
                seg.previousSegment = previous;

                // Link next segment while we're already iterating
                if (i < player.wormBodySegments.Count - 1)
                    seg.nextSegment = player.wormBodySegments[i + 1].GetComponent<CreatureBodySegment>();

                previous = seg;
            }
        }
        #endregion
    }
}
