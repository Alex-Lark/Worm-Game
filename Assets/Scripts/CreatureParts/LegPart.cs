using System.Collections;
using UnityEngine;

namespace CreatureParts
{
    public class LegPart : WormPart
    {
        public GameObject foot;

        private bool isMoving;
        private Coroutine movementCoroutine;
        
        [Header("Leg Walking Behavior")]
        public float liftHeight = 0.15f;
        public float swingSpeed = 8f;
        public float forwardSwingDistance = 0.12f;

        [Tooltip("Speed at which the leg rotates to align with the ground")]
        public float rotationSpeed = 10f;

        [Tooltip("Speed at which the foot rotates to align with the ground")]
        public float footRotationSpeed = 15f;

        void FixedUpdate()
        {
            base.FixedUpdate();

            if (IsGrounded && GroundObject != null)
            {
                Vector3 groundNormal = GetGroundNormal();

                // --- LEG ROTATION ---
                Quaternion legTargetRotation =
                    Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    legTargetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                );

                // --- FOOT ROTATION ---
                if (foot != null)
                {
                    Quaternion footTargetRotation =
                        Quaternion.FromToRotation(foot.transform.up, groundNormal) * foot.transform.rotation;

                    foot.transform.rotation = Quaternion.Slerp(
                        foot.transform.rotation,
                        footTargetRotation,
                        footRotationSpeed * Time.fixedDeltaTime
                    );
                }
            }

            if (isMoving && !IsGrounded)
            {
                // Phase 2: LIFT – lift the leg upward slightly
                Vector3 lifted = transform.localPosition + Vector3.up * liftHeight;

                // Phase 3: SWING – move the leg slightly forward relative to worm direction
                Vector3 swingTarget =
                    lifted +
                    Player.Player.Instance.wormVisualHead.forward.normalized * forwardSwingDistance;

                // Smooth transition
                transform.localPosition = Vector3.Lerp(
                    transform.localPosition,
                    swingTarget,
                    swingSpeed * Time.fixedDeltaTime
                );

                // “air stride” visual
                transform.localScale = new Vector3(transform.localScale.x, 0.2f, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.y, 0.3f, transform.localScale.z); //grows it's height
            }
            
            if (IsGrounded && foot != null)
            {
                foot.transform.localRotation = Quaternion.Slerp(
                    foot.transform.localRotation,
                    Quaternion.identity,
                    10f * Time.fixedDeltaTime
                );
            }
            isMoving = false;
        }

        private Vector3 GetGroundNormal()
        {
            Vector3 groundNormal = Vector3.up;

            Collider groundCollider = GroundObject.GetComponent<Collider>();
            if (groundCollider != null)
            {
                Ray ray = new Ray(transform.position, -transform.up);
                if (groundCollider.Raycast(ray, out RaycastHit hit, 1f))
                {
                    groundNormal = hit.normal;
                }
            }
            return groundNormal;
        }

        public override void MoveForward()
        {
            if (!IsGrounded) return;

            isMoving = true;

            if (movementCoroutine == null)
            {
                if (GroundObject != null)
                {
                    // 1. Get the ground normal
                    Vector3 groundNormal = GetGroundNormal();

// 2. Compute the tangent direction (perpendicular to the ground normal)
                    Vector3 groundTangent = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;

// 3. Leg forward
                    Vector3 legForward = transform.forward;

// 4. Worm head forward
                    Vector3 headForward = Player.Player.Instance.wormVisualHead.forward;

// --- COMBINE THEM WITH WEIGHTS ---
                    float groundTangentWeight = 0.8f;
                    float legForwardWeight    = 0.2f;
                    float headForwardWeight   = 0.1f;

// Weighted sum
                    Vector3 moveDirection =
                        groundTangent * groundTangentWeight +
                        legForward    * legForwardWeight +
                        headForward   * headForwardWeight;

// Normalize so the force stays constant
                    moveDirection = moveDirection.normalized;
                    
                    // --- ADD THE STEP ARC HERE ---
                    Vector3 stepUp = transform.up * 0.25f;
                    Vector3 arcDirection = (moveDirection + stepUp).normalized;
                    
                    Rigidbody rb = GroundObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForceAtPosition(
                            -GameParameters.legMoveForce * (-arcDirection),
                            transform.position
                        );
                    }
                    else
                    {
                        GetComponent<Rigidbody>().AddForce(
                            -GameParameters.legMoveForce * (-arcDirection)
                        );
                    }
                    movementCoroutine = StartCoroutine(StepRoutine(arcDirection));
                }
                transform.localScale = new Vector3(transform.localScale.y, 0.2f, transform.localScale.z); //shrinks it's height
            }
        }

        private IEnumerator StepRoutine(Vector3 moveDirection)
        {
            Rigidbody rb = GetComponent<Rigidbody>();

            // --- PHASE 1: LIFT / UNWEIGHT ---
            float liftTime = 0.08f;
            float liftForce = GameParameters.legMoveForce * 0.03f;

            for (float t = 0; t < liftTime; t += Time.fixedDeltaTime)
            {
                rb.AddForce(transform.up * liftForce);
                yield return new WaitForFixedUpdate();
            }

            // --- PHASE 2: FORWARD PUSH ---
            float pushTime = GameParameters.legMoveTime;
            float pushForce = GameParameters.legMoveForce * 0.03f;

            for (float t = 0; t < pushTime; t += Time.fixedDeltaTime)
            {
                rb.AddForce(moveDirection * pushForce);
                yield return new WaitForFixedUpdate();
            }

            movementCoroutine = null;
        }

        public override void Jump()
        {
            if (IsGrounded)
            {
                Vector3 jumpDirection =
                    Vector3.Slerp(transform.up, Vector3.up, GameParameters.WormJumpAngle).normalized;

                GetComponent<Rigidbody>().AddForce(
                    jumpDirection * GameParameters.legJumpForce
                );
            }
        }
    }
}
