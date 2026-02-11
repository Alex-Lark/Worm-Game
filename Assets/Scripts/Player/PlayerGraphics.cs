using UnityEngine;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerGraphics : MonoBehaviour
    {
        public TextMeshPro usernameText;
        private Camera mainCamera;
        
        void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            EnterGameScene();
        }
        
        void OnEnable()
        {
            CinemachineCore.CameraUpdatedEvent.AddListener(OnCinemachineUpdate);
        }

        void OnDisable()
        {
            CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCinemachineUpdate);
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
                Debug.Log("entering not game scene in playerGraphics");
                OnDisable();
                usernameText.enabled = false;
            }
        }

        private void EnterGameScene()
        {
            Debug.Log("entering game scene in playerGraphics");
            OnEnable();
            usernameText.enabled = true;
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            usernameText.text = "<mark=#000000aa>" + gameObject.GetComponent<Player>().PlayerName + "</mark>";
        }

        private void UsernameFaceCamera()
        {
            if (mainCamera != null && usernameText != null)
            {
                usernameText.transform.forward = mainCamera.transform.forward;
            }
        }
    }
}