using UnityEngine;

public static class GameParameters
{
    [Header("Worm")]
    public static readonly int WormSegmentCount = 11;
    public static readonly float SegmentMaxPartDistance = 0.125f;

    [Header("Worm Movement")]
    public static readonly float MaxWormTurnAngle = 10f;
    public static readonly float WormMoveForce = 25f;
    public static readonly float WormHeadRotationSpeed = 2f;
    public static readonly float WormJumpForce = 500f;

    [Header("Player Camera")]
    public static readonly float MaxCameraTurnAngle = 90f;
}
