using CreatureParts;
using UnityEngine;

public class WormPartGizmos : MonoBehaviour
{
    /*      GIZMOS FOR IN-EDITOR VISUALIZATIONS, DOES NOT AFFECT GAMEPLAY       */
    
    public bool showGroundCollider = true;
    public bool showVelocity = true;
    public bool showForces = true;
    
    private readonly float velocityScale = GameParameters.GizmoVelocityScale;
    private readonly float forceScale = GameParameters.GizmoForceScale;
    
    private SphereCollider partCollider;
    private Rigidbody rb;
    private readonly float verticalDetectionOffset = GameParameters.GroundingColliderVerticalDetectionOffset;
    
    private Vector3 lastVelocity;
    private Vector3 currentNetForce;
    
    void Start()
    {
        partCollider = GetComponent<SphereCollider>();
        rb = GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            lastVelocity = rb.linearVelocity;
        }
    }
    
    private void FixedUpdate()
    {
        if (rb != null)
        {
            Vector3 acceleration = (rb.linearVelocity - lastVelocity) / Time.fixedDeltaTime;
            currentNetForce = rb.mass * acceleration;
            lastVelocity = rb.linearVelocity;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (partCollider == null) partCollider = GetComponent<SphereCollider>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        
        DrawGroundCollider();
        DrawVelocity();
        DrawForces();
    }
    
    private void DrawGroundCollider()
    {
        if (!showGroundCollider || partCollider == null) return;

        var bounds = partCollider.bounds;
        
        Vector3 bottom = bounds.center - new Vector3(0, bounds.extents.y, 0);
        Vector3 checkPos = bottom + Vector3.down * verticalDetectionOffset;
        float radius = bounds.extents.x * 0.9f;
        
        Gizmos.color = GetComponent<CreaturePart>().IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(checkPos, radius);
    }
    
    private void DrawVelocity()
    {
        if (!showVelocity || rb == null || !Application.isPlaying) return;
        
        Vector3 velocityVector = rb.linearVelocity * velocityScale;
        if (velocityVector.magnitude < 0.01f) return;
        
        Gizmos.color = Color.cyan;
        var position = transform.position;
        Gizmos.DrawRay(position, velocityVector);
        DrawArrowHead(position + velocityVector, velocityVector.normalized, 0.3f, Color.cyan);
    }
    
    private void DrawForces()
    {
        if (!showForces || !Application.isPlaying) return;
        
        Vector3 forceVector = currentNetForce * forceScale;
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