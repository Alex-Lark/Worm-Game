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
            if (cam == null)
            {
                return;
            }
        
            orbitalFollow = cam.GetComponent<CinemachineOrbitalFollow>();
        
        
            cam.Follow = global::Player.Player.Instance.wormVisualHead;
            cam.LookAt = global::Player.Player.Instance.wormVisualHead;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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

            if (global::Player.Player.Instance == null || global::Player.Player.Instance.wormHead == null)
            {
                return;
            }

            float headYaw = global::Player.Player.Instance.wormHead.eulerAngles.y;
            float camYaw  = orbitalFollow.HorizontalAxis.Value;

            float angle = Mathf.DeltaAngle(headYaw, camYaw);
            float clampedAngle = Mathf.Clamp(angle, -maxAngle, maxAngle);

            float final = headYaw + clampedAngle;
            orbitalFollow.HorizontalAxis.Value = final;
        }
    }
}
