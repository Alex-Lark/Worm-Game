using System.Collections.Generic;
using UnityEngine;

public class WormJump : MonoBehaviour
{
    private Transform _wormHead;
    private List<Transform> _wormParts;
    
    void Start()
    {
        _wormHead = Player.Instance.wormHead;
        _wormParts = Player.Instance.wormParts;
    }

    public void Jump()
    {
        List<List<int>> consecutiveSegments = GetConsecutiveSegments();

        List<List<int>> jumpSegments = GetLargestConsecutiveSegments(consecutiveSegments, GameParameters.WormJumpSegments);
        
        //TODO: find middle wormPart per jump segment
        findMiddleJumpSegments();


        // if (_wormHead.GetComponent<WormPart>().IsGrounded)
        // {
        //     GameObject groundObject = _wormHead.GetComponent<WormPart>().GroundObject;
        //     if (groundObject != null)
        //     {
        //         Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
        //         if (groundRb != null)
        //         {
        //             Vector3 forceToApply = -GameParameters.WormJumpForce * _wormHead.up;
        //
        //             groundRb.AddForceAtPosition(forceToApply, _wormHead.position);
        //         }
        //         else
        //         {
        //             _wormHead.GetComponent<Rigidbody>().AddForce(GameParameters.WormJumpForce * _wormHead.up);
        //         }
        //     }
        // }

        //     for (int i = 0; i < _wormParts.Count; i++)
        //     {
        //         if (_wormParts[i].GetComponent<WormPart>().IsGrounded || _wormParts[i].GetComponent<WormBodySegment>().IsScrunched)
        //         {
        //             GameObject groundObject = _wormParts[i].GetComponent<WormPart>().GroundObject;
        //             if (groundObject != null)
        //             {
        //                 Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
        //                 if (groundRb != null)
        //                 {
        //                     groundRb.AddForceAtPosition(-GameParameters.WormJumpForce * _wormHead.up, _wormParts[i].position);
        //                 }
        //                 else
        //                 {
        //                     _wormParts[i].GetComponent<Rigidbody>().AddForce(GameParameters.WormJumpForce * _wormHead.up);
        //                 }
        //             }
        //         }
        //     }
    }

    private void findMiddleJumpSegments()
    {
        throw new System.NotImplementedException();
    }

    private List<List<int>> GetConsecutiveSegments()
    {
        List<List<int>> segments = new List<List<int>>();
        List<int> currentSegment = null;
        
        if (_wormHead.GetComponent<WormPart>().IsGrounded) {
            currentSegment = new List<int> { -1 }; // Use -1 to represent head
        }
        
        for (int i = 0; i < _wormParts.Count; i++) {
            WormPart part = _wormParts[i].GetComponent<WormPart>();
            WormBodySegment bodySeg = _wormParts[i].GetComponent<WormBodySegment>();
            bool isGroundedOrScrunched = part.IsGrounded || (bodySeg != null && bodySeg.IsScrunched);
                
            if (isGroundedOrScrunched) {
                if (currentSegment == null) {
                    currentSegment = new List<int>();
                }
                currentSegment.Add(i);
            } else {
                if (currentSegment != null && currentSegment.Count > 0) {
                    segments.Add(currentSegment);
                    currentSegment = null;
                }
            }
        }
        
        if (currentSegment != null && currentSegment.Count > 0) {
            segments.Add(currentSegment);
        }

        return segments;
    }
    
    private List<List<int>> GetLargestConsecutiveSegments(List<List<int>> consecutiveSegments, int numSegments)
    {
        if (consecutiveSegments.Count == 0) return new List<List<int>>();
        
        consecutiveSegments.Sort((a, b) => b.Count.CompareTo(a.Count));
        
        List<List<int>> jumpSegments = new List<List<int>>();
        
        // Threshold for considering a segment "significantly larger"
        // A segment is significantly larger if it's more than 2x the size of another
        float significantlyLargerThreshold = GameParameters.JumpingSegmentDivisionThreshold;
        
        int segmentsAdded = 0;
        int segmentIndex = 0;
        
        while (segmentsAdded < numSegments && segmentIndex < consecutiveSegments.Count) {
            List<int> currentSeg = consecutiveSegments[segmentIndex];
            
            // Check if we should split this segment
            bool shouldSplit = false;
            
            if (currentSeg.Count > 1 && segmentsAdded < numSegments - 1) {
                // Check if splitting would be better than taking the next segment
                int halfSize = currentSeg.Count / 2;
                
                if (segmentIndex + 1 < consecutiveSegments.Count) {
                    // There's a next segment available
                    int nextSegSize = consecutiveSegments[segmentIndex + 1].Count;
                    
                    // Split if the current segment is significantly larger AND
                    // splitting it would still result in halves larger than the next segment
                    if (currentSeg.Count > nextSegSize * significantlyLargerThreshold && 
                        halfSize >= nextSegSize) {
                        shouldSplit = true;
                    }
                } else {
                    // No more segments available, split if we need more segments
                    shouldSplit = true;
                }
            }
            
            if (shouldSplit) {
                // Split the segment
                int midpoint = currentSeg.Count / 2;
                jumpSegments.Add(currentSeg.GetRange(0, midpoint));
                segmentsAdded++;
                
                if (segmentsAdded < numSegments) {
                    jumpSegments.Add(currentSeg.GetRange(midpoint, currentSeg.Count - midpoint));
                    segmentsAdded++;
                }
                
                segmentIndex++;
            } else {
                // Take the segment as-is
                jumpSegments.Add(currentSeg);
                segmentsAdded++;
                segmentIndex++;
            }
        }
        
        return jumpSegments;
    }
}
