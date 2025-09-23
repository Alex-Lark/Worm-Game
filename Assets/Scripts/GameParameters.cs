using UnityEngine;

public static class GameParameters
{
    [Header("Worm")]
    public static readonly int WormSegmentCount = 10;
    
    [Header("Worm Movement")]
    public static readonly float MaxWormTurnAngle = 45f;
    public static readonly float SegmentMaxPartDistance = 0.5f;
    public static readonly float WormMoveSpeed = 5f;
    public static readonly float WormRotationSpeed = 10f;
}
