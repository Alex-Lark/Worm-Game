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
    private bool _isWormGrounded = false;

    private readonly int _wormSegmentCount = GameParameters.WormSegmentCount;
    private readonly float _wormHeadRotationSpeed = GameParameters.WormHeadRotationSpeed;
    private readonly float _maxPartDistance = GameParameters.SegmentMaxPartDistance;
    
    private readonly RaycastHit[] _stepDetectionHits = new RaycastHit[10];


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
        setWormGrounding();
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
        //TODO: head can exert some vertical control if not grounded
        print("worm head rotation: " + wormHead.rotation);
        
        Vector3 cameraForwardRotation = GetCameraForwardRotation();
        Rigidbody wormHeadRigidbody = wormHead.GetComponent<Rigidbody>();
        
        Vector3 forward = wormHead.forward;
        forward.y = 0f;
        forward.Normalize();
        
        Quaternion desiredRotation = Quaternion.LookRotation(cameraForwardRotation);
        Quaternion currentRotation = Quaternion.LookRotation(forward);
        
        float currentSpeed = wormHeadRigidbody.linearVelocity.magnitude;
        float velocityScaledRotationSpeed = _wormHeadRotationSpeed * (1f + currentSpeed / GameParameters.WormMoveForce);
        
        wormHead.rotation = Quaternion.Slerp(currentRotation, desiredRotation, velocityScaledRotationSpeed * Time.fixedDeltaTime);

        if (wormHeadRigidbody.GetComponent<WormPart>().IsGrounded)
        {
            if (!(wormHeadRigidbody.linearVelocity.magnitude > GameParameters.WormMaxVelocity))
            {
                GameObject groundObject = wormHeadRigidbody.GetComponent<WormPart>().GroundObject;
                Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
                
                if (DetectStep(wormHead.position, wormHead.forward, wormHead.GetComponent<Collider>(), out float stepHeight))
                {
                    float climbForce = GameParameters.WormStepClimbForce * (stepHeight / GameParameters.MaxStepHeight);
                    wormHeadRigidbody.AddForce(Vector3.up * climbForce);
                }
                
                Vector3 moveDirection = GetSlopeAlignedDirection(wormHead.forward, wormHead.GetComponent<WormPart>().GroundNormal);
                
                if (groundRb != null)
                {
                    groundRb.AddForceAtPosition(-GameParameters.WormMoveForce * moveDirection, wormHead.position);
                }
                else
                {
                    wormHeadRigidbody.AddForce(GameParameters.WormMoveForce * moveDirection);
                }
            }
        }
        else if (_isWormGrounded) 
{
    // Head is ungrounded but worm body is grounded - allow vertical control
    Vector3 fullCameraDirection = thirdPersonCamera.transform.forward;
    fullCameraDirection.Normalize();
    
    // Get current forward direction
    Vector3 currentForward = wormHead.forward;
    
    // Horizontal component (yaw)
    Vector3 horizontalCameraDir = fullCameraDirection;
    horizontalCameraDir.y = 0f;
    horizontalCameraDir.Normalize();
    
    Vector3 horizontalCurrentDir = currentForward;
    horizontalCurrentDir.y = 0f;
    horizontalCurrentDir.Normalize();
    
    // Vertical component (pitch) - calculate angle from horizontal plane
    float targetPitch = Mathf.Asin(fullCameraDirection.y) * Mathf.Rad2Deg;
    float currentPitch = Mathf.Asin(currentForward.y) * Mathf.Rad2Deg;
    
    // Interpolate horizontal rotation with velocityScaledRotationSpeed
    Quaternion horizontalRotation = Quaternion.LookRotation(horizontalCurrentDir);
    Quaternion targetHorizontalRotation = Quaternion.LookRotation(horizontalCameraDir);
    Quaternion newHorizontalRotation = Quaternion.Slerp(horizontalRotation, targetHorizontalRotation, velocityScaledRotationSpeed * Time.fixedDeltaTime);
    
    // Interpolate vertical rotation with velocityScaledVerticalRotationSpeed
    float velocityScaledVerticalRotationSpeed = GameParameters.WormHeadVerticalRotationSpeed * (1f + currentSpeed / GameParameters.WormMoveForce);
    float newPitch = Mathf.Lerp(currentPitch, targetPitch, velocityScaledVerticalRotationSpeed * Time.fixedDeltaTime);
    
    // Combine horizontal yaw with vertical pitch
    Vector3 horizontalForward = newHorizontalRotation * Vector3.forward;
    float pitchRotation = -newPitch;
    wormHead.rotation = Quaternion.LookRotation(horizontalForward) * Quaternion.Euler(pitchRotation, 0f, 0f);
    
    // Apply force in the direction the head is facing (includes vertical)
    Vector3 moveDirection = wormHead.forward;
    wormHeadRigidbody.AddForce(GameParameters.WormMoveForce * moveDirection);
}
    }
    
    private bool DetectStep(Vector3 position, Vector3 forward, Collider partCollider, out float stepHeight)
    {
        stepHeight = 0f;
    
        // Use horizontal forward direction for step detection, ignore vertical component
        Vector3 horizontalForward = new Vector3(forward.x, 0f, forward.z).normalized;
        
        if (horizontalForward.magnitude < 0.1f)
            return false;
    
        // Start raycast slightly forward and down from segment center to avoid hitting self/neighbors
        Vector3 footLevelOrigin = position + horizontalForward * (partCollider.bounds.extents.x * 1.2f) 
                                  - Vector3.up * (partCollider.bounds.extents.y * 0.5f);

        // Cast multiple rays to avoid missing steps between segments
        int hitCount = Physics.RaycastNonAlloc(footLevelOrigin, horizontalForward, _stepDetectionHits, GameParameters.StepDetectionDistance);
    
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit footHit = _stepDetectionHits[i];
        
            // Skip ALL worm parts, not just from same root
            if (footHit.collider.transform.root == transform.root)
                continue;
    
            // Check if there's walkable space above the obstacle
            Vector3 topCheckOrigin = footLevelOrigin + horizontalForward * GameParameters.StepDetectionDistance 
                                                     + Vector3.up * GameParameters.MaxStepHeight;
        
            if (!Physics.Raycast(topCheckOrigin, Vector3.down, out RaycastHit topHit, GameParameters.MaxStepHeight * 1.5f))
                continue;
        
            if (topHit.collider.transform.root == transform.root)
                continue;
        
            stepHeight = topHit.point.y - (position.y - partCollider.bounds.extents.y);
        
            if (stepHeight > 0.05f && stepHeight <= GameParameters.MaxStepHeight)
            {
                return true;
            }
        }
    
        return false;
    }
    
    private Vector3 GetSlopeAlignedDirection(Vector3 forward, Vector3 groundNormal)
    {
        Vector3 slopeDirection = Vector3.ProjectOnPlane(forward, groundNormal).normalized;
    
        // If on a steep slope, blend with horizontal direction to maintain some control
        float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
        if (slopeAngle > GameParameters.MaxSlopeAngle)
        {
            float blendFactor = Mathf.InverseLerp(GameParameters.MaxSlopeAngle, 90f, slopeAngle);
            slopeDirection = Vector3.Lerp(slopeDirection, forward, blendFactor).normalized;
        }
    
        return slopeDirection;
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
            float excessAngle = Mathf.Abs(signedAngle) - GameParameters.MaxWormTurnAngle;
            float t = Mathf.Clamp01(excessAngle / 90f);
            float baseForceMagnitude = t * t * GameParameters.WormMoveForce;
            
            Vector3 correctionDir = Vector3.RotateTowards(partToPreviousPartVector, backVector, Mathf.Deg2Rad * excessAngle, 0f).normalized;
            float velocityInCorrectionDir = Vector3.Dot(partRigigBody.linearVelocity, correctionDir);
            
            Vector3 correctionForce = correctionDir * (forceMagnitude);
            
            forceMagnitude = Mathf.Clamp(baseForceMagnitude - (velocityInCorrectionDir), 0f, GameParameters.WormMoveForce);
            part.GetComponent<Rigidbody>().AddForce(correctionForce);
        }
        
        if (_isWormMoving)
        {
            if (!(partRigigBody.linearVelocity.magnitude > GameParameters.WormMaxVelocity))
            {
                WormPart wormPart = part.GetComponent<WormPart>();
                if (wormPart.IsGrounded)
                {
                    GameObject groundObject = wormPart.GroundObject;
                    float moveForce = GameParameters.WormMoveForce - forceMagnitude;
                    
                    if (DetectStep(part.position, previousPart.forward, part.GetComponent<Collider>(), out float stepHeight))
                    {
                        float climbForce = GameParameters.WormStepClimbForce * (stepHeight / GameParameters.MaxStepHeight);
                        partRigigBody.AddForce(Vector3.up * climbForce);
                    }
                
                    Vector3 moveDirection = GetSlopeAlignedDirection(previousPart.forward, wormPart.GroundNormal);
                    if (groundObject != null)
                    {
                        Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
                        if (groundRb != null)
                        {
                            groundRb.AddForceAtPosition(-moveForce * moveDirection, part.position);
                        }
                        else
                        {
                            part.GetComponent<Rigidbody>().AddForce(moveForce * moveDirection);
                        }
                    }
                }
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
        GameObject groundObject = wormHead.GetComponent<WormPart>().GroundObject;
        if (groundObject != null)
        {
            Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
            if (groundRb != null)
            {
                Vector3 forceToApply = -GameParameters.WormJumpForce * wormHead.up;

                groundRb.AddForceAtPosition(forceToApply, wormHead.position);
            }
            else
            {
                wormHead.GetComponent<Rigidbody>().AddForce(GameParameters.WormJumpForce * wormHead.up);
            }
        }
    }
    
    for (int i = 0; i < wormParts.Count; i++)
    {
        if (wormParts[i].GetComponent<WormPart>().IsGrounded)
        {
            GameObject groundObject = wormParts[i].GetComponent<WormPart>().GroundObject;
            if (groundObject != null)
            {
                Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
                if (groundRb != null)
                {
                    groundRb.AddForceAtPosition(-GameParameters.WormJumpForce * wormHead.up, wormParts[i].position);
                }
                else
                {
                    wormParts[i].GetComponent<Rigidbody>().AddForce(GameParameters.WormJumpForce * wormHead.up);
                }
            }
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

    private void setWormGrounding()
    {
        _isWormGrounded = false; 
        
        foreach (var part in wormParts)
        {
            if (part.GetComponent<WormPart>().IsGrounded)
            {
                _isWormGrounded = true;
            }
        }
    }
}

