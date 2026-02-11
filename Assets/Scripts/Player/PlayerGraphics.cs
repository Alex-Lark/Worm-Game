using UnityEngine;
using TMPro;
using Unity.Cinemachine;

namespace Player
{
    public class PlayerGraphics : MonoBehaviour
    {
        public TextMeshPro usernameText;
        private Camera mainCamera;
        
        void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
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

        private void UsernameFaceCamera()
        {
            if (mainCamera != null && usernameText != null)
            {
                usernameText.transform.forward = mainCamera.transform.forward;
            }
        }
    }
}