using System.Collections;
using UnityEngine;

namespace CreatureParts
{
    public class LegPart : WormPart
    {
        private bool isMoving;
        private Coroutine movementCoroutine;

        [Tooltip("Speed at which the leg rotates to align with the ground")]
        public float rotationSpeed = 10f;

        void FixedUpdate()
        {
            base.FixedUpdate();
            isMoving = false;

            // Align leg with ground if grounded
            if (IsGrounded && GroundObject != null)
            {
                Rigidbody groundRb = GroundObject.GetComponent<Rigidbody>();
                Vector3 groundNormal = Vector3.up;

                // Try to get normal from the collider if possible
                Collider groundCollider = GroundObject.GetComponent<Collider>();
                if (groundCollider != null)
                {
                    // Raycast down from leg to get ground normal
                    Ray ray = new Ray(transform.position, -transform.up);
                    if (groundCollider.Raycast(ray, out RaycastHit hit, 1f))
                    {
                        groundNormal = hit.normal;
                    }
                }

                // Smoothly rotate leg so its up points along the ground normal
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }

        public override void MoveForward()
        {
            if (IsGrounded)
            {
                isMoving = true;

                if (movementCoroutine == null)
                {
                    if (GroundObject != null)
                    {
                        Rigidbody rb = GroundObject.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.AddForceAtPosition(-GameParameters.legMoveForce * (transform.forward + transform.up), transform.position);
                        }
                        else
                        {
                            gameObject.GetComponent<Rigidbody>().AddForce(-GameParameters.legMoveForce * (transform.forward + transform.up));
                        }
                    }
                    movementCoroutine = StartCoroutine(MoveForwardTimer());
                }
            }
        }

        private IEnumerator MoveForwardTimer()
        {
            yield return new WaitForSeconds(GameParameters.legMoveTime);
            movementCoroutine = null;
        }

        public override void Jump()
        {
            if (IsGrounded)
            {
                Vector3 jumpDirection = Vector3.Slerp(transform.up, Vector3.up, GameParameters.WormJumpAngle).normalized;
                gameObject.GetComponent<Rigidbody>().AddForce(jumpDirection * GameParameters.legJumpForce);
            }
        }
    }
}
