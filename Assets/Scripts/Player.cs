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
    private bool isWormMoving = false;

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
        //GetComponent<WormPhysics>().AddCollidersToSegments();
        //GetComponent<WormPhysics>().SetupWormCollisions();
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
        Vector3 camForward = thirdPersonCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();
    
        // Set rotation directly (freezeRotation allows this)
        if (camForward.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            wormVisualHead.rotation = targetRotation;
        }
        
        MoveWormBody();
    }

    public void StartWormMoving()
    {
        isWormMoving = true;
    }

    public void StopWormMoving()
    {
        isWormMoving = false;
        
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
    }
    
    private void MoveWormBody()
    {
        Vector3 previousPosition = wormHead.transform.position;
        Transform previousPart = wormHead;
        
        for (int i = 0; i < wormParts.Count; i++)
        {
            Transform part = wormParts[i];
            
            //calculating angle
            Vector3 partToPreviousPartVector = (part.position - previousPosition).normalized;
            Vector3 backVector = (-previousPart.forward).normalized;
            Vector3 axis = previousPart.up;
            float signedAngle = Vector3.SignedAngle(partToPreviousPartVector, backVector, axis);

            float forceMagnitude = 0;

            if (Mathf.Abs(signedAngle) > GameParameters.MaxWormTurnAngle)
            {
                Debug.Log("Segment " + i + " angle: " + signedAngle);
                
                float excessAngle = Mathf.Abs(signedAngle) - GameParameters.MaxWormTurnAngle;
                float t = Mathf.Clamp01(excessAngle / 90f);
                forceMagnitude = t * GameParameters.WormMoveSpeed;
                
                Vector3 correctionDir = Vector3.RotateTowards(partToPreviousPartVector, backVector, Mathf.Deg2Rad * excessAngle, 0f).normalized;
                
                Vector3 correctionForce = correctionDir * (forceMagnitude);
                Debug.Log("Applying force magnitude" + (forceMagnitude) + " to segment " + i);
                
                part.GetComponent<Rigidbody>().AddForce(correctionForce);
            }
            if (isWormMoving)
            {
                part.GetComponent<Rigidbody>().AddForce((GameParameters.WormMoveSpeed - forceMagnitude) * previousPart.forward);
            }
            
            previousPosition = part.position;
            previousPart = part;
        }
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
            
            Debug.Log("Head forward: " + wormHead.forward);
            Debug.Log("PrevPart forward: " + previousSegmentRigidBody.gameObject.transform.forward);
            Debug.Log("BackDir: " + backDir);
        }
    }

    private Rigidbody AddJoint(Transform wormPart, Rigidbody previousSegmentRigidBody) 
    {
        ConfigurableJoint joint = wormPart.AddComponent<ConfigurableJoint>();
        joint.connectedBody = previousSegmentRigidBody;
    
        // Lock linear motion to maintain exact distance
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
    
        // Set the anchor point to be maxPartDistance behind the previous segment
        joint.anchor = Vector3.back * maxPartDistance; // Local space offset
        joint.connectedAnchor = Vector3.zero; // Previous segment's center
    
        // Limited angular motion within maxAngle cone
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;
    
        // Set angular limits to maxAngle
        SoftJointLimit angularLimit = new SoftJointLimit();
        angularLimit.limit = maxAngle; // Cone angle from directly behind
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
    
        // Set target rotation to be aligned with previous segment
        joint.targetRotation = Quaternion.identity; // Try to stay aligned
    
        previousSegmentRigidBody = wormPart.GetComponent<Rigidbody>();
        return previousSegmentRigidBody;
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
}

