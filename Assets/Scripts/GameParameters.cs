using UnityEngine;

public static class GameParameters
{
    #region Game Loop Default Settings
    [Header("Game loop default settings")] 
    
    public static readonly int DefaultNumberOfRounds = 3;
    public static readonly int DefaultNumberOfPartsPerRound = 1; //not including discarded card(s)
    public static readonly int DefaultTimePerPartSelection = 5;
    public static readonly int DefaultTimePerCreatureBuilding = 30;
    public static readonly int DefaultTimePerMinigame = 60;
    
    public static readonly int TimeForLeaderboard = 5;
    
    #endregion
    
    #region Creature Builder UI
    [Header("Creature Builder UI")] 
    
    public static readonly float CardTransparencyWhileDragging = 0.6f;
    public static readonly Color PartDraggingOutlineColor = Color.cyan;
    public static readonly float PartDraggingOutlineWidth = 0.03f;
    
    #endregion
    
    #region Creature Builder
    [Header("Creature Builder")] 
    
    public static readonly float DistanceToClampPart = 30f;
    
    #endregion

    #region Worm League UI
    [Header("Worm League UI")]   
    
    public static readonly float TitleFadeTime = 1f;
    public static readonly float TitleShowTime = 1.5f;
    public static readonly float TeamFadeTime = 1f;
    public static readonly float TeamShowTime = 1f;
    public static readonly float ScoreFadeTime = 1f;
    public static readonly float ScoreShowTime = 1f;
    
    #endregion
    
    #region Worm
    [Header("Worm")]
    
    public static readonly int WormSegmentCount = 23;
    public static readonly float SegmentMaxPartDistance = 0.075f;
    public static readonly float WormBodyWidth = 0.25f;
    
    #endregion
    
    #region Part Configurable Joint
    [Header("Part Configurable Joint")] 
    
    public static readonly float MaxJointAngle = 1f;
    
    #endregion
    
    #region Worm Movement
    [Header("Worm Movement")]
    
    public static readonly float MaxWormTurnAngle = 500f;
    public static readonly float WormMoveForce = 300f;
    public static readonly float WormCorrectionForceMultiplier = 1f;
    public static readonly float WormHeadRotationSpeed = 1.5f;
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
    public static readonly float MaxTimeSinceLastGrounded = 0.02f;
    
    #endregion

    #region Worm Jumping
    [Header("Worm Jumping")] 
    
    public static readonly float WormMiddleSegmentScrunchForce = 2500f;
    public static readonly float WormJumpForce = 2250f;
    public static readonly int WormJumpSegments = 2;
    public static readonly float JumpingSegmentDivisionThreshold = 2.0f; // if a segment is this number or greater than another, it gets split in 2
    public static readonly float WormJumpAngle = 0.85f;
    public static readonly float WormJumpPreviousPartVsHeadAngle = 0.9f; //1 for all head, 0 for all previouspart
    
    #endregion

    #region Worm Attack
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
    
    #endregion

    #region Worm Leg
    [Header("Worm Leg")] 
    
    public static float LegMaxVelocityIncrease = 1f;
    public static float LegMoveForce = 9000f;
    public static float LegMoveTime = 0.75f;
    public static float LegJumpForce = 4000f;
    
    #endregion
    
    #region Worm Visual Head
    [Header("Worm Visual Head")] 
    
    public static readonly float VisualHeadVerticalOffset = 0.75f;
    public static readonly float VisualHeadMaxDegrees = 85f;
    
    #endregion

    #region Worm Physics
    [Header("Worm Physics")] 
    
    public static readonly int NumSegmentCollisionsIgnored = 5;
    
    #endregion
    
    #region Worm Part Ground Detection
    [Header("Worm Part Ground Detection")]
    
    public static readonly float GroundingColliderVerticalDetectionOffset = 0.05f;
    public static readonly float GroundColliderDetectionRadiusScale = 0.5f;
    public static readonly int GroundColliderMaxHeldCollisions = 8;
    
    #endregion
    
    #region Worm Part Gizmos
    [Header("Worm Part Gizmos")]
    
    public static readonly float GizmoVelocityScale = 1f;
    public static readonly float GizmoForceScale = 0.1f;
    
    #endregion

    #region Player Camera
    [Header("Player Camera")]
    
    public static readonly float MinCameraPitch = -10f;
    public static readonly float MaxCameraPitch = 40f;
    public static readonly float MaxCameraTurnAngle = 90f;
    
    #endregion

    #region Jump Pad
    [Header("Jump Pad")] 
    
    public static readonly float JumpPadForce = 5000f;
    
    #endregion
    
    #region HealthSystem
    
    public static readonly float DefaultPlayerHealth = 100f;
    public static readonly float PlayerHealthRegen = 0.05f;

    public static readonly float MinSpikeCollisionForceToDamage = 50f;
    public static readonly float SpikeForceToDamageMultiplier = 0.04f;
    
    public static readonly float MinProjectileCollisionForceToDamage = 50f;
    public static readonly float ProjectileForceToDamageMultiplier = 0.04f;

    public static readonly float MinBluntCollisionForceToDamage = 200f;
    public static readonly float BluntForceToDamageMultiplier = 0.02f;

    public static readonly float HeadbutDamageReductionOnHead = 0.05f; //head takes less damage when actively headbutting
    public static readonly float HeadDamageMultiplier = 1.25f; //head takes more damage normally
    
    public static readonly float ShellDamageReduction = 0.05f;

    public static readonly float PlayerRespawnTimeInSeconds = 3f;

    #endregion
    
    #region Worm Death Effects

    public static readonly float DeadPartVelocityMultiplier = 1f;
    public static readonly float DeadPartMass = 1f;
    public static readonly float DeadPartLinearDamping = 1f;
    public static readonly float DeadPartVelocityReduction = 0.8f; //higher = less velocity
    public static readonly float DeadPartDeleteTime = 3f;

    #endregion
}
