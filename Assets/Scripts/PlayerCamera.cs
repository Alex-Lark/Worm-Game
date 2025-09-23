using UnityEngine;
using Unity.Cinemachine;

public class FreeLookClampCine3 : MonoBehaviour
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
        if (orbitalFollow == null || Player.Instance == null) return;

        float playerYaw = Player.Instance.transform.eulerAngles.y;
        float camYaw = orbitalFollow.HorizontalAxis.Value;

        // Compute signed difference between camera and player yaw
        float angle = Mathf.DeltaAngle(playerYaw, camYaw);

        // Clamp relative to player's forward
        float clampedAngle = Mathf.Clamp(angle, -maxAngle, maxAngle);

        // Apply correction
        orbitalFollow.HorizontalAxis.Value = playerYaw + clampedAngle;
    }
}