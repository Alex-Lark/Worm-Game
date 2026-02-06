using System.Collections.Generic;
using CreatureParts;
using UnityEngine;

public class WormForwardMovement : MonoBehaviour
{
    private GameObject _camera;
    private Transform _wormHead;
    private Rigidbody _wormHeadRb;

    private readonly RaycastHit[] _stepHits = new RaycastHit[10];
    private readonly List<float> _segmentMaxForwardForce = new List<float>();
    private float _movementPhase;

    void Start()
    {
        SetVariables();
    }

    public void SetVariables()
    {
        var player = Player.Player.Instance;
        _camera = player.thirdPersonCamera;
        _wormHead = player.wormHead;
        _wormHeadRb = _wormHead.GetComponent<Rigidbody>();
        CreateSegmentMaxForwardForceList();
    }
    
    public void CreateSegmentMaxForwardForceList()
    {
        for (int i = 0; i < GameParameters.WormSegmentCount; i++)
        {
            _segmentMaxForwardForce.Add(0);
        }
    }

    public void MoveHead()
    {
        float speedFactor = 1f + _wormHeadRb.linearVelocity.magnitude / GameParameters.WormMoveForce;
        float rotationSpeed = GameParameters.WormHeadRotationSpeed * speedFactor;

        var part = _wormHead.GetComponent<CreaturePart>();
        //if (part.IsGrounded)
        //{
            RotateHeadGrounded(rotationSpeed);
            MoveHeadGrounded(part);
        //}
        // else if (Player.Player.Instance.IsWormGrounded)
        // {
        //     RotateHeadUngrounded(rotationSpeed);
        //     MoveHeadUngrounded();
        // }
    }
    
    public void MoveWormBody()
    {
        Vector3 previousPosition = _wormHead.transform.position;
        Transform previousPart = _wormHead;
        
        List<Transform> wormParts = Player.Player.Instance.wormBodySegments;

        for (int i = 0; i < Player.Player.Instance.wormBodySegments.Count; i++)
        {
            _segmentMaxForwardForce[i] = GameParameters.WormMoveForce - TryToConstrainWormAngle(wormParts[i], _wormHead.transform, _wormHead.position);
        }

        (int groundedSegmentStartIndex, int groundedSegmentCount) = GetGroundedMiddleSegment(wormParts);

        if (groundedSegmentStartIndex != -1)
        {
            int middleIndex = groundedSegmentStartIndex + (groundedSegmentCount / 2);
            Transform middlePart = wormParts[middleIndex];
            
            float movementLoopLength = GameParameters.WormForwardMovementLoopLength;
            
            // Update movement phase (0 to 1 cycle)
            _movementPhase += Time.fixedDeltaTime / movementLoopLength;
            if (_movementPhase > 1f)
            {
                _movementPhase = 0f;
            }

            if (_movementPhase < 0.33f)
            {
                MoveBackPartsForward(wormParts, middleIndex);
            }
            else if (_movementPhase < 0.66)
            {
                MoveMiddleSegmentUp(middlePart, middleIndex);
            }
            else
            {
                MoveFrontPartsForward(wormParts, middleIndex);
            }
        }
    }

