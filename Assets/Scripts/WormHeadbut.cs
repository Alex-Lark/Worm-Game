using System.Collections.Generic;
using UnityEngine;

public class WormHeadBut : MonoBehaviour
{
    private List<Transform> _wormParts;

    private void Start()
    {
        _wormParts = Player.Instance.wormParts;
    }
    

    public void ReadyHeadbut()
    {
        int segmentCount = GameParameters.WormSegmentCount + 1; // head
        int liftedSegment = 0;
        
        for (int i = 0; i < _wormParts.Count; i++)
        {
            Transform wormPart = _wormParts[i];
            Rigidbody wormPartRigidBody = wormPart.GetComponent<Rigidbody>();
            if (i > segmentCount / 2)
            {
                GroundBackSegment(wormPartRigidBody);
            }
            else
            {
                liftedSegment++;
                LiftFrontSegments(wormPartRigidBody, liftedSegment, segmentCount);
            }
        }
        LiftFrontSegments(Player.Instance.wormHead.GetComponent<Rigidbody>(), segmentCount, segmentCount);
    }

    private void LiftFrontSegments(Rigidbody wormPart, int liftedSegment, int segmentCount)
    {
        float forceMutliplier = liftedSegment / (segmentCount/2);
        wormPart.AddForce(Vector3.up * (GameParameters.WormHeadbutGroundingForce * forceMutliplier));
    }

    private void GroundBackSegment(Rigidbody wormPart)
    {
        if (wormPart.GetComponent<WormBodySegment>().IsGrounded)
        {
            wormPart.AddForce(Vector3.down * GameParameters.WormHeadbutGroundingForce);
        }
    }
}
