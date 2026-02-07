using System.Collections.Generic;
using UnityEngine;

public class WormPhysics : MonoBehaviour
{
    private List<SphereCollider> segmentColliders = new List<SphereCollider>();
    
    public void AddCollidersToSegments()
    {
        SphereCollider headCollider = Player.Player.Instance.wormHead.GetComponent<SphereCollider>();
        if (headCollider == null)
        {
            headCollider = Player.Player.Instance.wormHead.gameObject.AddComponent<SphereCollider>();
        }
        
        headCollider.radius = GameParameters.WormBodyWidth * 4;
        
        for (int i = 0; i < Player.Player.Instance.wormBodySegments.Count; i++)
        {
            SphereCollider segmentCollider = Player.Player.Instance.wormBodySegments[i].GetComponent<SphereCollider>();
            if (segmentCollider == null)
            {
                segmentCollider = Player.Player.Instance.wormBodySegments[i].gameObject.AddComponent<SphereCollider>();
            }
            segmentCollider.radius = GameParameters.WormBodyWidth * 4;
            
            if (!segmentColliders.Contains(segmentCollider))
                segmentColliders.Add(segmentCollider);
        }
        
        IgnoreWormSelfCollision();
    }
    
    public void ResetWormPhysics()
    {
        SetSegmentPhysics(Player.Player.Instance.wormHead, isKinematic: true, useGravity: false);
        foreach (Transform segment in Player.Player.Instance.wormBodySegments)
        {
            SetSegmentPhysics(segment, isKinematic: true, useGravity: false);
        }
    }

    public void ResetWormOrientation()
    {
        Player.Player.Instance.wormVisualHead.rotation = Quaternion.identity;
        Player.Player.Instance.wormHead.rotation = Quaternion.identity;
        foreach (Transform segment in Player.Player.Instance.wormBodySegments)
        {
            segment.rotation = Quaternion.identity;
        }
    }

    public void SetSegmentPhysics(Transform segment, bool isKinematic, bool useGravity)
    {
        Rigidbody rb = segment.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.isKinematic = isKinematic;
        rb.useGravity = useGravity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void PositionWormSegments(Vector3 headPosition)
    {
        Player.Player.Instance.wormHead.position = headPosition;
        Vector3 currentPosition = headPosition;
        Vector3 backDirection = -Player.Player.Instance.wormHead.forward;

        for (int i = 0; i < Player.Player.Instance.wormBodySegments.Count; i++)
        {
            currentPosition += backDirection * GameParameters.SegmentMaxPartDistance;
            Player.Player.Instance.wormBodySegments[i].position = currentPosition;
            Player.Player.Instance.wormBodySegments[i].rotation = Player.Player.Instance.wormHead.rotation;
        }
    }

    public void ResetWormPosition()
    {
        if (Player.Player.Instance.wormHead == null) return;
        
        Player.Player.Instance.wormHead.position = new Vector3(0, 2, 0);
        Rigidbody headRb = Player.Player.Instance.wormHead.GetComponent<Rigidbody>();
        if (headRb != null)
        {
            headRb.useGravity = true;
            headRb.isKinematic = false;
            headRb.linearVelocity = Vector3.zero;
            headRb.angularVelocity = Vector3.zero;
        }

        Vector3 currentPos = Player.Player.Instance.wormHead.position;
        Vector3 backDir = -Player.Player.Instance.wormHead.forward;

        for (int i = 0; i < Player.Player.Instance.wormBodySegments.Count; i++)
        {
            currentPos += backDir * GameParameters.SegmentMaxPartDistance;
            Transform segment = Player.Player.Instance.wormBodySegments[i];
            segment.position = currentPos;
            segment.rotation = Player.Player.Instance.wormHead.rotation;
            
            Rigidbody segmentRb = segment.GetComponent<Rigidbody>();
            segmentRb.useGravity = true;
            segmentRb.isKinematic = false;
            segmentRb.linearVelocity = Vector3.zero;
            segmentRb.angularVelocity = Vector3.zero;
        }
    }

    void IgnoreWormSelfCollision()
    {
        List<Collider> allWormColliders = new List<Collider>();
        
        Collider headCollider = Player.Player.Instance.wormHead.GetComponent<Collider>();
        if (headCollider != null)
            allWormColliders.Add(headCollider);
        
        foreach (var segment in Player.Player.Instance.wormBodySegments)
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