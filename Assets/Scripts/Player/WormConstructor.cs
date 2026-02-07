using System.Collections.Generic;
using CreatureParts;
using UnityEngine;

namespace Player
{
    public class WormConstructor
    {
        private readonly Transform wormHead;
        private readonly List<Transform> wormBodySegments;
        private readonly GameObject wormSegmentPrefab;
        private readonly Transform parentTransform;
        private readonly int wormSegmentCount;
        private readonly float maxPartDistance;

        public WormConstructor(Transform wormHead, List<Transform> wormBodySegments, GameObject wormSegmentPrefab, 
            Transform parentTransform, int segmentCount, float partDistance)
        {
            this.wormHead = wormHead;
            this.wormBodySegments = wormBodySegments;
            this.wormSegmentPrefab = wormSegmentPrefab;
            this.parentTransform = parentTransform;
            this.wormSegmentCount = segmentCount;
            this.maxPartDistance = partDistance;
        }

        public void CreateWormSegments()
        {
            CreaturePart previousSegment = wormHead.GetComponent<CreaturePart>();
    
            for (int i = 0; i < wormSegmentCount; i++)
            {
                GameObject newSegment = Object.Instantiate(wormSegmentPrefab, parentTransform);
                newSegment.GetComponent<CreatureBodySegment>().previousSegment = previousSegment;
                wormBodySegments.Add(newSegment.transform);
                previousSegment = newSegment.GetComponent<CreatureBodySegment>();
            }
            
            for (int i = 0; i < wormBodySegments.Count - 1; i++)
            {
                wormBodySegments[i].GetComponent<CreatureBodySegment>().nextSegment = 
                    wormBodySegments[i + 1].GetComponent<CreatureBodySegment>();
            }
        }

        public void ConstructWorm()
        {
            Vector3 currentPos = wormHead.position;
            Vector3 backDir = -wormHead.forward;
            Rigidbody previousRb = wormHead.GetComponent<Rigidbody>();

            for (int i = 0; i < wormBodySegments.Count; i++)
            {
                currentPos += backDir * maxPartDistance;
                Transform segment = wormBodySegments[i];
                segment.position = currentPos;
                segment.rotation = wormHead.rotation;
                previousRb = segment.GetComponent<CreatureBodySegment>().AddJoint(segment, previousRb);
            }
        }
    }
}