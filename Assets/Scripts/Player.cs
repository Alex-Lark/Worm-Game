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
    private float _movementPhase = 0f;

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
            Vector3 fullCameraDirection = thirdPersonCamera.transform.forward.normalized;
            Vector3 currentForward = wormHead.forward;

            // --- Horizontal (yaw) ---
            Vector3 horizontalCameraDir = fullCameraDirection;
            horizontalCameraDir.y = 0f;
            if (horizontalCameraDir.sqrMagnitude < 1e-6f) horizontalCameraDir = Vector3.forward;
            horizontalCameraDir.Normalize();

            Vector3 horizontalCurrentDir = currentForward;
            horizontalCurrentDir.y = 0f;
            if (horizontalCurrentDir.sqrMagnitude < 1e-6f) horizontalCurrentDir = Vector3.forward;
            horizontalCurrentDir.Normalize();

            Quaternion qHorizCurrent = Quaternion.LookRotation(horizontalCurrentDir);
            Quaternion qHorizTarget = Quaternion.LookRotation(horizontalCameraDir);
            Quaternion qHorizNew = Quaternion.Slerp(
                qHorizCurrent,
                qHorizTarget,
                velocityScaledRotationSpeed * Time.fixedDeltaTime
            );

            // --- Camera pitch (signed) ---
            // Use SignedAngle so we get negative for looking down, positive for looking up
            float cameraPitchDeg = Vector3.SignedAngle(
                Vector3.ProjectOnPlane(fullCameraDirection, Vector3.up),
                fullCameraDirection,
                thirdPersonCamera.transform.right
            );

            // Match these to your camera vertical limits (FreeLook Y axis limits)
            float minCameraPitch = -10f;   // camera looking down limit (degrees)
            float maxCameraPitch = 45f;    // camera looking up limit (degrees)

            // Remap camera pitch -> 0..1
            float normalizedInput = Mathf.InverseLerp(minCameraPitch, maxCameraPitch, cameraPitchDeg);
            normalizedInput = Mathf.Clamp01(normalizedInput);

            // If mapping feels inverted, uncomment the next line:
            normalizedInput = 1f - normalizedInput;

            // Map to worm full pitch range
            float wormMinPitch = -85f;
            float wormMaxPitch = 85f;
            float targetPitch = Mathf.Lerp(wormMinPitch, wormMaxPitch, normalizedInput);

            // --- Current worm pitch and smoothing ---
            float currentPitch = Mathf.Asin(Mathf.Clamp(currentForward.y, -1f, 1f)) * Mathf.Rad2Deg;
            float velocityScaledVerticalRotationSpeed = GameParameters.WormHeadVerticalRotationSpeed *
                                                        (1f + currentSpeed / GameParameters.WormMoveForce);
            float newPitch = Mathf.LerpAngle(currentPitch, targetPitch, velocityScaledVerticalRotationSpeed * Time.fixedDeltaTime);

            // --- Compose final rotation: yaw then pitch about local right ---
            Vector3 horizontalForward = qHorizNew * Vector3.forward;
            float yaw = Mathf.Atan2(horizontalForward.x, horizontalForward.z) * Mathf.Rad2Deg;
            Quaternion qYaw = Quaternion.AngleAxis(yaw, Vector3.up);

            // local right AFTER yaw
            Vector3 localRight = qYaw * Vector3.right;

            // IMPORTANT: positive pitch should make the head look UP.
            // Quaternion.AngleAxis rotates the forward vector DOWN for positive angles around the right axis,
            // so we invert the pitch angle here.
            Quaternion qPitchLocal = Quaternion.AngleAxis(-newPitch, localRight);

            wormHead.rotation = qPitchLocal * qYaw;

            // --- Movement ---
            Vector3 moveDirection = wormHead.forward;
            wormHeadRigidbody.AddForce(GameParameters.WormMoveForce * moveDirection);

        #if UNITY_EDITOR
            // Debug help: uncomment while fiddling
            // Debug.Log($"camPitch={cameraPitchDeg:F1} norm={normalizedInput:F2} targetPitch={targetPitch:F1} currPitch={currentPitch:F1} newPitch={newPitch:F1}");
        #endif
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
    
    if (_isWormMoving)
    {
        AdvancedWormMove();
    }
    
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
            // if (!(partRigigBody.linearVelocity.magnitude > GameParameters.WormMaxVelocity))
            // {
            //     WormPart wormPart = part.GetComponent<WormPart>();
            //     if (wormPart.IsGrounded)
            //     {
            //         GameObject groundObject = wormPart.GroundObject;
            //         float moveForce = GameParameters.WormMoveForce - forceMagnitude;
            //         
            //         if (DetectStep(part.position, previousPart.forward, part.GetComponent<Collider>(), out float stepHeight))
            //         {
            //             float climbForce = GameParameters.WormStepClimbForce * (stepHeight / GameParameters.MaxStepHeight);
            //             partRigigBody.AddForce(Vector3.up * climbForce);
            //         }
            //     
            //         Vector3 moveDirection = GetSlopeAlignedDirection(previousPart.forward, wormPart.GroundNormal);
            //         if (groundObject != null)
            //         {
            //             Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
            //             if (groundRb != null)
            //             {
            //                 groundRb.AddForceAtPosition(-moveForce * moveDirection, part.position);
            //             }
            //             else
            //             {
            //                 part.GetComponent<Rigidbody>().AddForce(moveForce * moveDirection);
            //             }
            //         }
            //     }
            //}
        }
        previousPosition = part.position;
        previousPart = part;
    }
}

