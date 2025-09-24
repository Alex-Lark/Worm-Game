using System.Collections.Generic;
using UnityEngine;

public class WormPhysics : MonoBehaviour
{
    private List<CapsuleCollider> segmentColliders = new List<CapsuleCollider>();
    
    void Start()
    {
        //AddCollidersToSegments();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void AddCollidersToSegments()
    {
        // Add collider to head (if it doesn't already have one from CharacterController)
        if (Player.Instance.wormHead.GetComponent<CapsuleCollider>() == null)
        {
            CapsuleCollider headCollider = Player.Instance.wormHead.gameObject.AddComponent<CapsuleCollider>();
            headCollider.radius = GameParameters.SegmentMaxPartDistance;
            headCollider.height = GameParameters.SegmentMaxPartDistance * 2f;
            // Don't set as trigger - we want solid collision
        }
        
        // Add colliders to body segments
        for (int i = 0; i < Player.Instance.wormParts.Count; i++)
        {
            CapsuleCollider segmentCollider = Player.Instance.wormParts[i].gameObject.AddComponent<CapsuleCollider>();
            segmentCollider.radius = GameParameters.SegmentMaxPartDistance * 0.9f; // Slightly smaller to prevent getting stuck
            segmentCollider.height = GameParameters.SegmentMaxPartDistance * 2f;
            // Don't set as trigger - we want solid collision
            
            segmentColliders.Add(segmentCollider);
        }
        
        // Make sure worm segments don't collide with each other
        IgnoreWormSelfCollision();
    }
    
    void IgnoreWormSelfCollision()
    {
        // Get all colliders on the worm
        List<Collider> allWormColliders = new List<Collider>();
        
        // Add head collider
        Collider headCollider = Player.Instance.wormHead.GetComponent<Collider>();
        if (headCollider != null)
            allWormColliders.Add(headCollider);
        
        // Add body colliders
        foreach (var segment in Player.Instance.wormParts)
        {
            Collider segmentCollider = segment.GetComponent<Collider>();
            if (segmentCollider != null)
                allWormColliders.Add(segmentCollider);
        }
        
        // Make each collider ignore all other worm colliders
        for (int i = 0; i < allWormColliders.Count; i++)
        {
            for (int j = i + 1; j < allWormColliders.Count; j++)
            {
                Physics.IgnoreCollision(allWormColliders[i], allWormColliders[j]);
            }
        }
    }
}
