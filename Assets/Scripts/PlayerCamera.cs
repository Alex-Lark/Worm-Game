using UnityEngine;
using Unity.Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    public CinemachineCamera cineCam;
    private CinemachineOrbitalFollow orbitalFollow;

    public float maxAngle = GameParameters.MaxWormTurnAngle;

    void Awake()
    {
        if (cineCam != null)
            orbitalFollow = cineCam.GetComponent<CinemachineOrbitalFollow>();
    }

    void LateUpdate()
    {
        if (orbitalFollow == null || Player.Instance == null || Player.Instance.wormHead == null) return;

        // Use the head's yaw instead of the parent
        float headYaw = Player.Instance.wormHead.eulerAngles.y;
        float camYaw = orbitalFollow.HorizontalAxis.Value;

        // Compute signed difference between camera and head yaw
        float angle = Mathf.DeltaAngle(headYaw, camYaw);

        // Clamp relative to head's forward
        float clampedAngle = Mathf.Clamp(angle, -maxAngle, maxAngle);

        // Apply correction
        orbitalFollow.HorizontalAxis.Value = headYaw + clampedAngle;
    }
}