    private void MoveBackPartsForward(List<Transform> wormParts, int middleIndex)
    {
        Transform previousPart = wormParts[middleIndex];
            
        for (int i = middleIndex + 1; i < wormParts.Count; i++)
        {
            Transform part = wormParts[i];
            Rigidbody wormPartRigidbody = part.GetComponent<Rigidbody>();
            CreaturePart creaturePart = part.GetComponent<CreaturePart>();

            if ((creaturePart.IsGrounded || creaturePart.TimeSinceLastGrounded < GameParameters.maxTimeSinceLastGrounded) && !(wormPartRigidbody.linearVelocity.magnitude > gameObject.GetComponent<Player.Player>().MaxVelocity))
            {
                // Get the part in front (toward head)
                Transform targetPart = (i > 0) ? wormParts[i - 1] : _wormHead;
                Vector3 directionToTarget = (targetPart.position - part.position).normalized;
                    
                Vector3 moveDir = AlignToSlope(directionToTarget, creaturePart.GroundNormal);
                GameObject groundObject = creaturePart.GroundObject;

                //step movement
                if (DetectStep(part.position, previousPart.forward, part.GetComponent<Collider>(), out float stepHeight))
                {
                    float climbForce = GameParameters.WormStepClimbForce * (stepHeight / GameParameters.MaxStepHeight);
                    wormPartRigidbody.AddForce(Vector3.up * climbForce);
                    wormPartRigidbody.AddForce(previousPart.forward * climbForce);
                }
                    
                if (groundObject != null)
                {
                    Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
                    if (groundRb != null)
                    {
                        groundRb.AddForceAtPosition(-_segmentMaxForwardForce[i] * moveDir, part.position);
                    }
                    else
                    {
                        wormPartRigidbody.AddForce(_segmentMaxForwardForce[i] * moveDir);
                    }
                }
            }
        }
    }
    
    private void MoveMiddleSegmentUp(Transform middlePart, int middleIndex)
    {
        Rigidbody middlePartRigidbody = middlePart.GetComponent<Rigidbody>();
        if (!(middlePartRigidbody.linearVelocity.magnitude > gameObject.GetComponent<Player.Player>().MaxVelocity))
        {
            float currentHeight = middlePart.position.y;
            float maxMiddleHeight = GameParameters.WormMiddleMaxHeight;
            if (currentHeight < maxMiddleHeight)
            {
                float heightDiff = maxMiddleHeight - currentHeight;
                float upwardForce = Mathf.Clamp(heightDiff * GameParameters.WormScrunchForceMultiplier, 0f, GameParameters.WormScrunchForce);
                middlePartRigidbody.AddForce(Vector3.up * upwardForce);
                middlePart.gameObject.GetComponent<CreatureBodySegment>().SetIsScrunched();
            }
        }
    }
    
    private void MoveFrontPartsForward(List<Transform> wormParts, int middleIndex)
    {
        for (int i = 0; i < middleIndex; i++)
        {
            Transform part = wormParts[i];
            Rigidbody wormPartRigidbody = part.GetComponent<Rigidbody>();
            CreaturePart creaturePart = part.GetComponent<CreaturePart>();
            if ((creaturePart.IsGrounded || creaturePart.TimeSinceLastGrounded < GameParameters.maxTimeSinceLastGrounded) && !(wormPartRigidbody.linearVelocity.magnitude > gameObject.GetComponent<Player.Player>().MaxVelocity))
            {
                // Get the part in front (closer to head)
                Transform targetPart = (i > 0) ? wormParts[i - 1] : _wormHead;
                Vector3 directionToTarget = (targetPart.position - part.position).normalized;
                
                //step movement
                if (DetectStep(part.position, targetPart.forward, part.GetComponent<Collider>(), out float stepHeight))
                {
                    float climbForce = GameParameters.WormStepClimbForce * (stepHeight / GameParameters.MaxStepHeight);
                    wormPartRigidbody.AddForce(Vector3.up * climbForce);
                    wormPartRigidbody.AddForce(targetPart.forward * climbForce);
                }
                
                // Project direction onto ground plane
                Vector3 moveDir = AlignToSlope(directionToTarget, creaturePart.GroundNormal);
                GameObject groundObject = creaturePart.GroundObject;
                
                if (groundObject != null)
                {
                    Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
                    if (groundRb != null)
                    {
                        groundRb.AddForceAtPosition(-_segmentMaxForwardForce[i] * moveDir, part.position);
                    }
                    else
                    {
                        wormPartRigidbody.AddForce(_segmentMaxForwardForce[i] * moveDir);
                    }
                }
            }
        }
    }

