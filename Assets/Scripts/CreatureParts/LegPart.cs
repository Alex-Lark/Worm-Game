using System.Collections;
using UnityEngine;

namespace CreatureParts
{
    public class LegPart : WormPart
    {
        public GameObject foot;

        private bool isMoving;
        private Coroutine movementCoroutine;

        [Tooltip("Speed at which the leg rotates to align with the ground")]
        public float rotationSpeed = 10f;

        [Tooltip("Speed at which the foot rotates to align with the ground")]
        public float footRotationSpeed = 15f;

        void FixedUpdate()
        {
            base.FixedUpdate();
            isMoving = false;

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

            if (!IsGrounded)
            {
                transform.localScale = new Vector3(transform.localScale.y, 0.2f, transform.localScale.z); //shrinks it's height
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.y, 0.3f, transform.localScale.z); //grows it's height
            }
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
                    Vector3 moveDir = (transform.up * 0.99f + transform.forward * 0.5f).normalized;
                    
                    Rigidbody rb = GroundObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForceAtPosition(
                            -GameParameters.legMoveForce * (moveDir),
                            transform.position
                        );
                    }
                    else
                    {
                        GetComponent<Rigidbody>().AddForce(
                            -GameParameters.legMoveForce * (moveDir)
                        );
                    }
                }

                transform.localScale = new Vector3(transform.localScale.y, 0.2f, transform.localScale.z); //shrinks it's height
                
                movementCoroutine = StartCoroutine(MoveForwardTimer());
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
                Vector3 jumpDirection =
                    Vector3.Slerp(transform.up, Vector3.up, GameParameters.WormJumpAngle).normalized;

                GetComponent<Rigidbody>().AddForce(
                    jumpDirection * GameParameters.legJumpForce
                );
            }
        }
    }
}
