using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public GameObject thirdPersonCamera;
    public GameObject wormSegmentPrefab;
    public Transform wormHead;
    public Transform wormVisualHead;
    public List<Transform> wormParts;

    private int wormSegmentCount = GameParameters.WormSegmentCount;
    private float moveSpeed = GameParameters.WormMoveSpeed;
    private float rotationSpeed = GameParameters.WormRotationSpeed;
    private float maxPartDistance = GameParameters.SegmentMaxPartDistance;
    private float maxAngle = GameParameters.MaxWormTurnAngle;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        wormParts.Clear();
        CreateWormSegments();
        ConstructWorm();
        GetComponent<WormPhysics>().AddCollidersToSegments();
        GetComponent<WormPhysics>().SetupWormCollisions();
    }
    
    void Update()
    {
        Rigidbody headRb = wormHead.GetComponent<Rigidbody>();
        if (headRb.angularVelocity.magnitude > 0.01f)
        {
            Debug.Log($"HEAD IS ROTATING when it shouldn't! Angular velocity: {headRb.angularVelocity}");
        }
    }

    private void FixedUpdate()
    {
            // Rigidbody rb = wormParts[0].GetComponent<Rigidbody>();
            // Rigidbody headRB = wormHead.GetComponent<Rigidbody>();
            // ConfigurableJoint conJoint = wormParts[0].GetComponent<ConfigurableJoint>();
            //
            // Debug.Log($"Segment 0 Angular Velocity: {rb.angularVelocity.magnitude:F3}");
            // Debug.Log($"Segment 0 Velocity: {rb.linearVelocity.magnitude:F3}");
            //
            //     Vector3 headAngularVel = headRB.angularVelocity;
            //     Debug.Log($"Head Angular Velocity: {headAngularVel.magnitude:F3}");
            //
            //     // Check if head is actually stationary
            //     Debug.Log($"Head Velocity: {headRB.linearVelocity.magnitude:F3}");
            //
            // // Check for external forces
            // Debug.Log($"Segment 0 Joint Current Force: {conJoint.currentForce.magnitude:F3}");
            // Debug.Log($"Segment 0 Joint Current Torque: {conJoint.currentTorque.magnitude:F3}");
        
        //MoveWormBody();
        
        Vector3 camForward = thirdPersonCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();
    
        // Set rotation directly (freezeRotation allows this)
        if (camForward.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            wormVisualHead.rotation = targetRotation;
        }
    }

    public void MoveForward() 
    {
        Vector3 camForward = thirdPersonCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Rigidbody rigidbody = wormHead.GetComponent<Rigidbody>();
    
        // Calculate target rotation: move towards camForward from current rotation by MaxWromHeadAngle
        Quaternion desiredRotation = Quaternion.LookRotation(camForward);
        Quaternion currentRotation = wormHead.rotation;
        Quaternion targetRotation = ApplyTurnConstraint(currentRotation, desiredRotation);
        
        wormHead.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
    
        // Move forward towards new orientation
        rigidbody.AddForce(moveSpeed * wormHead.forward);
        //MoveWormBody();

        // GameObject previousSegment = wormHead.gameObject;
        // for (int i = 0; i < wormSegmentCount; i++)
        // {
        //     Vector3 partForward = previousSegment.transform.forward;
        //     partForward.y = 0f;
        //     partForward.Normalize();
        //     
        //     desiredRotation = Quaternion.LookRotation(partForward);
        //     currentRotation = wormParts[i].rotation;
        //     targetRotation = ApplyTurnConstraint(currentRotation, desiredRotation);
        //     
        //     wormParts[i].rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        //     
        //     wormParts[i].GetComponent<Rigidbody>().AddForce(moveSpeed * wormHead.forward);
        // }
    }
    
    private void CreateWormSegments()
    {
        for (int i = 0; i < wormSegmentCount; i++)
        {
            GameObject newWormSegment = Instantiate(wormSegmentPrefab, transform);
            wormParts.Add(newWormSegment.transform);
        }
    }

    private void ConstructWorm()
    {
        
        Vector3 currentPos = wormHead.position;
        Vector3 backDir = -wormHead.forward;

        Rigidbody previousSegmentRigidBody = wormHead.gameObject.GetComponent<Rigidbody>();

        for (int i = 0; i < wormParts.Count; i++)
        {
            currentPos += backDir * maxPartDistance;

            Transform part = wormParts[i];
            part.position = currentPos;
            
            part.rotation = wormHead.rotation;

            previousSegmentRigidBody = AddJoint(wormParts[i], previousSegmentRigidBody);
            //var joint = wormParts[i].AddComponent<ConfigurableJoint>();
            //ChainJointConfigurator.ConfigureChainJoint(joint, previousSegmentRigidBody, angleLimitDegrees: 10f);
            //wormParts[i].GetComponent<FixedJoint>().connectedBody = previousSegmentRigidBody;
            previousSegmentRigidBody = wormParts[i].GetComponent<Rigidbody>();
        }
    }

    private Rigidbody AddJoint(Transform wormPart, Rigidbody previousSegmentRigidBody) 
    {
        //ConfigurableJoint joint = wormPart.GetComponent<ConfigurableJoint>();
        //joint.connectedBody = previousSegmentRigidBody;
    
        // // Instead of locking all linear motion, allow limited movement in a sphere
        // joint.xMotion = ConfigurableJointMotion.Limited;
        // joint.yMotion = ConfigurableJointMotion.Limited;
        // joint.zMotion = ConfigurableJointMotion.Limited;
        //
        // // Set tight linear limits to prevent side-to-side swinging
        // SoftJointLimit linearLimit = new SoftJointLimit();
        // linearLimit.limit = maxPartDistance * Mathf.Sin(maxAngle * Mathf.Deg2Rad); // Max side movement based on angle
        // linearLimit.bounciness = 0f;
        //
        // joint.linearLimit = linearLimit;
        //
        // // Set the distance constraint
        // joint.anchor = Vector3.zero; // Center of this segment
        // joint.connectedAnchor = Vector3.zero; // Center of previous segment
        //
        // // Add strong springs to keep segments at proper distance
        // JointDrive linearDrive = new JointDrive();
        // linearDrive.positionSpring = 2000f;
        // linearDrive.positionDamper = 500f;
        // linearDrive.maximumForce = 5000f;
        //
        // joint.xDrive = linearDrive;
        // joint.yDrive = linearDrive;
        // joint.zDrive = linearDrive;
        //
        // // Set target position to maintain maxPartDistance behind previous segment
        // joint.targetPosition = Vector3.back * maxPartDistance;
        //
        // // Angular settings can be simpler now
        // joint.angularXMotion = ConfigurableJointMotion.Limited;
        // joint.angularYMotion = ConfigurableJointMotion.Limited;
        // joint.angularZMotion = ConfigurableJointMotion.Limited;
        //
        // SoftJointLimit angularLimit = new SoftJointLimit();
        // angularLimit.limit = maxAngle;
        // angularLimit.bounciness = 0f;
        //
        // joint.lowAngularXLimit = angularLimit;
        // joint.highAngularXLimit = angularLimit;
        // joint.angularYLimit = angularLimit;
        // joint.angularZLimit = angularLimit;
        
        ConfigurableJoint joint = wormPart.AddComponent<ConfigurableJoint>();
        joint.connectedBody = previousSegmentRigidBody;

// Allow some linear motion to prevent bunched-up segments
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;

        SoftJointLimitSpring linearSpring = new SoftJointLimitSpring();
        linearSpring.spring = 5000f; // higher = stiffer
        linearSpring.damper = 100f;  // damping to prevent oscillation
        joint.linearLimitSpring = linearSpring;

        SoftJointLimit linearLimit = new SoftJointLimit();
        linearLimit.limit = maxPartDistance; // the desired spacing
        joint.linearLimit = linearLimit;

// Angular constraints
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;

        SoftJointLimitSpring angularSpring = new SoftJointLimitSpring();
        angularSpring.spring = 5000f;
        angularSpring.damper = 50f;
        joint.angularXLimitSpring = angularSpring;
        joint.angularYZLimitSpring = angularSpring;

        SoftJointLimit angularLimit = new SoftJointLimit();
        angularLimit.limit = 45f;
        joint.lowAngularXLimit = angularLimit;
        joint.highAngularXLimit = angularLimit;
        joint.angularYLimit = angularLimit;
        joint.angularZLimit = angularLimit;
    
        return wormPart.GetComponent<Rigidbody>();
    }

    private Quaternion ApplyTurnConstraint(Quaternion currentRotation, Quaternion desiredRotation)
    {
        float angle = Quaternion.Angle(currentRotation, desiredRotation);
        
        if (angle <= GameParameters.MaxWormHeadTurnAngle)
        {
            return desiredRotation;
        }
        
        float t = GameParameters.MaxWormHeadTurnAngle / angle; 
        return Quaternion.Slerp(currentRotation, desiredRotation, t);
    }

    private void MoveWormBody()
    {
        Vector3 previousPosition = wormHead.transform.position;
        float maxMovePerFrame = moveSpeed * Time.fixedDeltaTime;

        for (int i = 0; i < wormParts.Count; i++)
        {
            Rigidbody rb = wormParts[i].GetComponent<Rigidbody>();
            if (rb == null) continue;

            Vector3 toPrev = previousPosition - rb.position;
            float distance = toPrev.magnitude;

            if (distance > maxPartDistance)
            {
                float moveDistance = distance - maxPartDistance;
                moveDistance = Mathf.Min(moveDistance, maxMovePerFrame);

                Vector3 targetPos = rb.position + toPrev.normalized * moveDistance;
                rb.MovePosition(targetPos);

                if (toPrev.sqrMagnitude > 0.001f)
                {
                    Quaternion desiredBodyRotation = Quaternion.LookRotation(toPrev);
                    Quaternion constrainedBodyRotation = ApplyTurnConstraint(rb.rotation, desiredBodyRotation);

                    rb.MoveRotation(Quaternion.Slerp(
                        rb.rotation,
                        constrainedBodyRotation,
                        rotationSpeed * Time.fixedDeltaTime));
                }
            }

            previousPosition = rb.position;
        }
    }
}

