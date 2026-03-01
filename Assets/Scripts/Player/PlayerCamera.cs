using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
    public class PlayerCamera : MonoBehaviour
    {
        private CinemachineOrbitalFollow orbitalFollow;

        public float maxAngle = GameParameters.MaxCameraTurnAngle;

        void Awake()
        {
            var cam = gameObject.GetComponent<CinemachineCamera>();
            if (cam == null) return;

            orbitalFollow = cam.GetComponent<CinemachineOrbitalFollow>();

            if (LocalPlayer.Instance != null)
                SetupCamera(cam);
            else
                LocalPlayer.OnLocalPlayerReady += () => SetupCamera(cam);
        }

        private void OnDestroy()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void OnEnable()
        {
            CinemachineCore.CameraUpdatedEvent.AddListener(OnCinemachineLateUpdate);
        }

        void OnDisable()
        {
            CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCinemachineLateUpdate);
        }

        void OnCinemachineLateUpdate(CinemachineBrain brain)
        {

            if (orbitalFollow == null)
            {
                return;
            }

            if (LocalPlayer.Instance == null || LocalPlayer.Instance.wormHead == null)
            {
                return;
            }

            float headYaw = LocalPlayer.Instance.wormHead.eulerAngles.y;
            float camYaw  = orbitalFollow.HorizontalAxis.Value;

            float angle = Mathf.DeltaAngle(headYaw, camYaw);
            float clampedAngle = Mathf.Clamp(angle, -maxAngle, maxAngle);

            float final = headYaw + clampedAngle;
            orbitalFollow.HorizontalAxis.Value = final;
        }
        
        private void SetupCamera(CinemachineCamera cam)
        {
            cam.Follow = LocalPlayer.Instance.wormVisualHead;
            cam.LookAt = LocalPlayer.Instance.wormVisualHead;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
    }
}
