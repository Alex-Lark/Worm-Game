using Player;
using UnityEngine;

namespace CreatureParts
{
    public class AttachablePart : CreaturePart
    {
        public Vector3 defaultConnectedAnchor;
        
        private Rigidbody attachedSegmentRigidbody;
        private Transform attachedEndPoint;
        private Vector3 localPositionOnAttach;
        private Quaternion localRotationOnAttach;
        
        public void ResetJoint()
        {
            HingeJoint existing = GetComponent<HingeJoint>();
            if (existing != null)
                Destroy(existing);
            
            transform.position = attachedSegmentRigidbody.transform.TransformPoint(localPositionOnAttach);
            transform.rotation = attachedSegmentRigidbody.transform.rotation * localRotationOnAttach;

            ConfigureHingeJoint(attachedSegmentRigidbody, attachedEndPoint);
            IgnorePartCollisionWithWorm(gameObject, attachedSegmentRigidbody.transform);
        }
        
        protected override void OnDespawned()
        {
            Debug.LogError($"[AttachablePart] {gameObject.name} is being despawned!\n{System.Environment.StackTrace}");
            base.OnDespawned();
        }
    
        protected override void OnDestroy()
        {
            Debug.LogError($"[AttachablePart] {gameObject.name} is being DESTROYED!\n{System.Environment.StackTrace}");
            base.OnDestroy();
        }
        
        private void OnDisable()
        {
            Debug.LogWarning($"[AttachablePart] {gameObject.name} disabled. Exists: {gameObject != null}, Scene: {gameObject.scene.name}\n{System.Environment.StackTrace}");
        }
        
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
            attachedSegmentRigidbody = segmentRigidbody;
            attachedEndPoint = endPoint;
            
            localPositionOnAttach = segmentRigidbody.transform.InverseTransformPoint(transform.position);
            localRotationOnAttach = Quaternion.Inverse(segmentRigidbody.transform.rotation) * transform.rotation;
            
            HingeJoint hinge = gameObject.AddComponent<HingeJoint>();
            hinge.connectedBody = segmentRigidbody;
        
            hinge.anchor = gameObject.transform.InverseTransformPoint(endPoint.position);
            
            JointLimits limits = hinge.limits;
            limits.min = -10f;
            limits.max = 10f;
            hinge.limits = limits;
            hinge.useLimits = true;
            
            hinge.enablePreprocessing = true;
            hinge.enableCollision = false;
            
            defaultConnectedAnchor = hinge.connectedAnchor;
        }

        public void IgnorePartCollisionWithWorm(GameObject part, Transform nearestWormSegment)
        {
            int numSegments = GameParameters.NumSegmentCollisionsIgnored;
            
            Collider[] partColliders = part.GetComponentsInChildren<Collider>();
            
            IgnoreCollisionsInDirection(partColliders, nearestWormSegment, true, numSegments);
            IgnoreCollisionsInDirection(partColliders, nearestWormSegment, false, numSegments);
            
            foreach (var attachedWormPart in LocalPlayer.Instance.attachedWormParts)
            {
                Physics.IgnoreCollision(part.GetComponent<Collider>(), attachedWormPart.GetComponent<Collider>(), true);
            }
        }
        
        private void IgnoreCollisionsInDirection(Collider[] partColliders, Transform startSegment, bool forward, int numSegments)
        {
            Transform current = startSegment;

            for (int i = 0; i < numSegments && current != null; i++)
            { 
                Collider[] segmentColliders = current.GetComponentsInChildren<Collider>();
                
                foreach (var pCol in partColliders)
                {
                    foreach (var sCol in segmentColliders)
                    {
                        Physics.IgnoreCollision(pCol, sCol, true);
                    }
                }
                
                if (forward)
                {
                    current = current.childCount > 0 ? current.GetChild(0) : null;
                }
                else
                {
                    current = current.parent;
                }
            }
        }
    }
}
