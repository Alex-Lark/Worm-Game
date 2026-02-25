using System.Collections;
using System.Collections.Generic;
using CreatureParts;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerSpawning : MonoBehaviour
    {
        public Player player;

        public bool canRespawn = true;
        
        void Start()
        {
            player = GetComponent<Player>();
            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                SpawnInGameScene();
            }
        }

        public void SpawnInCreatureBuildingScene()
        {
            WormPhysics wormPhysics = GetComponent<WormPhysics>();
            wormPhysics.ResetWormPhysics();
            SetWormSpawnOrientation(Quaternion.identity);
            wormPhysics.PositionWormSegments(new Vector3(0, 2, 0));
            player.DeactivatePlayer();
        }
        
        public void SpawnInGameScene()
        {
            player.wormHead.GetComponent<Rigidbody>().isKinematic = false;
            
            foreach (Transform segment in player.wormBodySegments)
            {
                segment.GetComponent<Rigidbody>().isKinematic = false;
            }
            StartCoroutine(SetupAfterSceneLoad());
            player.ActivatePlayer();
            player.currentPlayerHealth = GameParameters.DefaultPlayerHealth;
            player.CurrentState = WormState.Idle;
        }

        public void TryToRespawn()
        {
            if (canRespawn)
            {
                StartCoroutine(RespawnTimer());
            }
            else
            {
                //player is permanently dead
            }
        }
        
        public IEnumerator RespawnTimer()
        {
            float timeLeft = GameParameters.PlayerRespawnTimeInSeconds;
    
            while (timeLeft > 0)
            {
                DeathScreenUI.Instance.respawnText.text = "Respawning in " + Mathf.Ceil(timeLeft);
                yield return new WaitForSeconds(1f);
                timeLeft -= 1f;
            }
    
            DeathScreenUI.Instance.respawnText.text = "Respawning...";
            RespawnPlayer();
        }
        
        private void RespawnPlayer()
        {
            player.CurrentState = WormState.Idle;
            player.currentPlayerHealth = GameParameters.DefaultPlayerHealth;
            player.thirdPersonCamera.GetComponent<CinemachineBrain>().enabled = true;
            DeathScreenUI.Instance.DisableDeathUI();

            player.wormHead.gameObject.SetActive(true);
            
            ClearDuplicateParts();
            
            foreach (Transform bodySegment in player.wormBodySegments)
            {
                bodySegment.gameObject.SetActive(true);
            }
            
            GetComponent<WormRenderer>().enabled = true;
            GetComponent<WormRenderer>().Restart();
            
            ResetPlayerPhysics();
            StartCoroutine(ReactivateAttachedParts());
        }
        
        private IEnumerator SetupAfterSceneLoad()
        {
            yield return null;

            player.thirdPersonCamera = Camera.main?.gameObject;
            
            ResetPlayerPhysics();
            
            player.wormForwardMovement.SetVariables();
        }

        private void ResetPlayerPhysics()
        {
            player.wormHead.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            player.wormHead.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            foreach (Transform segment in player.wormBodySegments)
            {
                Rigidbody rb = segment.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            foreach (GameObject part in player.attachedWormParts)
            {
                Rigidbody rb = part.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            SetWormSpawnOrientation(Quaternion.Euler(0, 90, 0));
            GetComponent<WormPhysics>().ResetWormPosition();
            player.wormConstructor.ConstructWorm();
            GetComponent<WormPhysics>().AddCollidersToSegments();
        }

        private void ClearDuplicateParts()
        {
            Destroy(player.wormHeadCopy);
            foreach (Transform bodySegmentCopy in player.wormBodySegmentsCopy)
            {
                Destroy(bodySegmentCopy.gameObject);
            }
            player.wormBodySegmentsCopy.Clear();
            foreach (GameObject partCopy in player.attachedWormPartsCopy)
            {
                Destroy(partCopy.gameObject);
            }
            player.attachedWormPartsCopy.Clear();
        }
        
        public void SetWormSpawnOrientation(Quaternion orientation)
        {
            player.wormVisualHead.rotation = orientation;
            player.wormHead.rotation = orientation;
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
        
    }
}
