using System.Collections.Generic;
using CreatureParts;
using UnityEngine;

public class WormHeadBut : MonoBehaviour
{
    private List<Transform> wormParts;
    private Rigidbody wormHead;

    private void Start()
    {
        wormParts = Player.Player.Instance.wormBodySegments;
        wormHead = Player.Player.Instance.wormHead.GetComponent<Rigidbody>();
    }
    

    public void ReadyHeadbut()
    {
        int segmentCount = GameParameters.WormSegmentCount + 1; // head
        int liftedSegment = 0;

        if (!Player.Player.Instance.IsWormGrounded)
        {
            return;
        }
        
        //TODO: fix lifting logic
        LiftFrontSegments(wormHead, segmentCount, segmentCount);
        
        for (int i = 0; i < wormParts.Count; i++)
        {
            Transform wormPart = wormParts[i];
            Rigidbody wormPartRigidBody = wormPart.GetComponent<Rigidbody>();
            if (i > segmentCount / 2)
            {
                GroundBackSegment(wormPartRigidBody);
            }
            else
            {
                liftedSegment++;
                LiftFrontSegments(wormPartRigidBody, liftedSegment, segmentCount);
            }
        }

        MoveHead();
    }

    public void EndHeadBut()
    {
        SnapHeadRotation();
        wormHead.AddForce(wormHead.transform.forward * GameParameters.WormHeadButHeadForce);
        for (int i = 0; i < wormParts.Count; i++)
        {
            Rigidbody wormPartRigidBody = wormParts[i].GetComponent<Rigidbody>();
            if (i < ((GameParameters.WormSegmentCount + 1) / 2))
            {
                wormPartRigidBody.AddForce(wormHead.transform.forward * (GameParameters.WormHeadButForce/(i + 1)));
            }
            else
            {
                GroundBackSegment(wormPartRigidBody);
            }
        }
    }

    public void WormheadbutCoolDown()
    {
        for (int i = 0; i < wormParts.Count; i++)
        {
            if (i > ((GameParameters.WormSegmentCount + 1) / 2))
            {
                Rigidbody wormPartRigidBody = wormParts[i].GetComponent<Rigidbody>();
                GroundBackSegment(wormPartRigidBody);
            }
        }
        
    }

    private void LiftFrontSegments(Rigidbody wormPart, int liftedSegment, int segmentCount)
    {
        float maxSegmentHeight = GameParameters.WormMaxHeightPerSegment / liftedSegment;

        if (wormPart.position.y < maxSegmentHeight)
        {
            float forceMultiplier = liftedSegment / (segmentCount/2);
            wormPart.AddForce((Vector3.up + (wormPart.transform.forward * GameParameters.WormHeadButForwardPercent)) * (GameParameters.WormHeadButLiftingForce * forceMultiplier));
        }
        
    }

    private void GroundBackSegment(Rigidbody wormPart) {
        CreatureBodySegment segment = wormPart.GetComponent<CreatureBodySegment>();
        if (segment.IsGrounded)
        {
            Vector3 groundNormal = segment.GroundNormal;
            Vector3 velocity = wormPart.linearVelocity;
        
            // Remove velocity component moving away from ground
            float normalVelocity = Vector3.Dot(velocity, groundNormal);
            if (normalVelocity > 0)
            {
                wormPart.linearVelocity = velocity - groundNormal * normalVelocity;
            }
        
            // Optional: gentle downward force
            wormPart.AddForce(-groundNormal * GameParameters.WormHeadbutGroundingForce);
        }
    }
    
    private void MoveHead()
    {
        RotateHeadUngrounded(GameParameters.WormHeadRotationSpeedWhileAttacking);
        wormHead.AddForce(GameParameters.WormMoveForce * wormHead.transform.forward);
    }

    private void RotateHeadUngrounded(float speed) {
        Vector3 camDirFlat = Flatten(Player.Player.Instance.thirdPersonCamera.transform.forward);
        Quaternion targetYaw = Quaternion.LookRotation(camDirFlat);
    
        wormHead.rotation = Quaternion.Slerp(
            wormHead.rotation,
            targetYaw,
            speed * Time.fixedDeltaTime
        );
    }
    
    private static Vector3 Flatten(Vector3 v) {
        v.y = 0f;
        return v.normalized;
    }
    
    private void SnapHeadRotation() {
        Vector3 camDir = Player.Player.Instance.thirdPersonCamera.transform.forward.normalized;
        float pitch = CalculatePitch(camDir);
    
        // Clamp pitch to the allowed range
        pitch = Mathf.Clamp(pitch, -GameParameters.WormheadButMaxHeadVerticleAngle, GameParameters.WormheadButMaxHeadVerticleAngle);
    
        // Keep current yaw, only apply new pitch
        Vector3 currentForward = wormHead.transform.forward;
        Vector3 currentForwardFlat = Flatten(currentForward);
        Quaternion currentYaw = Quaternion.LookRotation(currentForwardFlat);
    
        Quaternion pitchRot = Quaternion.AngleAxis(-pitch, currentYaw * Vector3.right);
        wormHead.rotation = pitchRot * currentYaw;
    }

    private float CalculatePitch(Vector3 camDir) {
        float camPitch = Vector3.SignedAngle(
            Vector3.ProjectOnPlane(camDir, Vector3.up),
            camDir,
            Player.Player.Instance.thirdPersonCamera.transform.right
        );
    
        float normalized = Mathf.InverseLerp(GameParameters.MinCameraPitch, GameParameters.MaxCameraPitch, camPitch);
        normalized = 1f - Mathf.Clamp01(normalized);
    
        return Mathf.Lerp(-85f, 85f, normalized);
    }
}
