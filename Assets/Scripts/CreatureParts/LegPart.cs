using System.Collections;
using UnityEngine;

namespace CreatureParts
{
    public class LegPart : WormPart
    {
        private bool isMoving;
        private Coroutine movementCoroutine;
        
        void FixedUpdate()
        {
            base.FixedUpdate();
            isMoving = false;
        }
    
        public override void MoveForward()
        {
            print("moveForward called");
            if (IsGrounded)
            {
                isMoving = true;

                if (movementCoroutine == null)
                {
                    if (GroundObject != null)
                    {
                        print("leg part moving forward");
                        Rigidbody groundRb = GroundObject.GetComponent<Rigidbody>();
                        if (groundRb != null)
                        {
                            groundRb.AddForceAtPosition(-GameParameters.legMoveForce * (transform.forward + transform.up), transform.position);
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
            Vector3 jumpDirection = Vector3.Slerp(transform.up, Vector3.up, GameParameters.WormJumpAngle).normalized;
            gameObject.GetComponent<Rigidbody>().AddForce(jumpDirection * GameParameters.legJumpForce);
        }
    }
}
