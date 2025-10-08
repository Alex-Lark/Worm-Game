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
        if (_wormHead.GetComponent<WormPart>().IsGrounded)
        {
            GameObject groundObject = _wormHead.GetComponent<WormPart>().GroundObject;
            if (groundObject != null)
            {
                Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
                if (groundRb != null)
                {
                    Vector3 forceToApply = -GameParameters.WormJumpForce * _wormHead.up;

                    groundRb.AddForceAtPosition(forceToApply, _wormHead.position);
                }
                else
                {
                    _wormHead.GetComponent<Rigidbody>().AddForce(GameParameters.WormJumpForce * _wormHead.up);
                }
            }
        }
    
        for (int i = 0; i < _wormParts.Count; i++)
        {
            if (_wormParts[i].GetComponent<WormPart>().IsGrounded)
            {
                GameObject groundObject = _wormParts[i].GetComponent<WormPart>().GroundObject;
                if (groundObject != null)
                {
                    Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
                    if (groundRb != null)
                    {
                        groundRb.AddForceAtPosition(-GameParameters.WormJumpForce * _wormHead.up, _wormParts[i].position);
                    }
                    else
                    {
                        _wormParts[i].GetComponent<Rigidbody>().AddForce(GameParameters.WormJumpForce * _wormHead.up);
                    }
                }
            }
        }
    }
}
