using UnityEngine;

public static class GameParameters
{
    [Header("Worm")]
    public static readonly int WormSegmentCount = 11;

    [Header("Worm Movement")]
    public static readonly float MaxWormHeadTurnAngle = 10f;
    public static readonly float MaxWormTurnAngle = 10f;
    public static readonly float SegmentMaxPartDistance = 0.125f;
    public static readonly float WormMoveSpeed = 25f;
    public static readonly float WormRotationSpeed = 10f;
    public static readonly float WormJumpForce = 500f;

    [Header("Player Camera")]
    public static readonly float MaxCameraTurnAngle = 90f;
}
