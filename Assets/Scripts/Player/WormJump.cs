using System.Collections.Generic;
using CreatureParts;
using UnityEngine;

public class WormJump : MonoBehaviour
{
    private Transform wormHead;
    private List<Transform> wormParts;
    private float jumpChargeTime = 0f;
    
    private List<int> middleIndices = null;
    private List<List<int>> consecutiveSegments = null;
    private Dictionary<int, float> reachedHeights = new Dictionary<int, float>(); // Track which segments reached target
    
    void Start()
    {
        wormHead = Player.Player.Instance.wormHead;
        wormParts = Player.Player.Instance.wormBodySegments;
        middleIndices = null;
    }

    public void StartJump()
    {
        
    }
    
    public void StopJump() 
    {
        jumpChargeTime = 0f;
        reachedHeights.Clear();
    }

    public void Jump() 
    {
        consecutiveSegments = GetConsecutiveSegments();
        List<List<int>> jumpSegments = GetLargestConsecutiveSegments(consecutiveSegments, GameParameters.WormJumpSegments);
        
        middleIndices = FindMiddleJumpSegments(jumpSegments);
        reachedHeights.Clear();
        
        if (middleIndices == null)
        {
            return;
        }
        
        // foreach (int middleIndex in middleIndices)
        // {
        //     _wormParts[middleIndex].GetComponent<Rigidbody>().AddForce(_wormParts[middleIndex].transform.up * GameParameters.WormMiddleSegmentScrunchForce);
        //     _wormParts[middleIndex].GetComponent<WormBodySegment>().SetIsScrunched();
        // }

        Transform previousPart = wormHead;
        
        for (int i = 0; i < wormParts.Count; i++)
        {
            Transform wormPart = wormParts[i];
            if (middleIndices != null && (middleIndices.Contains(i)))
            {
                wormPart.GetComponent<Rigidbody>().AddForce(Vector3.down * GameParameters.WormMiddleSegmentScrunchForce);
            }
            else
            {
                if (wormPart.GetComponent<CreatureBodySegment>().IsGrounded || wormPart.GetComponent<CreatureBodySegment>().IsScrunched || (wormPart.GetComponent<CreatureBodySegment>().TimeSinceLastGrounded < GameParameters.MaxTimeSinceLastGrounded))
                {
                    Vector3 forwardDirection = Vector3.Slerp(previousPart.forward, wormHead.forward, GameParameters.WormJumpPreviousPartVsHeadAngle);
                    Vector3 jumpDirection = Vector3.Slerp(forwardDirection, Vector3.up, GameParameters.WormJumpAngle).normalized;
                    wormPart.GetComponent<Rigidbody>().AddForce(jumpDirection * GameParameters.WormJumpForce);
                }
            }
            previousPart = wormParts[i];
        }
    }

    private List<List<int>> GetConsecutiveSegments()
    {
        List<List<int>> segments = new List<List<int>>();
        List<int> currentSegment = null;
        
        if (wormHead.GetComponent<CreaturePart>().IsGrounded) 
        {
            currentSegment = new List<int> { -1 }; // Use -1 to represent head
        }
        
        for (int i = 0; i < wormParts.Count; i++) 
        {
            CreaturePart part = wormParts[i].GetComponent<CreaturePart>();
            CreatureBodySegment bodySeg = wormParts[i].GetComponent<CreatureBodySegment>();
            bool isGroundedOrScrunched = part.IsGrounded || (bodySeg != null && bodySeg.IsScrunched);
                
            if (isGroundedOrScrunched) 
            {
                if (currentSegment == null) 
                {
                    currentSegment = new List<int>();
                }
                currentSegment.Add(i);
            } 
            else 
            {
                if (currentSegment != null && currentSegment.Count > 0) 
                {
                    segments.Add(currentSegment);
                    currentSegment = null;
                }
            }
        }
        
        if (currentSegment != null && currentSegment.Count > 0) 
        {
            segments.Add(currentSegment);
        }

        return segments;
    }
    
    private List<List<int>> GetLargestConsecutiveSegments(List<List<int>> consecutiveSegments, int numSegments)
    {
        if (consecutiveSegments.Count == 0) return new List<List<int>>();
        
        consecutiveSegments.Sort((a, b) => b.Count.CompareTo(a.Count));
        
        List<List<int>> jumpSegments = new List<List<int>>();
        
        float significantlyLargerThreshold = GameParameters.JumpingSegmentDivisionThreshold;
        
        int segmentsAdded = 0;
        int segmentIndex = 0;
        
        while (segmentsAdded < numSegments && segmentIndex < consecutiveSegments.Count) 
        {
            List<int> currentSeg = consecutiveSegments[segmentIndex];
            
            bool shouldSplit = false;
            
            if (currentSeg.Count > 1 && segmentsAdded < numSegments - 1) 
            {
                int halfSize = currentSeg.Count / 2;
                
                if (segmentIndex + 1 < consecutiveSegments.Count) 
                {
                    int nextSegSize = consecutiveSegments[segmentIndex + 1].Count;
                    
                    if (currentSeg.Count > nextSegSize * significantlyLargerThreshold && 
                        halfSize >= nextSegSize) 
                    {
                        shouldSplit = true;
                    }
                } 
                else 
                {
                    shouldSplit = true;
                }
            }
            
            if (shouldSplit) 
            {
                int midpoint = currentSeg.Count / 2;
                jumpSegments.Add(currentSeg.GetRange(0, midpoint));
                segmentsAdded++;
                
                if (segmentsAdded < numSegments) 
                {
                    jumpSegments.Add(currentSeg.GetRange(midpoint, currentSeg.Count - midpoint));
                    segmentsAdded++;
                }
                
                segmentIndex++;
            } 
            else 
            {
                jumpSegments.Add(currentSeg);
                segmentsAdded++;
                segmentIndex++;
            }
        }
        
        return jumpSegments;
    }
    
    private List<int> FindMiddleJumpSegments(List<List<int>> jumpSegments) 
    {
        List<int> middleIndices = new List<int>();
    
        foreach (List<int> segment in jumpSegments) 
        {
            if (segment.Count > 0) 
            {
                int middleIndex = segment[segment.Count / 2];
                middleIndices.Add(middleIndex);
            }
        }
    
        return middleIndices;
    }
}