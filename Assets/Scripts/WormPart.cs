using Unity.VisualScripting;
using UnityEngine;

public class WormPart : MonoBehaviour
{
    public bool IsGrounded { get; private set; }
    public GameObject GroundObject { get; private set; }

    private SphereCollider _partCollider;
    
    private readonly Collider[] _results = new Collider[GameParameters.GroundColliderMaxHeldCollisions];
    private readonly float _verticalDetectionOffset = GameParameters.GroundingColliderVerticalDetectionOffset;
    private readonly float _detectionRadiusScale = GameParameters.GroundColliderDetectionRadiusScale;

    private void Awake()
    {
        _partCollider = GetComponent<SphereCollider>();
        GroundObject = null;
    }

    private void FixedUpdate()
    {
        CheckGrounded();
    }

    private void CheckGrounded()
    {
        Vector3 bottom = _partCollider.bounds.center - new Vector3(0, _partCollider.bounds.extents.y, 0);
        Vector3 checkPos = bottom + Vector3.down * _verticalDetectionOffset;
        float radius = _partCollider.bounds.extents.x * _detectionRadiusScale;
        
        int hitCount = Physics.OverlapSphereNonAlloc(checkPos, radius, _results, ~0, QueryTriggerInteraction.Ignore);

        IsGrounded = false;
        GroundObject = null;
        
        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _results[i];
            if (hit.transform.root != transform.root)
            {
                IsGrounded = true;
                GroundObject = hit.gameObject;
                
                break;
            }
        }
    }
}