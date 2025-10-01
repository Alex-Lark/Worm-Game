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
    
    private bool _isWormMoving = false;

    private readonly int _wormSegmentCount = GameParameters.WormSegmentCount;
    private readonly float _moveForce = GameParameters.WormMoveForce;
    private readonly float _wormHeadRotationSpeed = GameParameters.WormHeadRotationSpeed;
    private readonly float _maxPartDistance = GameParameters.SegmentMaxPartDistance;
    private readonly float _maxAngle = GameParameters.MaxWormTurnAngle;

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
    }

    private void FixedUpdate()
    {
        RotateVisualHead();
        MoveWormBody();
    }

    public void StartWormMoving()
    {
        _isWormMoving = true;
    }

    public void StopWormMoving()
    {
        _isWormMoving = false;
        
    }

    public void MoveForward() 
    {
        //TODO: only if grounded
        //TODO: head can exert some vertical control if not grounded
        //TODO: add max speed
        
        Vector3 cameraForwardRotation = GetCameraForwardRotation();
        Rigidbody wormHeadRigidbody = wormHead.GetComponent<Rigidbody>();
        
        Quaternion desiredRotation = Quaternion.LookRotation(cameraForwardRotation);
        Quaternion currentRotation = wormHead.rotation;
        
        wormHead.rotation = Quaternion.Slerp(currentRotation, desiredRotation, _wormHeadRotationSpeed * Time.deltaTime);
        
        wormHeadRigidbody.AddForce(_moveForce * wormHead.forward);
    }
    
    private void RotateVisualHead()
    {
        //TODO: allow vertical rotation
        
        Vector3 cameraForwardRotation = GetCameraForwardRotation();
        
        if (cameraForwardRotation.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForwardRotation);
            wormVisualHead.rotation = targetRotation;
        }
        
    }

    private Vector3 GetCameraForwardRotation()
    {
        Vector3 camForward = thirdPersonCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        return camForward;
    }
    
    private void MoveWormBody()
    {
        Vector3 previousPosition = wormHead.transform.position;
        Transform previousPart = wormHead;
        
        for (int i = 0; i < wormParts.Count; i++)
        {
            Transform part = wormParts[i];
            Rigidbody partRigigBody = part.gameObject.GetComponent<Rigidbody>();
            
            //calculating angle
            Vector3 partToPreviousPartVector = (part.position - previousPosition).normalized;
            Vector3 backVector = (-previousPart.forward).normalized;
            Vector3 axis = previousPart.up;
            float signedAngle = Vector3.SignedAngle(partToPreviousPartVector, backVector, axis);
            
            float forceMagnitude = 0;

            if (Mathf.Abs(signedAngle) > GameParameters.MaxWormTurnAngle)
            {
                Debug.Log("Segment " + i + " angle: " + signedAngle);
                
                float baseForceMagnitude = 0;
                
                float excessAngle = Mathf.Abs(signedAngle) - GameParameters.MaxWormTurnAngle;
                float t = Mathf.Clamp01(excessAngle / 90f);
                baseForceMagnitude = t * t * GameParameters.WormMoveForce;
                
                Vector3 correctionDir = Vector3.RotateTowards(partToPreviousPartVector, backVector, Mathf.Deg2Rad * excessAngle, 0f).normalized;
                float velocityInCorrectionDir = Vector3.Dot(partRigigBody.linearVelocity, correctionDir);
                
                Vector3 correctionForce = correctionDir * (forceMagnitude);
                Debug.Log("Applying force magnitude" + (forceMagnitude) + " to segment " + i + " it's current velocity in the correct direction is: " + velocityInCorrectionDir);
                
                forceMagnitude = Mathf.Clamp(baseForceMagnitude - (velocityInCorrectionDir * 2), 0f, GameParameters.WormMoveForce);
                part.GetComponent<Rigidbody>().AddForce(correctionForce);
            }
            
            //TODO: only works while grounded
            
            if (_isWormMoving)
            {
                part.GetComponent<Rigidbody>().AddForce((GameParameters.WormMoveForce - forceMagnitude) * previousPart.forward);
            }
            
            //TODO: apply "sticky" force downwards if not moving
            
            previousPosition = part.position;
            previousPart = part;
        }
    }

    public void Jump()
    {
        //TODO: only works if part is on ground
        
        wormHead.GetComponent<Rigidbody>().AddForce(GameParameters.WormJumpForce * wormHead.up);

        for (int i = 0; i < wormParts.Count; i++)
        {
            wormParts[i].GetComponent<Rigidbody>().AddForce(GameParameters.WormJumpForce * wormHead.up);
        }
        
    }
    
    private void CreateWormSegments()
    {
        for (int i = 0; i < _wormSegmentCount; i++)
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
            currentPos += backDir * _maxPartDistance;

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
    
        // Lock linear motion to maintain exact distance
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
    
        // Set the anchor point to be maxPartDistance behind the previous segment
        joint.anchor = Vector3.back * _maxPartDistance; // Local space offset
        joint.connectedAnchor = Vector3.zero; // Previous segment's center
    
        // Limited angular motion within maxAngle cone
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;
    
        // Set angular limits to maxAngle
        SoftJointLimit angularLimit = new SoftJointLimit();
        angularLimit.limit = _maxAngle; // Cone angle from directly behind
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

