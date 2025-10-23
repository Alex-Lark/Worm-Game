using UnityEngine;

public class JointTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ConfigureJoint();
    }

    private void ConfigureJoint() {
    ConfigurableJoint joint = GetComponent<ConfigurableJoint>();
    
    if (joint == null) {
        Debug.LogError("No ConfigurableJoint found on this GameObject!");
        return;
    }
    
    joint.anchor = new Vector3(-2, 0, 0);
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
        contactDistance = 1f // Start applying spring force 1 degree before limit
    };
    SoftJointLimit highXLimit = new SoftJointLimit { 
        limit = maxAngle,
        bounciness = 0f,
        contactDistance = 1f
    };
    SoftJointLimit yzLimit = new SoftJointLimit { 
        limit = maxAngle,
        bounciness = 0f,
        contactDistance = 1f
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
    angularDrive.positionDamper = 20f;  // Keep damping to reduce rotation
    angularDrive.maximumForce = 1000f;
    
    joint.angularXDrive = angularDrive;
    joint.angularYZDrive = angularDrive;
    
    joint.rotationDriveMode = RotationDriveMode.XYAndZ;
}

    // Update is called once per frame
    void Update()
    {
        
    }
}
