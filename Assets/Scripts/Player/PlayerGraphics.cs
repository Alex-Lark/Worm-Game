using System.Collections;
using System.Collections.Generic;
using CreatureBuilder;
using Graphics;
using PurrNet;
using UnityEngine;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerGraphics : NetworkBehaviour
    {
        public TextMeshPro usernameText;
        
        private Camera mainCamera;
        private HighlightOutline playerOutline;
        
        void Start()
        {
            //SceneManager.sceneLoaded += OnSceneLoaded;

            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                EnterGameScene();
            }
        }
        
        void OnEnable()
        {
            GetComponent<Player>().OnPlayerTeamChanged += HandleTeamChanged;
            CinemachineCore.CameraUpdatedEvent.AddListener(OnCinemachineUpdate);
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            GetComponent<Player>().OnWormDeath += OnPlayerDeath;
            GetComponent<PlayerSpawning>().OnWormRespawn += OnWormRespawn;
        }

        void OnDisable()
        {
            GetComponent<Player>().OnPlayerTeamChanged -= HandleTeamChanged;
            CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCinemachineUpdate);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            
            GetComponent<Player>().OnWormDeath -= OnPlayerDeath;
            GetComponent<PlayerSpawning>().OnWormRespawn -= OnWormRespawn;
        }

        void OnDestroy()
        {
            //SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnPlayerDeath()
        {
            usernameText.enabled = false;
        }

        private void OnWormRespawn()
        {
            usernameText.enabled = true;
        }

        private void OnCinemachineUpdate(CinemachineBrain brain)
        {
            UsernameFaceCamera();
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (GameSceneList.IsSceneAGameScene(scene.name))
            {
                EnterGameScene();
            }
            else
            {
                if (playerOutline != null)
                {
                    playerOutline.RemoveHighlight(); 
                }

                if (usernameText != null)
                {
                    usernameText.enabled = false;
                }
            }
        }

        private void EnterGameScene()
        {

            if (mainCamera == null) mainCamera = Camera.main;

            StartCoroutine(SetUserNameTag());
        }

        IEnumerator SetUserNameTag()
        {
            yield return new WaitUntil(() => usernameText != null);
            usernameText.enabled = true;
            usernameText.text = "<mark=#000000aa>" + GetComponent<Player>().PlayerName + "</mark>";
        }

        private void UsernameFaceCamera()
        {
            if (mainCamera != null && usernameText != null)
            {
                usernameText.transform.forward = mainCamera.transform.forward;
            }
        }
        
        #region PlayerOutline

        private void HandleTeamChanged(string team)
        {
            Color teamColor = Color.white;
            if (team == "Red")
            {
                teamColor = Color.red;
            }
            else if (team == "Blue")
            {
                teamColor = Color.blue;
            }

            ServerHandleTeamChanged(gameObject, teamColor);
        }

        [ServerRpc]
        private void ServerHandleTeamChanged(GameObject player, Color teamColor)
        {
            ObserverHandleTeamChanged(player, teamColor);
        }

        [ObserversRpc]
        private void ObserverHandleTeamChanged(GameObject player, Color teamColor)
        {
            if (player != gameObject) return;
            
            GameObject wormMesh = transform.Find("WormMesh").gameObject;
            if (playerOutline == null) playerOutline = wormMesh.AddComponent<HighlightOutline>();
            playerOutline.HighlightPart(teamColor, 0.1f);
        }
        
        #endregion
        
    }
}