using System;
using System.Collections.Generic;
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

        public void Start()
        {
            player = GetComponent<Player>();
        }
        
        #endregion

        #region Public Methods
        
        public void AddCollidersToSegments()
        {
            SphereCollider headCollider = player.wormHead.GetComponent<SphereCollider>();

            if (headCollider == null)
            {
                headCollider = player.wormHead.GetComponent<SphereCollider>();
            }
        
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
    
        public void ResetWormPhysics()
        {
            SetSegmentPhysics(player.wormHead, isKinematic: true, useGravity: false);
            foreach (Transform segment in player.wormBodySegments)
            {
                SetSegmentPhysics(segment, isKinematic: true, useGravity: false);
            }
        }

        public void SetSegmentPhysics(Transform segment, bool isKinematic, bool useGravity)
        {
            Rigidbody rb = segment.GetComponent<Rigidbody>();
            if (rb == null) return;

            rb.isKinematic = isKinematic;
            rb.useGravity = useGravity;
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
        
        public void ResetPlayerPhysics()
        {
            player.wormHead.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            player.wormHead.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            foreach (Transform segment in player.wormBodySegments)
            {
                Rigidbody rb = segment.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = true;
            }

            foreach (GameObject part in player.attachedWormParts)
            {
                Rigidbody rb = part.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = true;
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
        
        #endregion
    }
}