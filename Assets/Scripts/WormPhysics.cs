using System.Collections.Generic;
using UnityEngine;

public class WormPhysics : MonoBehaviour
{
    private List<SphereCollider> segmentColliders = new List<SphereCollider>();
    
    public void AddCollidersToSegments()
    {
        SphereCollider headCollider = Player.Instance.wormHead.GetComponent<SphereCollider>();
        if (headCollider == null)
        {
            headCollider = Player.Instance.wormHead.gameObject.AddComponent<SphereCollider>();
        }
        
        headCollider.radius = GameParameters.WormBodyWidth * 4;
        
        for (int i = 0; i < Player.Instance.wormParts.Count; i++)
        {
            SphereCollider segmentCollider = Player.Instance.wormParts[i].GetComponent<SphereCollider>();
            if (segmentCollider == null)
            {
                segmentCollider = Player.Instance.wormParts[i].gameObject.AddComponent<SphereCollider>();
            }
            segmentCollider.radius = GameParameters.WormBodyWidth * 4; 
            
            if (!segmentColliders.Contains(segmentCollider))
                segmentColliders.Add(segmentCollider);
        }
        
        IgnoreWormSelfCollision();
    }
    
    void IgnoreWormSelfCollision()
    {
        List<Collider> allWormColliders = new List<Collider>();
        
        Collider headCollider = Player.Instance.wormHead.GetComponent<Collider>();
        if (headCollider != null)
            allWormColliders.Add(headCollider);
        
        foreach (var segment in Player.Instance.wormParts)
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
}