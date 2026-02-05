using Unity.VisualScripting;
using UnityEngine;

public class WormPart : MonoBehaviour
{
    public bool IsGrounded { get; private set; }
    public GameObject GroundObject { get; private set; }
    public Vector3 GroundNormal { get; private set; }
    
    public float TimeSinceLastGrounded { get; private set; }

    private Collider _partCollider;
    
    private readonly Collider[] _results = new Collider[GameParameters.GroundColliderMaxHeldCollisions];
    private readonly float _verticalDetectionOffset = GameParameters.GroundingColliderVerticalDetectionOffset;
    private readonly float _detectionRadiusScale = GameParameters.GroundColliderDetectionRadiusScale;

    private void Awake()
    {
        _partCollider = GetComponent<Collider>();
        GroundObject = null;
    }

    protected virtual void FixedUpdate()
    {
        CheckGrounded();
        
    }

    public virtual void Jump()
    {
        
    }

    public virtual void MoveForward()
    {
        
    }

    public void ConfigureRigidBody(Rigidbody partRigidbody, Rigidbody segmentRigidbody)
    {
        //TODO: properly set part's rigidBody rather than just copying
        partRigidbody.mass = segmentRigidbody.mass;
        partRigidbody.linearDamping = segmentRigidbody.linearDamping;
        partRigidbody.angularDamping = segmentRigidbody.angularDamping;
        partRigidbody.interpolation = segmentRigidbody.interpolation;
        partRigidbody.collisionDetectionMode = segmentRigidbody.collisionDetectionMode;
            
        partRigidbody.linearDamping = 1f;
        partRigidbody.angularDamping = 1f;
    }

    public void ConfigureHingeJoint(Rigidbody segmentRigidbody, Transform endPoint)
    {
        //TODO: properly configure hinge joint
        HingeJoint hinge = gameObject.AddComponent<HingeJoint>();
        hinge.connectedBody = segmentRigidbody;
        
        hinge.anchor = gameObject.transform.InverseTransformPoint(endPoint.position);

        //limited rotation
        JointLimits limits = hinge.limits;
        limits.min = -10f;   // degrees
        limits.max = 10f;    // degrees
        hinge.limits = limits;
        hinge.useLimits = true;

        // smoothing
        hinge.enablePreprocessing = true;
        hinge.enableCollision = false;
    }

    private void CheckGrounded()
    {
        Vector3 bottom = _partCollider.bounds.center - new Vector3(0, _partCollider.bounds.extents.y, 0);
        Vector3 checkPos = bottom + Vector3.down * _verticalDetectionOffset;
        float radius = _partCollider.bounds.extents.x * _detectionRadiusScale;
        
        int hitCount = Physics.OverlapSphereNonAlloc(checkPos, radius, _results, ~0, QueryTriggerInteraction.Ignore);

        IsGrounded = false;
        GroundObject = null;
        GroundNormal = Vector3.up;
        
        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _results[i];
            if (hit.transform.root != transform.root)
            {
                IsGrounded = true;
                GroundObject = hit.gameObject;
                GroundNormal = GetGroundNormal(hit);
                
                break;
            }
        }
    }
    
    private Vector3 GetGroundNormal(Collider groundCollider)
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            if (hit.collider == groundCollider)
            {
                TimeSinceLastGrounded = 0f;
                return hit.normal;
            }
        }

        TimeSinceLastGrounded += Time.fixedDeltaTime;
        return Vector3.up;
    }
}