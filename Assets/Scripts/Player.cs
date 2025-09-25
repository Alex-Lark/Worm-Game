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

    private void FixedUpdate()
    {
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
    
        // Calculate target rotation: move towards camForward from current rotation by MaxAngle
        Quaternion desiredRotation = Quaternion.LookRotation(camForward);
        Quaternion currentRotation = wormHead.rotation;
        Quaternion targetRotation = ApplyTurnConstraint(currentRotation, desiredRotation);
        
        wormHead.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
    
        // Move forward towards new orientation
        rigidbody.AddForce(moveSpeed * wormHead.forward);
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

            previousSegmentRigidBody =  AddJoint(wormParts[i], previousSegmentRigidBody);
        }
    }

    private Rigidbody AddJoint(Transform wormPart, Rigidbody previousSegmentRigidBody) 
    {
        ConfigurableJoint joint = wormPart.AddComponent<ConfigurableJoint>();
        joint.connectedBody = previousSegmentRigidBody;
    
        // Lock all motion to replicate FixedJoint behavior
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
    
        // Set angular limits to MaxTurnAngle
        SoftJointLimit angularLimit = new SoftJointLimit();
        angularLimit.limit = maxAngle; // MaxTurnAngle in degrees
        angularLimit.bounciness = 0f; // No bouncing at limits
    
        joint.lowAngularXLimit = angularLimit;
        joint.highAngularXLimit = angularLimit;
        joint.angularYLimit = angularLimit;
        joint.angularZLimit = angularLimit;
    
        previousSegmentRigidBody = wormPart.GetComponent<Rigidbody>();
    
        return previousSegmentRigidBody;
    }

    private Quaternion ApplyTurnConstraint(Quaternion currentRotation, Quaternion desiredRotation)
    {
        float angle = Quaternion.Angle(currentRotation, desiredRotation);
        
        if (angle <= maxAngle)
        {
            return desiredRotation;
        }
        
        float t = maxAngle / angle; 
        return Quaternion.Slerp(currentRotation, desiredRotation, t);
    }

    private void MoveWormBody()
    {
        Vector3 previousPosition = wormHead.transform.position;
        float maxMovePerFrame = moveSpeed * Time.deltaTime;

        for (int i = 0; i < wormParts.Count; i++)
        {
            Transform part = wormParts[i];
            Vector3 toPrev = previousPosition - part.position;
            float distance = toPrev.magnitude;

            if (distance > maxPartDistance)
            {
                float moveDistance = distance - maxPartDistance;
                moveDistance = Mathf.Min(moveDistance, maxMovePerFrame);

                part.position += toPrev.normalized * moveDistance;
                
                if (toPrev.sqrMagnitude > 0.001f)
                {
                    Quaternion desiredBodyRotation = Quaternion.LookRotation(toPrev);
                    Quaternion constrainedBodyRotation = ApplyTurnConstraint(part.rotation, desiredBodyRotation);

                    part.rotation = Quaternion.Slerp(part.rotation,
                        constrainedBodyRotation,
                        rotationSpeed * Time.deltaTime);
                }
            }

            previousPosition = part.position;
        }
    }
}

