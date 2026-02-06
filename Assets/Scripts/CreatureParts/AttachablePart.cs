using UnityEngine;

namespace CreatureParts
{
    public class AttachablePart : CreaturePart
    {
        public void ConfigureRigidBody(Rigidbody partRigidbody, Rigidbody segmentRigidbody, float mass)
        {
            partRigidbody.mass = mass;
            
            partRigidbody.linearDamping = segmentRigidbody.linearDamping;
            partRigidbody.angularDamping = segmentRigidbody.angularDamping;
            partRigidbody.interpolation = segmentRigidbody.interpolation;
            partRigidbody.collisionDetectionMode = segmentRigidbody.collisionDetectionMode;
            
            partRigidbody.linearDamping = 1f;
            partRigidbody.angularDamping = 1f;
        }
        
        public void ConfigureHingeJoint(Rigidbody segmentRigidbody, Transform endPoint)
        {
            //Hinge connects the attachable part to the worm itself and allows some movement
            
            HingeJoint hinge = gameObject.AddComponent<HingeJoint>();
            hinge.connectedBody = segmentRigidbody;
        
            hinge.anchor = gameObject.transform.InverseTransformPoint(endPoint.position);

            //limited rotation
            JointLimits limits = hinge.limits;
            limits.min = -10f;
            limits.max = 10f;
            hinge.limits = limits;
            hinge.useLimits = true;

            // smoothing
            hinge.enablePreprocessing = true;
            hinge.enableCollision = false;
        }
    }
}
