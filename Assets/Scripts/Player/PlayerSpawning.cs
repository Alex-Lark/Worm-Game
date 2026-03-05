using System.Collections;
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
        
        #endregion
        
        #region Built-In Methods
        
        void Start()
        {
            player = GetComponent<Player>();
            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                SpawnInGameScene();
            }

            deathScreenUI = FindFirstObjectByType<DeathScreenUI>().GetComponent<DeathScreenUI>();
        }
        
        #endregion

        #region Public Methods
        
        public void SpawnInCreatureBuildingScene()
        {
            // Debug.Log("Spawning in creature builder scene");
            //
            // StopAllCoroutines();
            // player.CancelDeath();
            //
            // WormPhysics wormPhysics = GetComponent<WormPhysics>();
            // wormPhysics.ResetWormPhysics();
            // SetWormSpawnOrientation(Quaternion.identity);
            // wormPhysics.PositionWormSegments(new Vector3(0, 2, 0));
            // player.DeactivatePlayer();
        }
        
        public void SpawnInGameScene()
        {
            // Debug.Log("Spawning in game scene");
            // player.wormHead.GetComponent<Rigidbody>().isKinematic = false;
            //
            // foreach (Transform segment in player.wormBodySegments)
            // {
            //     segment.GetComponent<Rigidbody>().isKinematic = false;
            // }
            // StartCoroutine(SetupAfterSceneLoad());
            // player.ActivatePlayer();
            // player.currentPlayerHealth = GameParameters.DefaultPlayerHealth;
            // player.CurrentState = WormState.Idle;
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
        
        #endregion
        
        #region Private Methods
        
        private IEnumerator SetupAfterSceneLoad()
        {
            yield return null;

            deathScreenUI = FindFirstObjectByType<DeathScreenUI>(); 
            player.thirdPersonCamera = Camera.main?.gameObject;
            
            GetComponent<WormPhysics>().ResetPlayerPhysics();
            GetComponent<WormPhysics>().AddCollidersToSegments();
            SetWormSpawnPosition(new Vector3(0, 2, 0));
            SetWormSpawnOrientation(Quaternion.Euler(0, 90, 0));
            player.wormConstructor.ConstructWorm();
            
            player.wormForwardMovement.SetVariables();
        }
        
        private void RespawnPlayer()
        {
            if (!GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                return;
            }
            
            player.CurrentState = WormState.Idle;
            player.currentPlayerHealth = GameParameters.DefaultPlayerHealth;
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
        
        public void SetWormSpawnPosition(Vector3 spawnPosition)
        {
            Debug.Log("SetWormPosition called");
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
        
        #endregion
    }
}
