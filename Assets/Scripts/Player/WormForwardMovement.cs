using System.Collections.Generic;
using CreatureParts;
using GameLoop.multiplayer;
using PurrNet;
using UnityEngine;
using UnityEngine.UIElements;

namespace Player
{
    public class WormForwardMovement : MonoBehaviour
    {
        #region Private Variables
        
        private Player player;
        private new GameObject camera;
        private Transform wormHead;
        private NetworkedPhysicsObject wormHeadNetworkedPhysicsObject;
        private NetworkRigidbody wormHeadNetworkRigidbody;

        private readonly RaycastHit[] stepHits = new RaycastHit[10];
        private readonly List<float> segmentMaxForwardForce = new List<float>();
        private float movementPhase;
        
        #endregion

        #region Built-In Methods
        
        void Start()
        {
            SetVariables();
        }
        
        #endregion
        
        #region Public Methods
        
        public void SetVariables()
        {
            player = GetComponent<Player>();
            camera = player.thirdPersonCamera;
            wormHead = player.wormHead;
            wormHeadNetworkedPhysicsObject = wormHead.GetComponent<NetworkedPhysicsObject>();
            wormHeadNetworkRigidbody = wormHead.GetComponent<NetworkRigidbody>();
            
            segmentMaxForwardForce.Clear();
            for (int i = 0; i < GameParameters.WormSegmentCount; i++)
            {
                segmentMaxForwardForce.Add(0);
            }
        }

        public void MoveHead()
        {
            float speedFactor = 1f + wormHeadNetworkRigidbody.linearVelocity.magnitude / GameParameters.WormMoveForce;
            float rotationSpeed = GameParameters.WormHeadRotationSpeed * speedFactor;
            Vector3 direction = player.thirdPersonCamera.transform.forward;
            
            RotateHeadGrounded(rotationSpeed, direction);
            MoveHeadGrounded(wormHead.GetComponent<CreaturePart>());
        }
    
        public void MoveWormBody()
        {
            List<Transform> wormParts = player.wormBodySegments;

            // Calculate constraint forces for all segments
            for (int i = 0; i < wormParts.Count; i++)
            {
                segmentMaxForwardForce[i] = GameParameters.WormMoveForce - 
                    TryToConstrainWormAngle(wormParts[i], wormHead.transform, wormHead.position);
            }

            (int startIndex, int count) = GetGroundedMiddleSegment(wormParts);
            if (startIndex == -1) return;

            int middleIndex = startIndex + (count / 2);
            UpdateMovementPhase();

            if (movementPhase < 0.33f)
                MoveBackPartsForward(wormParts, middleIndex);
            else if (movementPhase < 0.66f)
                MoveMiddleSegmentUp(wormParts[middleIndex], middleIndex);
            else
                MoveFrontPartsForward(wormParts, middleIndex);
        }
        
        #endregion
        
        #region Private Methods - Movement Phases
        
        private void UpdateMovementPhase()
        {
            movementPhase += Time.fixedDeltaTime / GameParameters.WormForwardMovementLoopLength;
            if (movementPhase > 1f) movementPhase = 0f;
        }

        private void MoveBackPartsForward(List<Transform> wormParts, int middleIndex)
        {
            for (int i = middleIndex + 1; i < wormParts.Count; i++)
            {
                Transform part = wormParts[i];
                CreaturePart creaturePart = part.GetComponent<CreaturePart>();
                
                if (!CanMove(part, creaturePart)) continue;

                Transform targetPart = (i > 0) ? wormParts[i - 1] : wormHead;
                Vector3 directionToTarget = (targetPart.position - part.position).normalized;
                
                ApplyStepClimb(part, targetPart.forward);
                ApplyMovementForce(part, directionToTarget, creaturePart, i);
            }
        }
    
        private void MoveMiddleSegmentUp(Transform middlePart, int middleIndex)
        {
            NetworkRigidbody rb = middlePart.GetComponent<NetworkRigidbody>();
            
            if (rb.linearVelocity.magnitude > player.MaxVelocity) return;

            float currentHeight = middlePart.position.y;
            if (currentHeight < GameParameters.WormMiddleMaxHeight)
            {
                float heightDiff = GameParameters.WormMiddleMaxHeight - currentHeight;
                float upwardForce = Mathf.Clamp(
                    heightDiff * GameParameters.WormScrunchForceMultiplier, 
                    0f, 
                    GameParameters.WormScrunchForce
                );
                middlePart.GetComponent<NetworkedPhysicsObject>().AddForce(Vector3.up * upwardForce);
                middlePart.GetComponent<CreatureBodySegment>().SetIsScrunched();
            }
        }
    