    private float TryToConstrainWormAngle(Transform wormPart, Transform previousPart, Vector3 previousPosition)
    {
        //calculating angle
        Vector3 partToPreviousPartVector = (wormPart.position - previousPosition).normalized;
        Vector3 backVector = (-previousPart.forward).normalized;
        Vector3 axis = previousPart.up;
        float signedAngle = Vector3.SignedAngle(partToPreviousPartVector, backVector, axis);

        if (Mathf.Abs(signedAngle) > GameParameters.MaxWormTurnAngle)
        {
            float forceMagnitude = ConstrainWormAngle(wormPart, signedAngle, partToPreviousPartVector, backVector);
            return forceMagnitude;
        }
        else
        {
            return 0f;
        }
    }

    private float ConstrainWormAngle(Transform wormPart, float signedAngle, Vector3 partToPreviousPartVector, Vector3 backVector)
    {
        Rigidbody partRigigBody = wormPart.gameObject.GetComponent<Rigidbody>();
        
        float excessAngle = Mathf.Abs(signedAngle) - GameParameters.MaxWormTurnAngle;
        float t = Mathf.Clamp01(excessAngle / 90f);
        float baseForceMagnitude = t * t * GameParameters.WormMoveForce;
            
        Vector3 correctionDir = Vector3.RotateTowards(partToPreviousPartVector, backVector, Mathf.Deg2Rad * excessAngle, 0f).normalized;
        float velocityInCorrectionDir = Vector3.Dot(partRigigBody.linearVelocity, correctionDir);
        
        float forceMagnitude = Mathf.Clamp(baseForceMagnitude - (velocityInCorrectionDir), 0f, (GameParameters.WormMoveForce * GameParameters.WormCorrectionForceMultiplier));
        Vector3 correctionForce = correctionDir * (forceMagnitude);
        
        wormPart.GetComponent<Rigidbody>().AddForce(correctionForce);

        return forceMagnitude;
    }
    
