using UnityEngine;

public static class GameParameters
{
    [Header("Worm")]
    public static readonly int WormSegmentCount = 23;
    public static readonly float SegmentMaxPartDistance = 0.075f;
    public static readonly float WormBodyWidth = 0.25f;

    [Header("Worm Movement")]
    public static readonly float MaxWormTurnAngle = 5f;
    public static readonly float WormMoveForce = 100f;
    public static readonly float WormHeadRotationSpeed = 1.5f;
    public static readonly float WormJumpForce = 2000f;
    public static readonly float WormMaxVelocity = 4f;
    public static float MaxSlopeAngle = 45f;
    public static float MaxStepHeight = 0.5f;
    public static float StepDetectionDistance = 0.3f;
    public static float WormStepClimbForce = 1000f;

    [Header("Worm Visual Head Movement")] 
    public static readonly float VisualHeadVerticalOffset = 0.75f;

    [Header("Worm Physics")] 
    public static readonly int NumSegmentCollisionsIgnored = 5;
    
    [Header("Worm Part Ground Detection")]
    public static readonly float GroundingColliderVerticalDetectionOffset = 0.05f;
    public static readonly float GroundColliderDetectionRadiusScale = 0.5f;
    public static readonly int GroundColliderMaxHeldCollisions = 8;
    
    [Header("Worm Part Gizmos")]
    public static readonly float GizmoVelocityScale = 1f;
    public static readonly float GizmoForceScale = 0.1f;

    [Header("Player Camera")]
    public static readonly float MaxCameraTurnAngle = 90f;
    
    
}
