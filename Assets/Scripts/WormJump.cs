using System.Collections.Generic;
using UnityEngine;

public class WormJump : MonoBehaviour
{
    private Transform _wormHead;
    private List<Transform> _wormParts;
    private float _jumpChargeTime = 0f;
    
    private List<int> middleIndices = null;
    private List<List<int>> consecutiveSegments = null;
    private Dictionary<int, float> _reachedHeights = new Dictionary<int, float>(); // Track which segments reached target
    
    void Start()
    {
        _wormHead = Player.Instance.wormHead;
        _wormParts = Player.Instance.wormParts;
        middleIndices = null;
    }

    public void StartJump()
    {
        
    }
    
    public void StopJump() 
    {
        _jumpChargeTime = 0f;
        _reachedHeights.Clear();
    }

    public void Jump() 
    {
        consecutiveSegments = GetConsecutiveSegments();
        List<List<int>> jumpSegments = GetLargestConsecutiveSegments(consecutiveSegments, GameParameters.WormJumpSegments);
        
        middleIndices = FindMiddleJumpSegments(jumpSegments);
        _reachedHeights.Clear();
        
        if (middleIndices == null)
        {
            return;
        }
        
        // foreach (int middleIndex in middleIndices)
        // {
        //     _wormParts[middleIndex].GetComponent<Rigidbody>().AddForce(_wormParts[middleIndex].transform.up * GameParameters.WormMiddleSegmentScrunchForce);
        //     _wormParts[middleIndex].GetComponent<WormBodySegment>().SetIsScrunched();
        // }

        Transform previousPart = _wormHead;
        
        for (int i = 0; i < _wormParts.Count; i++)
        {
            Transform wormPart = _wormParts[i];
            if (middleIndices != null && (middleIndices.Contains(i)))
            {
                wormPart.GetComponent<Rigidbody>().AddForce(Vector3.down * GameParameters.WormMiddleSegmentScrunchForce);
            }
            else
            {
                if (wormPart.GetComponent<WormBodySegment>().IsGrounded || wormPart.GetComponent<WormBodySegment>().IsScrunched || (wormPart.GetComponent<WormBodySegment>().TimeSinceLastGrounded < GameParameters.maxTimeSinceLastGrounded))
                {
                    Vector3 forwardDirection = Vector3.Slerp(previousPart.forward, _wormHead.forward, 0.5f);
                    Vector3 jumpDirection = Vector3.Slerp(forwardDirection, Vector3.up, GameParameters.WormJumpAngle).normalized;
                    wormPart.GetComponent<Rigidbody>().AddForce(jumpDirection * GameParameters.WormJumpForce);
                }
            }
            previousPart = _wormParts[i];
        }
    }

    private List<List<int>> GetConsecutiveSegments()
    {
        List<List<int>> segments = new List<List<int>>();
        List<int> currentSegment = null;
        
        if (_wormHead.GetComponent<WormPart>().IsGrounded) 
        {
            currentSegment = new List<int> { -1 }; // Use -1 to represent head
        }
        
        for (int i = 0; i < _wormParts.Count; i++) 
        {
            WormPart part = _wormParts[i].GetComponent<WormPart>();
            WormBodySegment bodySeg = _wormParts[i].GetComponent<WormBodySegment>();
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