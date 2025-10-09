using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;
    
    public bool IsWormMovingForward { get; private set; }
    public bool IsWormJumping { get; private set; }
    public bool IsWormGrounded { get; private set; }
    
    public GameObject thirdPersonCamera;
    public GameObject wormSegmentPrefab;
    public Transform wormHead;
    public Transform wormVisualHead;
    public List<Transform> wormParts;

    private WormForwardMovement _wormForwardMovement;
    private WormJump _wormJump;
    
    private readonly int _wormSegmentCount = GameParameters.WormSegmentCount;
    private readonly float _maxPartDistance = GameParameters.SegmentMaxPartDistance;
    
    private Coroutine _jumpCoroutine;
    
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
        IsWormMovingForward = false;
        IsWormGrounded = false;

        _wormForwardMovement = gameObject.GetComponent<WormForwardMovement>();
        _wormJump = gameObject.GetComponent<WormJump>();
        
        wormParts.Clear();
        CreateWormSegments();
        ConstructWorm();
        gameObject.GetComponent<WormPhysics>().AddCollidersToSegments();
    }

    private void FixedUpdate()
    {
        setWormGrounding();
        RotateVisualHead();
    }

    public void StartWormMoving()
    {
        IsWormMovingForward = true;
    }

    public void StopWormMoving()
    {
        IsWormMovingForward = false;
    }

    public void StartJump()
    {
        IsWormJumping = true;
        _wormJump.StartJump();
    }

    public void StopJump()
    {
        IsWormJumping = false;
        _wormJump.StopJump();
    }

    public void MoveForward()
    {
        if (!IsWormJumping)
        {
            _wormForwardMovement.MoveHead();
            _wormForwardMovement.MoveWormBody();
        }
    }
    
    public void Jump()
    {
        if (IsWormGrounded)
        {
            IsWormJumping = true;
            _wormJump.Jump();
        }
        IsWormJumping = false;
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
                Vector3 clampedDirection =
                    Vector3.RotateTowards(wormHead.forward, cameraForward, 90f * Mathf.Deg2Rad, 0f);
                wormVisualHead.rotation = Quaternion.LookRotation(clampedDirection);
            }
            else
            {
                wormVisualHead.rotation = Quaternion.LookRotation(cameraForward);
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