        private void MoveFrontPartsForward(List<Transform> wormParts, int middleIndex)
        {
            for (int i = 0; i < middleIndex; i++)
            {
                Transform part = wormParts[i];
                CreaturePart creaturePart = part.GetComponent<CreaturePart>();
                
                if (!CanMove(part, creaturePart)) continue;

                Transform targetPart = (i > 0) ? wormParts[i - 1] : wormHead;
                Vector3 directionToTarget = (targetPart.position - part.position).normalized;
                
                ApplyStepClimb(part, targetPart.forward);
                ApplyMovementForce(part, directionToTarget, creaturePart, i);
            }
        }
        
        #endregion

        #region Private Methods - Movement Helpers
        
        private bool CanMove(Transform part, CreaturePart creaturePart)
        {
            NetworkRigidbody rb = part.GetComponent<NetworkRigidbody>();
            return (creaturePart.IsGrounded || creaturePart.TimeSinceLastGrounded < GameParameters.MaxTimeSinceLastGrounded) 
                   && rb.linearVelocity.magnitude <= player.MaxVelocity;
        }

        private void ApplyStepClimb(Transform part, Vector3 forward)
        {
            if (DetectStep(part.position, forward, part.GetComponent<Collider>(), out float stepHeight))
            {
                NetworkedPhysicsObject networkedPhysicsObject = part.GetComponent<NetworkedPhysicsObject>();
                float climbForce = GameParameters.WormStepClimbForce * (stepHeight / GameParameters.MaxStepHeight);
                networkedPhysicsObject.AddForce(Vector3.up * climbForce);
                networkedPhysicsObject.AddForce(forward * climbForce);
            }
        }

        private void ApplyMovementForce(Transform part, Vector3 direction, CreaturePart creaturePart, int segmentIndex)
        {
            Vector3 moveDir = AlignToSlope(direction, creaturePart.GroundNormal);
            GameObject groundObject = creaturePart.GroundObject;
            
            if (groundObject == null) return;

            NetworkedPhysicsObject partRb = part.GetComponent<NetworkedPhysicsObject>();
            NetworkedPhysicsObject groundRb = groundObject.GetComponent<NetworkedPhysicsObject>();
            
            if (groundRb != null)
                groundRb.AddForceAtPosition(-segmentMaxForwardForce[segmentIndex] * moveDir, part.position);
            else
                partRb.AddForce(segmentMaxForwardForce[segmentIndex] * moveDir);
        }
        
        #endregion

        #region Private Methods - Angle Constraint
        
        private float TryToConstrainWormAngle(Transform wormPart, Transform previousPart, Vector3 previousPosition)
        {
            Vector3 partToPreviousPartVector = (wormPart.position - previousPosition).normalized;
            Vector3 backVector = -previousPart.forward;
            float signedAngle = Vector3.SignedAngle(partToPreviousPartVector, backVector, previousPart.up);

            if (Mathf.Abs(signedAngle) > GameParameters.MaxWormTurnAngle)
            {
                return ConstrainWormAngle(wormPart, signedAngle, partToPreviousPartVector, backVector);
            }
            
            return 0f;
        }

        private float ConstrainWormAngle(Transform wormPart, float signedAngle, Vector3 partToPreviousPartVector, Vector3 backVector)
        {
            NetworkRigidbody partRb = wormPart.GetComponent<NetworkRigidbody>();
        
            float excessAngle = Mathf.Abs(signedAngle) - GameParameters.MaxWormTurnAngle;
            float t = Mathf.Clamp01(excessAngle / 90f);
            float baseForceMagnitude = t * t * GameParameters.WormMoveForce;
            
            Vector3 correctionDir = Vector3.RotateTowards(partToPreviousPartVector, backVector, Mathf.Deg2Rad * excessAngle, 0f).normalized;
            float velocityInCorrectionDir = Vector3.Dot(partRb.linearVelocity, correctionDir);
        
            float forceMagnitude = Mathf.Clamp(
                baseForceMagnitude - velocityInCorrectionDir, 
                0f, 
                GameParameters.WormMoveForce * GameParameters.WormCorrectionForceMultiplier
            );
            
            wormPart.GetComponent<NetworkedPhysicsObject>().AddForce(correctionDir * forceMagnitude);
            return forceMagnitude;
        }
        
