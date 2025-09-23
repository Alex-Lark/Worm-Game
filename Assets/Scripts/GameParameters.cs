using UnityEngine;

public static class GameParameters
{
    [Header("Worm")]
    public static readonly int WormSegmentCount = 10;
    
    [Header("Worm Movement")]
    public static readonly float MaxWormTurnAngle = 30f;
    public static readonly float SegmentMaxPartDistance = 0.3f;
    public static readonly float WormMoveSpeed = 5f;
    public static readonly float WormRotationSpeed = 10f;

    [Header("Player Camera")]
    public static readonly float MaxCameraTurnAngle = 90f;
}