private void AdvancedWormMove()
{
    if (!_isWormMoving)
    {
        _movementPhase = 0f;
        return; // Pause in place when not moving
    }
    
    List<Transform> wormGroundedParts = new List<Transform>();
    
    foreach (var part in wormParts)
    {
        if (part.GetComponent<WormPart>().IsGrounded)
        {
            wormGroundedParts.Add(part);
        }
    }
    
    int largestStartIndex = -1;
    int largestCount = 0;
    
    int currentStartIndex = -1;
    int currentCount = 0;
    
    for (int i = 0; i < wormParts.Count; i++)
    {
        var part = wormParts[i].GetComponent<WormPart>();
        
        if (part.IsGrounded)
        {
            if (currentStartIndex == -1)
                currentStartIndex = i;
            
            currentCount++;
        }
        else
        {
            if (currentCount > largestCount)
            {
                largestCount = currentCount;
                largestStartIndex = currentStartIndex;
            }
            
            currentStartIndex = -1;
            currentCount = 0;
        }
    }
    
    // Edge case: sequence ends at last element
    if (currentCount > largestCount)
    {
        largestCount = currentCount;
        largestStartIndex = currentStartIndex;
    }
    
    // Find the middle point of the largest consecutive grounded group
    if (largestStartIndex != -1)
    {
        int middleIndex = largestStartIndex + (largestCount / 2);
        Transform middlePart = wormParts[middleIndex];
        
        float maxMiddleHeight = GameParameters.WormMiddleMaxHeight;
        float movementLoopLength = GameParameters.WormForwardMovementLoopLength;
        
        // Update movement phase (0 to 1 cycle)
        _movementPhase += Time.fixedDeltaTime / movementLoopLength;
        if (_movementPhase > 1f)
            _movementPhase = 0f;
        
        // Phase 1 (0-0.33): Parts behind middle move forward and scrunch up
        if (_movementPhase < 0.33f)
        {
            // Move rear segments forward toward the segment in front of them
            for (int i = middleIndex + 1; i < wormParts.Count; i++)
            {
                Transform part = wormParts[i];
                Rigidbody partRb = part.GetComponent<Rigidbody>();
                WormPart wormPart = part.GetComponent<WormPart>();
                
                if (wormPart.IsGrounded)
                {
                    // Get the part in front (toward head)
                    Transform targetPart = (i > 0) ? wormParts[i - 1] : wormHead;
                    Vector3 directionToTarget = (targetPart.position - part.position).normalized;
                    
                    // Project direction onto ground plane
                    Vector3 moveDir = GetSlopeAlignedDirection(directionToTarget, wormPart.GroundNormal);
                    GameObject groundObject = wormPart.GroundObject;
                    
                    if (groundObject != null)
                    {
                        Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
                        if (groundRb != null)
                        {
                            groundRb.AddForceAtPosition(-GameParameters.WormMoveForce * moveDir, part.position);
                        }
                        else
                        {
                            partRb.AddForce(GameParameters.WormMoveForce * moveDir);
                        }
                    }
                }
            }
        }
        
        // Phase 2 (0.33-0.66): Middle segment scrunches upward
        else if (_movementPhase < 0.66f)
        {
            Rigidbody middleRb = middlePart.GetComponent<Rigidbody>();
            float currentHeight = middlePart.position.y;
            
            // Apply upward force if below max height
            if (currentHeight < maxMiddleHeight)
            {
                float heightDiff = maxMiddleHeight - currentHeight;
                float upwardForce = Mathf.Clamp(heightDiff * GameParameters.WormScrunchForceMultiplier, 0f, GameParameters.WormJumpForce);
                middleRb.AddForce(Vector3.up * upwardForce);
            }
        }
        
        // Phase 3 (0.66-1.0): Front of worm moves forward
        else
        {
            // Move head toward camera forward direction
            if (wormHead.GetComponent<WormPart>().IsGrounded)
            {
                WormPart headPart = wormHead.GetComponent<WormPart>();
                Vector3 moveDir = GetSlopeAlignedDirection(wormHead.forward, headPart.GroundNormal);
                GameObject groundObject = headPart.GroundObject;
                Rigidbody headRb = wormHead.GetComponent<Rigidbody>();
                
                if (groundObject != null)
                {
                    Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
                    if (groundRb != null)
                    {
                        groundRb.AddForceAtPosition(-GameParameters.WormMoveForce * moveDir, wormHead.position);
                    }
                    else
                    {
                        headRb.AddForce(GameParameters.WormMoveForce * moveDir);
                    }
                }
            }
            
            // Move front segments toward the segment in front of them
            for (int i = 0; i < middleIndex; i++)
            {
                Transform part = wormParts[i];
                Rigidbody partRb = part.GetComponent<Rigidbody>();
                WormPart wormPart = part.GetComponent<WormPart>();
                
                if (wormPart.IsGrounded)
                {
                    // Get the part in front (closer to head)
                    Transform targetPart = (i > 0) ? wormParts[i - 1] : wormHead;
                    Vector3 directionToTarget = (targetPart.position - part.position).normalized;
                    
                    // Project direction onto ground plane
                    Vector3 moveDir = GetSlopeAlignedDirection(directionToTarget, wormPart.GroundNormal);
                    GameObject groundObject = wormPart.GroundObject;
                    
                    if (groundObject != null)
                    {
                        Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
                        if (groundRb != null)
                        {
                            groundRb.AddForceAtPosition(-GameParameters.WormMoveForce * moveDir, part.position);
                        }
                        else
                        {
                            partRb.AddForce(GameParameters.WormMoveForce * moveDir);
                        }
                    }
                }
            }
        }
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

