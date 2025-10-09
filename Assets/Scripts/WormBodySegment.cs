using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class WormBodySegment : WormPart
{
    public bool IsScrunched { get; private set; }
    private Coroutine _scrunchCoroutine;

    void Start()
    {
        IsScrunched = true;
    }
    
    public void SetIsScrunched()
    {
        IsScrunched = true;
        if (_scrunchCoroutine != null)
        {
            StopCoroutine(_scrunchCoroutine);
        }

        _scrunchCoroutine = StartCoroutine(ScrunchTimer());
    }
    
    private IEnumerator ScrunchTimer()
    {
        yield return new WaitForSeconds(GameParameters.WormSegmentScrunchTime);
        IsScrunched = false;
        _scrunchCoroutine = null;
    }
    
    public Rigidbody AddJoint(Transform wormPart, Rigidbody previousSegmentRigidBody)
    {
        ConfigurableJoint joint = wormPart.AddComponent<ConfigurableJoint>();
        joint.connectedBody = previousSegmentRigidBody;
        
        // CRITICAL: Set solver iterations for stability
        Rigidbody rb = wormPart.GetComponent<Rigidbody>();
        rb.solverIterations = 20; // Default is 6, increase for stability
        rb.solverVelocityIterations = 10; // Default is 1, increase for stability
        
        // Also increase on the connected body if not already set
        if (previousSegmentRigidBody.solverIterations < 20)
        {
            previousSegmentRigidBody.solverIterations = 20;
            previousSegmentRigidBody.solverVelocityIterations = 10;
        }

        // Lock linear motion to maintain exact distance
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        // Set the anchor point to be maxPartDistance behind the previous segment
        joint.anchor = Vector3.back * GameParameters.SegmentMaxPartDistance; // Local space offset
        joint.connectedAnchor = Vector3.zero; // Previous segment's center

        // Limited angular motion within maxAngle cone
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;

        // Set angular limits to maxAngle
        SoftJointLimit angularLimit = new SoftJointLimit();
        angularLimit.limit = GameParameters.MaxWormTurnAngle; // Cone angle from directly behind
        angularLimit.bounciness = 0f;

        joint.lowAngularXLimit = angularLimit;
        joint.highAngularXLimit = angularLimit;
        joint.angularYLimit = angularLimit;
        joint.angularZLimit = angularLimit;

        // REDUCED spring values to prevent oscillation/jitter
        // The key issue: springs that are too strong cause instability at high framerates
        JointDrive angularDrive = new JointDrive();
        angularDrive.positionSpring = 200f; // Reduced from 500
        angularDrive.positionDamper = 50f; // Reduced from 10000 (this was way too high!)
        angularDrive.maximumForce = 1000f; // Reduced from 2000

        joint.angularXDrive = angularDrive;
        joint.angularYZDrive = angularDrive;
            
        // Add soft limits with spring/damper
        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = 0.1f;
        limit.bounciness = 0f;
        limit.contactDistance = 0.01f;

        joint.linearLimit = limit;

        // REDUCED linear drive values
        JointDrive drive = new JointDrive();
        drive.positionSpring = 500f; // Reduced from 1000
        drive.positionDamper = 100f; // Reduced from 10000
        drive.maximumForce = 5000f; // Changed from Infinity - infinity can cause instability

        joint.xDrive = drive;
        joint.yDrive = drive;
        joint.zDrive = drive;

        // Set target rotation to be aligned with previous segment
        joint.targetRotation = Quaternion.identity; // Try to stay aligned
        
        // IMPORTANT: Enable preprocessing for more stable joints
        joint.enablePreprocessing = true;
        
        // Set projection settings to handle errors
        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.projectionDistance = 0.01f; // Snap back if drifts more than this
        joint.projectionAngle = 2f; // Snap back if rotates more than this

        previousSegmentRigidBody = wormPart.GetComponent<Rigidbody>();
        return previousSegmentRigidBody;
    }
}
