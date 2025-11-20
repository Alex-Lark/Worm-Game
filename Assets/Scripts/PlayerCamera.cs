using System;
using UnityEngine;
using Unity.Cinemachine;

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
        
        
        cam.Follow = Player.Instance.wormVisualHead;
        cam.LookAt = Player.Instance.wormVisualHead;

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

        if (Player.Instance == null || Player.Instance.wormHead == null)
        {
            return;
        }

        float headYaw = Player.Instance.wormHead.eulerAngles.y;
        float camYaw  = orbitalFollow.HorizontalAxis.Value;

        float angle = Mathf.DeltaAngle(headYaw, camYaw);
        float clampedAngle = Mathf.Clamp(angle, -maxAngle, maxAngle);

        float final = headYaw + clampedAngle;
        orbitalFollow.HorizontalAxis.Value = final;
    }
}