        #endregion
    
        #region Private Methods - Grounding Detection
        
        private (int startIndex, int count) GetGroundedMiddleSegment(List<Transform> wormParts)
        {
            int largestStartIndex = -1;
            int largestCount = 0;
            int currentStartIndex = -1;
            int currentCount = 0;
        
            for (int i = 0; i < wormParts.Count; i++)
            {
                if (wormParts[i].GetComponent<CreaturePart>().IsGrounded)
                {
                    if (currentStartIndex == -1) currentStartIndex = i;
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
        
            if (currentCount > largestCount)
            {
                largestCount = currentCount;
                largestStartIndex = currentStartIndex;
            }
        
            return (largestStartIndex, largestCount);
        }
        
        #endregion

        #region Private Methods - Head Movement
        
        private void RotateHeadGrounded(float speed, Vector3 direction)
        {
            Vector3 targetDir = Flatten(direction);
            if (targetDir.magnitude < 0.01f) return;
            
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            Quaternion newRotation = Quaternion.Slerp(wormHead.GetComponent<NetworkRigidbody>().rotation, targetRot, speed * Time.fixedDeltaTime);
            
            wormHead.GetComponent<NetworkRigidbody>().rotation = newRotation;

            // if (player.isOwner)
            // {
            //     Debug.Log("setting head rotation");
            //     player.SetHeadRotation(newRotation);
            // }
        }
        
        // public void Update()
        // {
        //     if (Input.GetKey(KeyCode.R))
        //     {
        //         Quaternion extraRotation = Quaternion.Euler(0, 0, 15f);
        //         wormHead.GetComponent<NetworkRigidbody>().rotation = wormHead.GetComponent<NetworkRigidbody>().rotation * extraRotation;
        //     }
        // }

        private void MoveHeadGrounded(CreaturePart part)
        {
            if (wormHeadNetworkRigidbody.linearVelocity.magnitude > player.MaxVelocity) return;

            ApplyStepClimb(wormHead, wormHead.forward);

            Vector3 moveDir = AlignToSlope(wormHead.forward, part.GroundNormal);
            NetworkedPhysicsObject groundRb = part.GroundObject?.GetComponent<NetworkedPhysicsObject>();

            if (groundRb)
                groundRb.AddForceAtPosition(-GameParameters.WormMoveForce * moveDir, wormHead.position);
            else
                wormHeadNetworkedPhysicsObject.AddForce(GameParameters.WormMoveForce * moveDir);
        }
        
        #endregion

        #region Private Methods - Step Detection
        
        private bool DetectStep(Vector3 position, Vector3 forward, Collider col, out float height)
        {
            height = 0f;
            Vector3 dir = Flatten(forward);
            if (dir.magnitude < 0.1f) return false;

            var bounds = col.bounds;
            Vector3 origin = position + dir * (bounds.extents.x * 1.2f) - Vector3.up * (bounds.extents.y * 0.5f);
            int hits = Physics.RaycastNonAlloc(origin, dir, stepHits, GameParameters.StepDetectionDistance);

            for (int i = 0; i < hits; i++)
            {
                if (stepHits[i].collider.transform.root == transform.root) continue;

                Vector3 topOrigin = origin + dir * GameParameters.StepDetectionDistance + Vector3.up * GameParameters.MaxStepHeight;
                if (!Physics.Raycast(topOrigin, Vector3.down, out RaycastHit topHit, GameParameters.MaxStepHeight * 1.5f)) continue;
                if (topHit.collider.transform.root == transform.root) continue;

                height = topHit.point.y - (position.y - col.bounds.extents.y);
                if (height > 0.05f && height <= GameParameters.MaxStepHeight) return true;
            }

            return false;
        }
        
        #endregion

        #region Private Methods - Utilities
        
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
        
        #endregion
    }
}