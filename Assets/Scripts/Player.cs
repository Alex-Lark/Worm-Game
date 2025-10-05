using System.Collections.Generic;
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
        gameObject.GetComponent<WormPhysics>().AddCollidersToSegments();
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
        //TODO: implement better slope/step control
        //TODO: head can exert some vertical control if not grounded
        //TODO: add max speed
        
        Vector3 cameraForwardRotation = GetCameraForwardRotation();
        Rigidbody wormHeadRigidbody = wormHead.GetComponent<Rigidbody>();
        
        Quaternion desiredRotation = Quaternion.LookRotation(cameraForwardRotation);
        Quaternion currentRotation = wormHead.rotation;
        
        wormHead.rotation = Quaternion.Slerp(currentRotation, desiredRotation, _wormHeadRotationSpeed * Time.deltaTime);

        if (wormHeadRigidbody.GetComponent<WormPart>().IsGrounded)
        {
            wormHeadRigidbody.AddForce(_moveForce * wormHead.forward);
        }
    }
    
    private void RotateVisualHead()
    {
        var forward = thirdPersonCamera.transform.forward;
        
        Vector3 cameraForward = new Vector3(forward.x, forward.y + GameParameters.VisualHeadVerticalOffset, forward.z);
        cameraForward.Normalize();

        if (cameraForward.magnitude > 0.1f)
        {
            float angle = Vector3.Angle(wormHead.forward, cameraForward);
            
            if (angle > 90f)
            {
                Vector3 clampedDirection = Vector3.RotateTowards(wormHead.forward, cameraForward, 90f * Mathf.Deg2Rad, 0f);
                wormVisualHead.rotation = Quaternion.LookRotation(clampedDirection);
            }
            else
            {
                wormVisualHead.rotation = Quaternion.LookRotation(cameraForward);
            }
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
            
            if (_isWormMoving)
            {
                if (part.GetComponent<WormPart>().IsGrounded)
                {
                    part.GetComponent<Rigidbody>().AddForce((GameParameters.WormMoveForce - forceMagnitude) * previousPart.forward);
                }
            }
            
            previousPosition = part.position;
            previousPart = part;
        }
    }

    public void Jump()
    {
        if (wormHead.GetComponent<WormPart>().IsGrounded)
        {
            wormHead.GetComponent<Rigidbody>().AddForce(GameParameters.WormJumpForce * wormHead.up);
        }

        for (int i = 0; i < wormParts.Count; i++)
        {
            if (wormParts[i].GetComponent<WormPart>().IsGrounded)
            {
                wormParts[i].GetComponent<Rigidbody>().AddForce(GameParameters.WormJumpForce * wormHead.up);
            }
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

            previousSegmentRigidBody = part.GetComponent<WormBodySegment>().AddJoint(wormParts[i], previousSegmentRigidBody);
        }
    }
}

