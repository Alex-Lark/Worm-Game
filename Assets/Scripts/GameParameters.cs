using UnityEngine;

public static class GameParameters
{
    [Header("Worm")]
    public static readonly int WormSegmentCount = 23;
    public static readonly float SegmentMaxPartDistance = 0.075f;
    public static readonly float WormBodyWidth = 0.25f;

    [Header("Worm Movement")]
    public static readonly float MaxWormTurnAngle = 5f;
    public static readonly float WormMoveForce = 300f;
    public static readonly float WormHeadRotationSpeed = 1.5f;
    public static readonly float WormHeadVerticalRotationSpeed = 5f;
    public static readonly float WormMaxVelocity = 4f;
    public static readonly float MaxSlopeAngle = 45f;
    public static readonly float MaxStepHeight = 0.5f;
    public static readonly float StepDetectionDistance = 0.3f;
    public static readonly float WormStepClimbForce = 1000f;
    public static readonly float WormMiddleMaxHeight = 300f;
    public static readonly float WormForwardMovementLoopLength = 0.75f;
    public static readonly float WormScrunchForceMultiplier = 50f;
    public static readonly float WormSegmentScrunchTime = 0.15f;
    public static readonly float WormScrunchForce = 1750f;
    public static readonly float WormGroundPinForce = 1000f;

    [Header("Worm Jumping")] 
    public static float WormMiddleSegmentScrunchForce = 10000f;
    public static float WormScrunchMaxHeight = 500f;
    public static readonly float WormJumpForce = 2500f;
    public static readonly float WormJumpMaxChargeTime = 0.25f;
    public static readonly int WormJumpSegments = 2;
    public static readonly float JumpingSegmentDivisionThreshold = 2.0f; // if a segment is this number or greater than another, it gets split in 2
    public static readonly float WormJumpAngle = 0.75f;
    public static readonly float WormMaxScrunchVelocity = 2f;
    
    

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
    public static readonly float minCameraPitch = -10f;
    public static readonly float maxCameraPitch = 40f;
    public static readonly float MaxCameraTurnAngle = 90f;
    
    
}
