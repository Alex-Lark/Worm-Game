using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;
    
    public bool IsWormMoving { get; private set; }
    public bool IsWormGrounded { get; private set; }

    public GameObject thirdPersonCamera;
    public GameObject wormSegmentPrefab;
    public Transform wormHead;
    public Transform wormVisualHead;
    public List<Transform> wormParts;

    private readonly int _wormSegmentCount = GameParameters.WormSegmentCount;
    private readonly float _maxPartDistance = GameParameters.SegmentMaxPartDistance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        IsWormMoving = false;
        IsWormGrounded = false; 
        
        wormParts.Clear();
        CreateWormSegments();
        ConstructWorm();
        gameObject.GetComponent<WormPhysics>().AddCollidersToSegments();
        gameObject.GetComponent<WormForwardMovement>().CreateSegmentMaxForwardForceList(_wormSegmentCount);
    }

    private void FixedUpdate()
    {
        setWormGrounding();
        RotateVisualHead();
    }

    public void StartWormMoving()
    {
        IsWormMoving = true;
    }

    public void StopWormMoving()
    {
        IsWormMoving = false;
    }
    
    private void RotateVisualHead()
    {
        var forward = thirdPersonCamera.transform.forward;
        
        Vector3 cameraForward = new Vector3(forward.x, forward.y + GameParameters.VisualHeadVerticalOffset, forward.z);
        cameraForward.Normalize();

        if (cameraForward.magnitude > 0.1f)
        {
            float angle = Vector3.Angle(wormHead.forward, cameraForward);
            
            if (angle > 90f)
            {
                Vector3 clampedDirection = Vector3.RotateTowards(wormHead.forward, cameraForward, 90f * Mathf.Deg2Rad, 0f);
                wormVisualHead.rotation = Quaternion.LookRotation(clampedDirection);
            }
            else
            {
                wormVisualHead.rotation = Quaternion.LookRotation(cameraForward);
            }
        }

    }

    public void Jump()
{
    if (wormHead.GetComponent<WormPart>().IsGrounded)
    {
        GameObject groundObject = wormHead.GetComponent<WormPart>().GroundObject;
        if (groundObject != null)
        {
            Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
            if (groundRb != null)
            {
                Vector3 forceToApply = -GameParameters.WormJumpForce * wormHead.up;

                groundRb.AddForceAtPosition(forceToApply, wormHead.position);
            }
            else
            {
                wormHead.GetComponent<Rigidbody>().AddForce(GameParameters.WormJumpForce * wormHead.up);
            }
        }
    }
    
    for (int i = 0; i < wormParts.Count; i++)
    {
        if (wormParts[i].GetComponent<WormPart>().IsGrounded)
        {
            GameObject groundObject = wormParts[i].GetComponent<WormPart>().GroundObject;
            if (groundObject != null)
            {
                Rigidbody groundRb = groundObject.GetComponent<Rigidbody>();
                if (groundRb != null)
                {
                    groundRb.AddForceAtPosition(-GameParameters.WormJumpForce * wormHead.up, wormParts[i].position);
                }
                else
                {
                    wormParts[i].GetComponent<Rigidbody>().AddForce(GameParameters.WormJumpForce * wormHead.up);
                }
            }
        }
    }
}
    
    private void CreateWormSegments()
    {
        for (int i = 0; i < _wormSegmentCount; i++)
        {
            GameObject newWormSegment = Instantiate(wormSegmentPrefab, transform);
            wormParts.Add(newWormSegment.transform);
        }
    }

    private void ConstructWorm()
    {
        Vector3 currentPos = wormHead.position;
        Vector3 backDir = -wormHead.forward;

        Rigidbody previousSegmentRigidBody = wormHead.gameObject.GetComponent<Rigidbody>();

        for (int i = 0; i < wormParts.Count; i++)
        {
            currentPos += backDir * _maxPartDistance;

            Transform part = wormParts[i];
            part.position = currentPos;
            
            part.rotation = wormHead.rotation;

            previousSegmentRigidBody = part.GetComponent<WormBodySegment>().AddJoint(wormParts[i], previousSegmentRigidBody);
        }
    }

    private void setWormGrounding()
    {
        IsWormGrounded = false; 
        
        foreach (var part in wormParts)
        {
            if (part.GetComponent<WormPart>().IsGrounded)
            {
                IsWormGrounded = true;
            }
        }
    }
}

