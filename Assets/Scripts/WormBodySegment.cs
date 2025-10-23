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
    
        joint.anchor = new Vector3(0, 0, -GameParameters.SegmentMaxPartDistance);
        float maxAngle = GameParameters.MaxJointAngle; // Degrees is correct
        
        // Lock all position axes so it stays in place
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        
        // Set angular motion to Limited
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;
        
        // Configure angle limits with spring to smoothly return
        SoftJointLimit lowXLimit = new SoftJointLimit { 
            limit = -maxAngle,
            bounciness = 0f,
            contactDistance = 0f // Start applying spring force 1 degree before limit
        };
        SoftJointLimit highXLimit = new SoftJointLimit { 
            limit = maxAngle,
            bounciness = 0f,
            contactDistance = 0f
        };
        SoftJointLimit yzLimit = new SoftJointLimit { 
            limit = maxAngle,
            bounciness = 0f,
            contactDistance = 0f
        };
        
        joint.lowAngularXLimit = lowXLimit;
        joint.highAngularXLimit = highXLimit;
        joint.angularYLimit = yzLimit;
        joint.angularZLimit = yzLimit;
        
        // Configure spring and damper on limits
        SoftJointLimitSpring limitSpring = new SoftJointLimitSpring {
            spring = 100f,  // Spring force to return to valid range
            damper = 10f    // Damping to smooth the return motion
        };
        
        joint.angularXLimitSpring = limitSpring;
        joint.angularYZLimitSpring = limitSpring;
        
        // Disable the angular drive - only use limit springs
        JointDrive angularDrive = new JointDrive();
        angularDrive.positionSpring = 0f;  // No spring to center
        angularDrive.positionDamper = 100f;  // Keep damping to reduce rotation
        angularDrive.maximumForce = 1000f;
        
        joint.angularXDrive = angularDrive;
        joint.angularYZDrive = angularDrive;
        
        joint.rotationDriveMode = RotationDriveMode.XYAndZ;

        previousSegmentRigidBody = wormPart.GetComponent<Rigidbody>();
        return previousSegmentRigidBody;
    }
}
