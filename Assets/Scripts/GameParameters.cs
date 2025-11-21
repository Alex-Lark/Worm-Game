using UnityEngine;

public static class GameParameters
{
    [Header("Game loop default settings")] 
    public static readonly int defaultNumberOfRounds = 3;
    public static readonly int defaultNumberOfPartsPerRound = 1; //not including discarded card(s)
    public static readonly int defaultTimePerPartSelection = 5;
    public static readonly int defaultTimePerCreatureBuilding = 10;
    public static readonly int defaultTimePerMinigame = 60;

    public static readonly int timeForLeaderboard = 5;
    
    [Header("Creature Builder")] 
    public static readonly float distanceToClampPart = 10f;

    [Header("Worm League UI")]   
    public static float titleFadeTime = 1f;
    public static float titleShowTime = 1.5f;
    public static float teamFadeTime = 1f;
    public static float teamShowTime = 1f;
    public static float scoreFadeTime = 1f;
    public static float scoreShowTime = 1f;
    
    [Header("Worm")]
    public static readonly int WormSegmentCount = 23;
    public static readonly float SegmentMaxPartDistance = 0.075f;
    public static readonly float WormBodyWidth = 0.25f;
    
    [Header("Configurable Joint")] 
    public static readonly float MaxJointAngle = 1f;

    [Header("Worm Movement")]
    public static readonly float MaxWormTurnAngle = 5f;
    public static readonly float WormMoveForce = 300f;
    public static readonly float WormCorrectionForceMultiplier = 1f;
    public static readonly float WormHeadRotationSpeed = 1.5f;
    public static readonly float WormHeadVerticalRotationSpeed = 5f;
    public static readonly float WormMaxVelocity = 4f;
    public static readonly float MaxSlopeAngle = 45f;
    public static readonly float MaxStepHeight = 0.5f;
    public static readonly float StepDetectionDistance = 0.3f;
    public static readonly float WormStepClimbForce = 2000f;
    public static readonly float WormMiddleMaxHeight = 300f;
    public static readonly float WormForwardMovementLoopLength = 0.67f;
    public static readonly float WormScrunchForceMultiplier = 50f;
    public static readonly float WormSegmentScrunchTime = 0.15f;
    public static readonly float WormScrunchForce = 1550f;
    public static readonly float WormGroundPinForce = 1000f;
    public static readonly float maxTimeSinceLastGrounded = 0.02f;

    [Header("Worm Jumping")] 
    public static float WormMiddleSegmentScrunchForce = 2500f;
    public static float WormScrunchMaxHeight = 500f;
    public static readonly float WormJumpForce = 2250f;
    public static readonly float WormJumpMaxChargeTime = 0.25f;
    public static readonly int WormJumpSegments = 2;
    public static readonly float JumpingSegmentDivisionThreshold = 2.0f; // if a segment is this number or greater than another, it gets split in 2
    public static readonly float WormJumpAngle = 0.85f;
    public static readonly float WormMaxScrunchVelocity = 2f;
    public static float WormJumpPreviousPartVsHeadAngle = 0.9f; //1 for all head, 0 for all previouspart

    [Header("Worm Attack")]
    public static readonly float WormHeadbutTime = 0.5f;
    public static readonly float WormHeadButCoolDown = 0.25f;
    
    public static readonly float WormHeadbutGroundingForce = 250f;
    public static readonly float WormHeadButLiftingForce = 500f;
    public static readonly float WormMaxHeightPerSegment = 400f;
    public static readonly float WormHeadButForwardPercent = 0.1f;
    public static readonly float WormHeadButForce = 7000f;
    public static readonly float WormHeadButHeadForce = 20000f;
    public static readonly float WormheadButMaxHeadVerticleAngle = 30f;
    public static readonly float WormHeadRotationSpeedWhileAttacking = 1.0f;

    [Header("Worm Leg")] 
    public static float legMaxVelocityIncrease = 1f;
    public static float legMoveForce = 9000f;
    public static float legMoveTime = 0.75f;
    public static float legJumpForce = 4000f;
    public static float legRotationSpeed = 1f;
    
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

    [Header("Jump Pad")] 
    public static readonly float JumpPadForce = 5000f;
}
