using System;
using System.Collections.Generic;
using CreatureParts;
using PurrNet;
using UnityEngine;

namespace Player
{
    public class WormPhysics : MonoBehaviour
    {
        #region Private Variables
        [Header("Private Variables")]
        
        private List<SphereCollider> segmentColliders = new List<SphereCollider>();
        private Player player;
        
        #endregion
        
        #region Built-In Methods

        public void Awake()
        {
            player = GetComponent<Player>();
        }
        
        #endregion

        #region Public Methods
        
        public void AddCollidersToSegments()
        {
            SphereCollider headCollider = player.wormHead.GetComponent<SphereCollider>();
            if (headCollider == null)
                headCollider = player.wormHead.gameObject.AddComponent<SphereCollider>();
        
            headCollider.radius = GameParameters.WormBodyWidth * 4;
        
            for (int i = 0; i < player.wormBodySegments.Count; i++)
            {
                SphereCollider segmentCollider = player.wormBodySegments[i].GetComponent<SphereCollider>();
                segmentCollider.radius = GameParameters.WormBodyWidth * 4;
            
                if (!segmentColliders.Contains(segmentCollider))
                    segmentColliders.Add(segmentCollider);
            }
        
            IgnoreWormSelfCollision();
        }

        public void ToggleWormKinematics(bool isKinematic)
        {
            if (isKinematic)
            {
                MakeWormKinematic();
            }
            else
            {
                MakeWormUnkinematic();
            }
        }
    
        public void MakeWormKinematic()
        {
            SetSegmentPhysics(player.wormHead, isKinematic: true, useGravity: false);
            foreach (Transform segment in player.wormBodySegments)
            {
                SetSegmentPhysics(segment, isKinematic: true, useGravity: false);
            }

            foreach (Transform part in player.wormBodySegments)
            {
                SetSegmentPhysics(part, isKinematic: true, useGravity: false);
            }
        }

        public void MakeWormUnkinematic()
        {
            Rigidbody headRb = player.wormHead.GetComponent<Rigidbody>();
            if (headRb != null)
            {
                headRb.useGravity = true;
                headRb.isKinematic = false;
                headRb.linearVelocity = Vector3.zero;
                headRb.angularVelocity = Vector3.zero;
            }
            
            foreach (Transform segment in player.wormBodySegments)
            {
                Rigidbody segmentRb = segment.GetComponent<Rigidbody>();
                segmentRb.useGravity = true;
                segmentRb.isKinematic = false;
                segmentRb.linearVelocity = Vector3.zero;
                segmentRb.angularVelocity = Vector3.zero;
                
                segment.GetComponent<CreatureBodySegment>().ResetJointPhysics();
            }
        }

        public void SetSegmentPhysics(Transform segment, bool isKinematic, bool useGravity)
        {
            Rigidbody rb = segment.GetComponent<Rigidbody>();
            if (rb == null) return;
            
            rb.isKinematic = isKinematic;
            rb.useGravity = useGravity;

            if (isKinematic == false)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        public void PositionWormSegments(Vector3 headPosition)
        {
            player.wormHead.position = headPosition;
            Vector3 currentPosition = headPosition;
            Vector3 backDirection = -player.wormHead.forward;

            for (int i = 0; i < player.wormBodySegments.Count; i++)
            {
                currentPosition += backDirection * GameParameters.SegmentMaxPartDistance;
                player.wormBodySegments[i].position = currentPosition;
                player.wormBodySegments[i].rotation = player.wormHead.rotation;
            }
        }
        
        public void IgnoreWormSelfCollision()
        {
            List<Collider> allWormColliders = new List<Collider>();
        
            Collider headCollider = player.wormHead.GetComponent<Collider>();
            if (headCollider != null)
                allWormColliders.Add(headCollider);
        
            foreach (var segment in player.wormBodySegments)
            {
                Collider segmentCollider = segment.GetComponent<Collider>();
                if (segmentCollider != null)
                    allWormColliders.Add(segmentCollider);
            }
        
            int ignoreDistance = GameParameters.NumSegmentCollisionsIgnored;
        
            for (int i = 0; i < allWormColliders.Count; i++)
            {
                for (int j = i + 1; j < allWormColliders.Count; j++)
                {
                    if (Mathf.Abs(i - j) <= ignoreDistance)
                    {
                        Physics.IgnoreCollision(allWormColliders[i], allWormColliders[j], true);
                    }
                }
            }
        }
        
        public void ResetWormPosition()
        {
            if (player.wormHead == null) return;
            
            player.wormHead.position = new Vector3(0, 2, 0);
            Rigidbody headRb = player.wormHead.GetComponent<Rigidbody>();
            if (headRb != null)
            {
                headRb.useGravity = true;
                headRb.isKinematic = false;
                headRb.linearVelocity = Vector3.zero;
                headRb.angularVelocity = Vector3.zero;
            }
            
            Vector3 currentPos = player.wormHead.position;
            Vector3 backDir = -player.wormHead.forward;
            
            for (int i = 0; i < player.wormBodySegments.Count; i++)
            {
                currentPos += backDir * GameParameters.SegmentMaxPartDistance;
                Transform segment = player.wormBodySegments[i];
                segment.position = currentPos;
                segment.rotation = player.wormHead.rotation;
            
                Rigidbody segmentRb = segment.GetComponent<Rigidbody>();
                segmentRb.useGravity = true;
                segmentRb.isKinematic = false;
                segmentRb.linearVelocity = Vector3.zero;
                segmentRb.angularVelocity = Vector3.zero;
            }
        }
        
        public void ResetWormOrientation()
        {
            player.wormVisualHead.rotation = Quaternion.identity;
            player.wormHead.rotation = Quaternion.identity;
            foreach (Transform segment in player.wormBodySegments)
            {
                segment.rotation = Quaternion.identity;
            }
        }
        
        public void IgnorePartCollisionWithWorm(GameObject part, Transform nearestWormSegment)
        {
            Debug.Log("ignoring part collision with worm");
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
        
        #endregion
    }
}