    private (int largestStartIndex, int largestCount) GetGroundedMiddleSegment(List<Transform> wormParts)
    {
        int largestStartIndex = -1;
        int largestCount = 0;
        int currentStartIndex = -1;
        int currentCount = 0;
        
        for (int i = 0; i < wormParts.Count; i++)
        {
            var part = wormParts[i].GetComponent<CreaturePart>();
             
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
        
        return (largestStartIndex, largestCount);
    }

    private void RotateHeadGrounded(float speed)
    {
        Vector3 targetDir = Flatten(_camera.transform.forward);
        Quaternion targetRot = Quaternion.LookRotation(targetDir);
        _wormHead.rotation = Quaternion.Slerp(_wormHead.rotation, targetRot, speed * Time.fixedDeltaTime);
    }

    private void RotateHeadUngrounded(float speed)
    {
        Vector3 camDir = _camera.transform.forward.normalized;
        Vector3 camDirFlat = Flatten(camDir);
        Vector3 wormDirFlat = Flatten(_wormHead.forward);

        Quaternion yawRot = Quaternion.Slerp(
            Quaternion.LookRotation(wormDirFlat),
            Quaternion.LookRotation(camDirFlat),
            speed * Time.fixedDeltaTime
        );

        if (_wormHead.GetComponent<CreaturePart>().TimeSinceLastGrounded > GameParameters.maxTimeSinceLastGrounded)
        {
            float pitch = CalculatePitch(camDir);
            ApplyYawPitch(yawRot, pitch);
        }
    }

    private float CalculatePitch(Vector3 camDir)
    {
        float camPitch = Vector3.SignedAngle(
            Vector3.ProjectOnPlane(camDir, Vector3.up),
            camDir,
            _camera.transform.right
        );

        float normalized = Mathf.InverseLerp(GameParameters.minCameraPitch, GameParameters.maxCameraPitch, camPitch);
        normalized = 1f - Mathf.Clamp01(normalized);

        return Mathf.Lerp(-85f, 85f, normalized);
    }

    private void ApplyYawPitch(Quaternion yawRot, float targetPitch)
    {
        float currentPitch = Mathf.Asin(Mathf.Clamp(_wormHead.forward.y, -1f, 1f)) * Mathf.Rad2Deg;
        float speedFactor = 1f + _wormHeadRb.linearVelocity.magnitude / GameParameters.WormMoveForce;
        float rotSpeed = GameParameters.WormHeadVerticalRotationSpeed * speedFactor;
        float newPitch = Mathf.LerpAngle(currentPitch, targetPitch, rotSpeed * Time.fixedDeltaTime);

        Quaternion pitchRot = Quaternion.AngleAxis(-newPitch, yawRot * Vector3.right);
        _wormHead.rotation = pitchRot * yawRot;
    }

    private void MoveHeadGrounded(CreaturePart part)
    {
        if (_wormHeadRb.linearVelocity.magnitude > gameObject.GetComponent<Player.Player>().MaxVelocity)
            return;

        var groundRb = part.GroundObject?.GetComponent<Rigidbody>();
        HeadTryClimbStep(part);

        Vector3 moveDir = AlignToSlope(_wormHead.forward, part.GroundNormal);

        if (groundRb)
            groundRb.AddForceAtPosition(-GameParameters.WormMoveForce * moveDir, _wormHead.position);
        else
            _wormHeadRb.AddForce(GameParameters.WormMoveForce * moveDir);
    }

    private void MoveHeadUngrounded()
    {
        _wormHeadRb.AddForce(GameParameters.WormMoveForce * _wormHead.forward);
    }

    private void HeadTryClimbStep(CreaturePart part)
    {
        if (DetectStep(_wormHead.position, _wormHead.forward, part.GetComponent<Collider>(), out float stepHeight))
        {
            float climbForce = GameParameters.WormStepClimbForce * (stepHeight / GameParameters.MaxStepHeight);
            _wormHeadRb.AddForce(Vector3.up * climbForce);
            _wormHeadRb.AddForce(_wormHead.forward * climbForce);
        }
    }

    private bool DetectStep(Vector3 position, Vector3 forward, Collider col, out float height)
    {
        height = 0f;
        Vector3 dir = Flatten(forward);
        if (dir.magnitude < 0.1f) return false;

        var bounds = col.bounds;
        
        Vector3 origin = position + dir * (bounds.extents.x * 1.2f) - Vector3.up * (bounds.extents.y * 0.5f);
        int hits = Physics.RaycastNonAlloc(origin, dir, _stepHits, GameParameters.StepDetectionDistance);

        for (int i = 0; i < hits; i++)
        {
            var hit = _stepHits[i];
            if (hit.collider.transform.root == transform.root) continue;

            Vector3 topOrigin = origin + dir * GameParameters.StepDetectionDistance + Vector3.up * GameParameters.MaxStepHeight;
            if (!Physics.Raycast(topOrigin, Vector3.down, out RaycastHit topHit, GameParameters.MaxStepHeight * 1.5f)) continue;
            if (topHit.collider.transform.root == transform.root) continue;

            height = topHit.point.y - (position.y - col.bounds.extents.y);
            if (height > 0.05f && height <= GameParameters.MaxStepHeight) return true;
        }

        return false;
    }

    private static Vector3 Flatten(Vector3 v)
    {
        v.y = 0f;
        return v.normalized;
    }

    private Vector3 AlignToSlope(Vector3 forward, Vector3 normal)
    {
        Vector3 slopeDir = Vector3.ProjectOnPlane(forward, normal).normalized;
        float slopeAngle = Vector3.Angle(Vector3.up, normal);

        if (slopeAngle > GameParameters.MaxSlopeAngle)
        {
            float blend = Mathf.InverseLerp(GameParameters.MaxSlopeAngle, 90f, slopeAngle);
            slopeDir = Vector3.Lerp(slopeDir, forward, blend).normalized;
        }

        return slopeDir;
    }
}
