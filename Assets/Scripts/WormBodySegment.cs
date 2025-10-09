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
    
        // Strong damping to prevent free rotation
        JointDrive angularDrive = new JointDrive();
        angularDrive.positionSpring = 500f; // Spring force to keep segments aligned
        angularDrive.positionDamper = 10000f; // Damping to stop oscillation
        angularDrive.maximumForce = 2000f;
    
        joint.angularXDrive = angularDrive;
        joint.angularYZDrive = angularDrive;
        
        // Add soft limits with spring/damper
        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = 0.1f;
        limit.bounciness = 0f;
        limit.contactDistance = 0.01f;
    
        joint.linearLimit = limit;
    
        // Add damping to the joint itself
        JointDrive drive = new JointDrive();
        drive.positionSpring = 1000f;
        drive.positionDamper = 10000f;
        drive.maximumForce = Mathf.Infinity;
    
        joint.xDrive = drive;
        joint.yDrive = drive;
        joint.zDrive = drive;
    
        // Set target rotation to be aligned with previous segment
        joint.targetRotation = Quaternion.identity; // Try to stay aligned
    
        previousSegmentRigidBody = wormPart.GetComponent<Rigidbody>();
        return previousSegmentRigidBody;
    }
}
