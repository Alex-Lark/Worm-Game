using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("FOV Settings")]
        public float baseFov = 60f;
        public float maxFov = 90f;
        public float maxSpeed = 15f;
        public float fovSmoothTime = 0.3f;
        
        private float fovVelocity;
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

            //TODO: update using new methods in multiplayer
            if (Player.Instance.IsWormMovingForward)
            {
                float playerSpeed = Player.Instance.wormHead.GetComponent<Rigidbody>().linearVelocity.magnitude;
                
                float targetFov = Mathf.Lerp(baseFov, maxFov, playerSpeed / maxSpeed);
                
                float currentFov = GetComponent<CinemachineCamera>().Lens.FieldOfView;
                GetComponent<CinemachineCamera>().Lens.FieldOfView = Mathf.SmoothDamp(currentFov, targetFov, ref fovVelocity, fovSmoothTime);
            }
            else
            {
                float currentFov = GetComponent<CinemachineCamera>().Lens.FieldOfView;
                GetComponent<CinemachineCamera>().Lens.FieldOfView = Mathf.SmoothDamp(currentFov, baseFov, ref fovVelocity, fovSmoothTime);
            }
        }
        
        private void SetupCamera(CinemachineCamera cam)
        {
            Debug.Log("Local player is ready");
            if (LocalPlayer.Instance != null)
            {
                cam.Follow = LocalPlayer.Instance.wormVisualHead;
                cam.LookAt = LocalPlayer.Instance.wormVisualHead;

            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
