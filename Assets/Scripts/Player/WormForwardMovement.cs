using System.Collections.Generic;
using UnityEngine.SceneManagement;
using CreatureParts;
using UnityEngine;

namespace Player
{
    public class WormForwardMovement : MonoBehaviour
    {
        public float finCount = 0;
        
        #region Private Variables
        
        private Player player;
        private new GameObject camera;
        private Transform wormHead;
        private Rigidbody wormHeadRb;

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
            wormHeadRb = wormHead.GetComponent<Rigidbody>();
            
            segmentMaxForwardForce.Clear();
            for (int i = 0; i < GameParameters.WormSegmentCount; i++)
            {
                segmentMaxForwardForce.Add(0);
            }
        }

        public void MoveHead()
        {
            float speedFactor = 1f + wormHeadRb.linearVelocity.magnitude / GameParameters.WormMoveForce;
            float rotationSpeed = GameParameters.WormHeadRotationSpeed * speedFactor;
            if (finCount >= 1)
            {
                float finMultiplier = 2 * finCount;
                rotationSpeed = rotationSpeed * finMultiplier;
            }

            RotateHeadGrounded(rotationSpeed);
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
        
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RecalculateFinCount();
        }

        private void RecalculateFinCount()
        {
            finCount = GetComponentsInChildren<FinPart>(true).Length;
            Debug.Log("Recalculated finCount = " + finCount);
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
            Rigidbody rb = middlePart.GetComponent<Rigidbody>();
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
                rb.AddForce(Vector3.up * upwardForce);
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
            Rigidbody rb = part.GetComponent<Rigidbody>();
            return (creaturePart.IsGrounded || creaturePart.TimeSinceLastGrounded < GameParameters.MaxTimeSinceLastGrounded) 
                   && rb.linearVelocity.magnitude <= player.MaxVelocity;
        }

        private void ApplyStepClimb(Transform part, Vector3 forward)
        {
            if (DetectStep(part.position, forward, part.GetComponent<Collider>(), out float stepHeight))
            {
                Rigidbody rb = part.GetComponent<Rigidbody>();
                float climbForce = GameParameters.WormStepClimbForce * (stepHeight / GameParameters.MaxStepHeight);
                rb.AddForce(Vector3.up * climbForce);
                rb.AddForce(forward * climbForce);
            }
        }

        private void ApplyMovementForce(Transform part, Vector3 direction, CreaturePart creaturePart, int segmentIndex)
        {
            Vector3 moveDir = AlignToSlope(direction, creaturePart.GroundNormal);
            GameObject groundObject = creaturePart.GroundObject;
            
            if (groundObject == null) return;

            Rigidbody partRb = part.GetComponent<Rigidbody>();
            Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
            
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
            Rigidbody partRb = wormPart.GetComponent<Rigidbody>();
        
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
            
            partRb.AddForce(correctionDir * forceMagnitude);
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
        
        private void RotateHeadGrounded(float speed)
        {
            Vector3 targetDir = Flatten(camera.transform.forward);
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            wormHead.rotation = Quaternion.Slerp(wormHead.rotation, targetRot, speed * Time.fixedDeltaTime);
        }

        private void MoveHeadGrounded(CreaturePart part)
        {
            if (wormHeadRb.linearVelocity.magnitude > player.MaxVelocity) return;

            ApplyStepClimb(wormHead, wormHead.forward);

            Vector3 moveDir = AlignToSlope(wormHead.forward, part.GroundNormal);
            Rigidbody groundRb = part.GroundObject?.GetComponent<Rigidbody>();

            if (groundRb)
                groundRb.AddForceAtPosition(-GameParameters.WormMoveForce * moveDir, wormHead.position);
            else
                wormHeadRb.AddForce(GameParameters.WormMoveForce * moveDir);
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