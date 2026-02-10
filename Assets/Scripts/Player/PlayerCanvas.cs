using UnityEngine;

namespace Player
{
    public class PlayerCanvas : MonoBehaviour
    {
        private Camera mainCamera;

        public void SetCamera(Camera playerCamera)
        {
            mainCamera = gameObject.GetComponentInParent<Player>().thirdPersonCamera.GetComponent<Camera>();
        }
        
        void LateUpdate()
        {
            if (mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up);
            }
        }
    }
}