using UnityEngine;

public class WormPartGizmos : MonoBehaviour
{
    public bool showGroundCollider = true;
    public bool showVelocity = true;
    public bool showForces = true;
    
    private readonly float _velocityScale = GameParameters.GizmoVelocityScale;
    private readonly float _forceScale = GameParameters.GizmoForceScale;
    
    private SphereCollider _partCollider;
    private Rigidbody _rb;
    private readonly float _verticalDetectionOffset = GameParameters.GroundingColliderVerticalDetectionOffset;
    
    private Vector3 _lastVelocity;
    private Vector3 _currentNetForce;
    
    void Start()
    {
        _partCollider = GetComponent<SphereCollider>();
        _rb = GetComponent<Rigidbody>();
        
        if (_rb != null)
        {
            _lastVelocity = _rb.linearVelocity;
        }
    }
    
    private void FixedUpdate()
    {
        if (_rb != null)
        {
            // Calculate net force from acceleration (F = ma)
            Vector3 acceleration = (_rb.linearVelocity - _lastVelocity) / Time.fixedDeltaTime;
            _currentNetForce = _rb.mass * acceleration;
            _lastVelocity = _rb.linearVelocity;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (_partCollider == null) _partCollider = GetComponent<SphereCollider>();
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        
        DrawGroundCollider();
        DrawVelocity();
        DrawForces();
    }
    
    private void DrawGroundCollider()
    {
        if (!showGroundCollider || _partCollider == null) return;

        var bounds = _partCollider.bounds;
        
        Vector3 bottom = bounds.center - new Vector3(0, bounds.extents.y, 0);
        Vector3 checkPos = bottom + Vector3.down * _verticalDetectionOffset;
        float radius = bounds.extents.x * 0.9f;
        
        Gizmos.color = GetComponent<WormPart>().IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(checkPos, radius);
    }
    
    private void DrawVelocity()
    {
        if (!showVelocity || _rb == null || !Application.isPlaying) return;
        
        Vector3 velocityVector = _rb.linearVelocity * _velocityScale;
        if (velocityVector.magnitude < 0.01f) return;
        
        Gizmos.color = Color.cyan;
        var position = transform.position;
        Gizmos.DrawRay(position, velocityVector);
        DrawArrowHead(position + velocityVector, velocityVector.normalized, 0.3f, Color.cyan);
    }
    
    private void DrawForces()
    {
        if (!showForces || !Application.isPlaying) return;
        
        Vector3 forceVector = _currentNetForce * _forceScale;
        if (forceVector.magnitude < 0.01f) return;
        
        Gizmos.color = Color.yellow;
        var position = transform.position;
        Gizmos.DrawRay(position, forceVector);
        DrawArrowHead(position + forceVector, forceVector.normalized, 0.3f, Color.yellow);
    }
    
    private void DrawArrowHead(Vector3 tip, Vector3 direction, float size, Color color)
    {
        Gizmos.color = color;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 30, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 30, 0) * Vector3.forward;
        
        Gizmos.DrawRay(tip, right * size);
        Gizmos.DrawRay(tip, left * size);
    }
}