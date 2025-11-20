using System;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    private CinemachineOrbitalFollow orbitalFollow;

    public float maxAngle = GameParameters.MaxCameraTurnAngle;

    void Awake()
    {
        print("camera awake");
        gameObject.GetComponent<CinemachineCamera>().Follow = Player.Instance.GetComponent<Player>().wormVisualHead;
        gameObject.GetComponent<CinemachineCamera>().LookAt = Player.Instance.GetComponent<Player>().wormVisualHead;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        // if (orbitalFollow == null || Player.Instance == null || Player.Instance.wormHead == null) return;
        //
        // float headYaw = Player.Instance.wormHead.eulerAngles.y;
        // float camYaw = orbitalFollow.HorizontalAxis.Value;
        //
        // float angle = Mathf.DeltaAngle(headYaw, camYaw);
        //
        // float clampedAngle = Mathf.Clamp(angle, -maxAngle, maxAngle);
        //
        // orbitalFollow.HorizontalAxis.Value = headYaw + clampedAngle;
    }
}