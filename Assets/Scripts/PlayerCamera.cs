using UnityEngine;
using Unity.Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    //public CinemachineCamera cineCam;
    private CinemachineOrbitalFollow orbitalFollow;

    public float maxAngle = GameParameters.MaxCameraTurnAngle;

    void Awake()
    {
        gameObject.GetComponent<CinemachineCamera>().Follow = Player.Instance.GetComponent<Player>().wormVisualHead;
        gameObject.GetComponent<CinemachineCamera>().LookAt = Player.Instance.GetComponent<Player>().wormVisualHead;
        
        //if (cineCam != null)
        //orbitalFollow = cineCam.GetComponent<CinemachineOrbitalFollow>();
    }

    void LateUpdate()
    {
        //if (orbitalFollow == null || Player.Instance == null || Player.Instance.wormHead == null) return;
        
        //float headYaw = Player.Instance.wormHead.eulerAngles.y;
        //float camYaw = orbitalFollow.HorizontalAxis.Value;
        
        //float angle = Mathf.DeltaAngle(headYaw, camYaw);
        
        //float clampedAngle = Mathf.Clamp(angle, -maxAngle, maxAngle);
        
        //orbitalFollow.HorizontalAxis.Value = headYaw + clampedAngle;
    }
}