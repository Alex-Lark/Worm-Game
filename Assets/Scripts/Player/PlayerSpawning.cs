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
        
        private Coroutine respawnCoroutine;
        
        private bool isRegistered = false;
        private bool hasBeenSetup = false;
        
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
            if (player == null) player = GetComponent<Player>();
    
            if (asServer) return;

            isRegistered = true;
            
            RequestOwnershipServerRpc(localPlayer.Value);
            
            if (!isOwner && !hasBeenSetup)
            {
                StartCoroutine(FindAndSetupRemoteWorm());
            }
        }
        
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        protected override void OnDespawned() {
            LocalPlayer.Unregister(player);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
        {
            Debug.Log("Player owner changed");
            if (asServer && newOwner.HasValue)
            {
                // Only server creates networked segments
                if (player == null) player = GetComponent<Player>();
                player.wormBodySegments.Clear();
                player.wormConstructor = new WormConstructor(player.wormHead, player.wormBodySegments, player.wormSegmentPrefab, transform, player.WormSegmentCount, player.MaxPartDistance);
                player.wormConstructor.CreateWormSegments();
            }
        
            if (!asServer && isOwner)
            {
                if (player == null) player = GetComponent<Player>();
                LocalPlayer.Register(player);
                OwnerSetup();
            }
        }
        
        #endregion

        #region Public Methods

        [ServerRpc]
        public void AddAttachedPartServerSide(GameObject prefab, Vector3 position, Quaternion rotation, GameObject wormSegment, float partMass, GameObject player)
        {
            GameObject networkedPart = Instantiate(prefab, position, rotation, player.transform);
            
            networkedPart.GetComponent<AttachablePart>().GiveOwnership(player.GetComponent<NetworkTransform>().owner);
            
            networkedPart.GetComponent<PartDragging>().DeselectPart();
            networkedPart.GetComponent<PartDragging>().enabled = false;
            
            var rb = networkedPart.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            
            LegPart legPart = networkedPart.GetComponent<LegPart>();
            if (legPart != null)
            {
                player.GetComponent<Player>().MaxVelocity += GameParameters.LegMaxVelocityIncrease;
                legPart.enabled = true;
            }
            
            Rigidbody partRigidbody = networkedPart.GetComponent<Rigidbody>();
            if (partRigidbody == null)
                partRigidbody = networkedPart.AddComponent<Rigidbody>();
            
            Rigidbody segmentRigidbody = wormSegment.GetComponent<Rigidbody>();
            if (segmentRigidbody != null)
                networkedPart.GetComponent<AttachablePart>().ConfigureRigidBody(partRigidbody, segmentRigidbody, partMass);
            
            Transform endPoint = networkedPart.GetComponent<PartDragging>().endPoint;
            if (endPoint == null)
            {
                Debug.LogError("No endPoint found on part: " + networkedPart.name);
                return;
            }
            
            networkedPart.GetComponent<AttachablePart>().ConfigureHingeJoint(segmentRigidbody, endPoint);
            player.GetComponent<WormPhysics>().IgnorePartCollisionWithWorm(networkedPart, wormSegment.transform);
            AddAttachedPartForClients(networkedPart, player, wormSegment);
            SyncLegOrderRpc(player);
            
        }

        [ObserversRpc]
        public void AddAttachedPartForClients(GameObject part, GameObject partPlayer, GameObject wormSegment)
        {
            //this method adds the part that was already created server-side and attaches it for all clients for smoother appearance
            part.GetComponent<AttachablePart>().GiveOwnership(player.GetComponent<NetworkTransform>().owner);
            
            part.GetComponent<PartDragging>().DeselectPart();
            part.GetComponent<PartDragging>().enabled = false;
            
            var rb = part.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            
            LegPart legPart = part.GetComponent<LegPart>();
            if (legPart != null)
            {
                player.GetComponent<Player>().MaxVelocity += GameParameters.LegMaxVelocityIncrease;
                legPart.enabled = true;
            }
            
            Rigidbody partRigidbody = part.GetComponent<Rigidbody>();
            if (partRigidbody == null)
                partRigidbody = part.AddComponent<Rigidbody>();
            
            Rigidbody segmentRigidbody = wormSegment.GetComponent<Rigidbody>();
            if (segmentRigidbody != null)
                part.GetComponent<AttachablePart>().ConfigureRigidBody(part.GetComponent<Rigidbody>(), segmentRigidbody, part.GetComponent<Rigidbody>().mass);
            Transform endPoint = part.GetComponent<PartDragging>().endPoint;
            if (endPoint == null)
            {
                Debug.LogError("No endPoint found on part: " + part.name);
                return;
            }
            part.GetComponent<AttachablePart>().ConfigureHingeJoint(segmentRigidbody, endPoint);
            player.GetComponent<WormPhysics>().IgnorePartCollisionWithWorm(part, wormSegment.transform);
        }
        
        [ObserversRpc]
        public void SyncLegOrderRpc(GameObject specificPlayer)
        {
            List<LegPart> legParts = new List<LegPart>();
            foreach (GameObject part in specificPlayer.GetComponent<Player>().attachedWormParts)
            {
                LegPart leg = part.GetComponent<LegPart>();
                if (leg != null)
                    legParts.Add(leg);
            }

            float totalTime = GameParameters.LegMoveTime;
            for (int i = 0; i < legParts.Count; i++)
            {
                legParts[i].timeOffset = i * (totalTime / legParts.Count);
            }
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
            
            // player.wormHead.GetComponent<Rigidbody>().isKinematic = false;
             // foreach (Transform segment in player.wormBodySegments)
             // {
             //     segment.GetComponent<Rigidbody>().isKinematic = false;
             // }
            
            StartCoroutine(SetupAfterSceneLoad());
            player.ActivatePlayer();
        }
        
        public IEnumerator SetWormInCreatureBuilderScene()
        {
            LocalPlayer.Instance.canDie = false;
            Debug.Log("setting worm in creature builder");
            yield return new WaitForSeconds(0.2f);
            yield return null;
            
            var wormPhysics = GetComponent<WormPhysics>();
            
            wormPhysics.ResetWormPhysics();
            
            yield return null;
            
            wormPhysics.ResetWormOrientation();
            
            wormPhysics.PositionWormSegments(new Vector3(0, 2, 0));
            
            yield return null;
            
            yield return null;
            
            player.DeactivatePlayer();
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
            GetComponent<WormPhysics>().ResetWormPhysics();

            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
                SetWormInGameScene();
            else if (SceneManager.GetActiveScene().name == "CreatureBuilderScene" && gameObject.activeSelf)
                StartCoroutine(SetWormInCreatureBuilderScene());
        }
        
        private IEnumerator SetupAfterSceneLoad()
        {
            Debug.Log("setting up after scene load as server: " + isServer);
            yield return null;

            deathScreenUI = FindFirstObjectByType<DeathScreenUI>();
            if (deathScreenUI == null)
            {
                Debug.LogError("couldn't find death screen UI");
            }
            player.thirdPersonCamera = Camera.main?.gameObject;
            
            GetComponent<WormPhysics>().ResetPlayerPhysics();
            
            GetComponent<WormPhysics>().AddCollidersToSegments();
            Debug.Log("added colliders to segments");
            
            SetWormSpawnPosition(new Vector3(0, 2, 0));
            SetWormSpawnOrientation(Quaternion.Euler(0, 90, 0));
            player.wormConstructor.ConstructWorm();
            Debug.Log("constructed worm");
            
            LocalPlayer.Instance.wormForwardMovement.SetVariables();
            LocalPlayer.Instance.canDie = true;
        }
        
        private void RespawnPlayer()
        {
            if (!GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                return;
            }
            
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
            {
                bodySegment.gameObject.SetActive(true);
            }
            
            GetComponent<WormRenderer>().enabled = true;
            GetComponent<WormRenderer>().Restart();
            
            GetComponent<WormPhysics>().ResetPlayerPhysics();
            GetComponent<PlayerSpawning>().SetWormSpawnPosition(new Vector3(0, 2, 0));
            GetComponent<PlayerSpawning>().SetWormSpawnOrientation(Quaternion.Euler(0, 90, 0));
            player.wormConstructor.ConstructWorm();
            GetComponent<WormPhysics>().AddCollidersToSegments();
            StartCoroutine(ReactivateAttachedParts());
        }

        private void SetWormSpawnOrientation(Quaternion orientation)
        {
            player.wormHead.rotation = orientation;
            player.wormVisualHead.localRotation = Quaternion.identity;
            foreach (Transform segment in player.wormBodySegments)
            {
                segment.rotation = orientation;
            }
        }
        
        private IEnumerator ReactivateAttachedParts()
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
    
            foreach (GameObject attachedPart in player.attachedWormParts)
            {
                attachedPart.SetActive(true);
                attachedPart.GetComponent<AttachablePart>().enabled = true;
                attachedPart.GetComponent<AttachablePart>().ResetJoint();
            }
    
            GetComponent<WormPhysics>().IgnoreWormSelfCollision();
        }
        
        private void SetWormSpawnPosition(Vector3 spawnPosition)
        {
            if (player.wormHead == null) return;
        
            player.wormHead.position = spawnPosition;
            Rigidbody headRb = player.wormHead.GetComponent<Rigidbody>();
            if (headRb != null)
            {
                headRb.useGravity = true;
                headRb.isKinematic = false;
                headRb.linearVelocity = Vector3.zero;
                headRb.angularVelocity = Vector3.zero;
            }

            Vector3 currentPos = player.wormHead.position;
            Vector3 backDir = -player.wormHead.forward;

            for (int i = 0; i < player.wormBodySegments.Count; i++)
            {
                currentPos += backDir * GameParameters.SegmentMaxPartDistance;
                Transform segment = player.wormBodySegments[i];
                segment.position = currentPos;
                segment.rotation = player.wormHead.rotation;
            
                Rigidbody segmentRb = segment.GetComponent<Rigidbody>();
                segmentRb.useGravity = true;
                segmentRb.isKinematic = false;
                segmentRb.linearVelocity = Vector3.zero;
                segmentRb.angularVelocity = Vector3.zero;
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
        
        [ServerRpc(requireOwnership: false)]
        private void RequestOwnershipServerRpc(PlayerID caller = default)
        {
            if (owner == null || owner.ToString() == "Server")
            {
                GiveOwnership(caller);
            }
        }
        #endregion
    }
}
