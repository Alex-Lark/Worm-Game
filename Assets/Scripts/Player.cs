using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public bool IsWormMovingForward { get; private set; }
    public bool IsWormJumping { get; private set; }
    public bool IsWormGrounded { get; private set; }
    public bool IsWormAttacking { get; private set; }
    
    public GameObject thirdPersonCamera;
    public GameObject wormSegmentPrefab;
    public Transform wormHead;
    public Transform wormVisualHead;
    public List<Transform> wormParts;

    private bool _isPlayerActive = false;
    private WormForwardMovement _wormForwardMovement;
    private WormJump _wormJump;
    private WormHeadBut _wormHeadBut;
    
    private readonly int _wormSegmentCount = GameParameters.WormSegmentCount;
    private readonly float _maxPartDistance = GameParameters.SegmentMaxPartDistance;
    
    private Coroutine _attackCoroutine;
    
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
        _wormHeadBut = gameObject.GetComponent<WormHeadBut>();
        
        wormParts.Clear();
        CreateWormSegments();
        ConstructWorm();
        gameObject.GetComponent<WormPhysics>().AddCollidersToSegments();

        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            SetWormInGameScene();
        }
    }

    private void FixedUpdate()
    {
        if (_isPlayerActive && thirdPersonCamera != null)
        {
            setWormGrounding();
            RotateVisualHead();

            if (IsWormAttacking)
            {
                _wormHeadBut.ReadyHeadbut();
            }
        }
    }

    public void SetWormInGameScene()
    {
        print("set worm in game scene");
        
        wormHead.GetComponent<Rigidbody>().isKinematic = false;
        foreach (Transform wormPart in wormParts)
        {
            wormPart.GetComponent<Rigidbody>().isKinematic = false;
        }
            
        StartCoroutine(SetupAfterSceneLoad());
        ActivatePlayer();
    }

    private IEnumerator SetupAfterSceneLoad()
    {
        // Wait until GameScene has loaded fully
        yield return null;

        // Move the Player object (and its hierarchy) into the active GameScene
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
        print($"Player moved to scene: {SceneManager.GetActiveScene().name}");

        // Assign the camera properly
        var cam = Camera.main;
        if (cam != null)
        {
            thirdPersonCamera = cam.gameObject;
            print($"Camera set to {thirdPersonCamera.name}");
        }
        else
        {
            Debug.LogWarning("No MainCamera found in GameScene!");
        }

        // Reset worm position safely
        if (wormHead != null)
        {
            wormHead.position = new Vector3(0, 1, 0);
            var rb = wormHead.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Vector3 currentPos = wormHead.position;
            Vector3 backDir = -wormHead.forward;

            Rigidbody previousSegmentRigidBody = wormHead.gameObject.GetComponent<Rigidbody>();

            for (int i = 0; i < wormParts.Count; i++)
            {
                currentPos += backDir * _maxPartDistance;

                Transform part = wormParts[i];
                part.position = currentPos;

                part.rotation = wormHead.rotation;
                Rigidbody partRigidbody = wormHead.GetComponent<Rigidbody>();
                partRigidbody.angularVelocity = Vector3.zero;
                partRigidbody.linearVelocity = Vector3.zero;
            }
        }
    }

    public void ActivatePlayer()
    {
        _isPlayerActive = true;
    }

    public void DeactivatePlayer()
    {
        _isPlayerActive = false;
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
        if ( _isPlayerActive && !IsWormJumping && !IsWormAttacking)
        {
            _wormForwardMovement.MoveHead();
            _wormForwardMovement.MoveWormBody();
        }
    }
    
    public void Jump()
    {
        if (IsWormGrounded  && !IsWormAttacking)
        {
            IsWormJumping = true;
            _wormJump.Jump();
        }
        IsWormJumping = false;
    }

    public void Attack()
    {
        if (IsWormGrounded)
        {
            if (!IsWormAttacking)
            {
                IsWormAttacking = true;
            }

            if (_attackCoroutine == null)
            {
                _attackCoroutine =  StartCoroutine(WormAttackTimer());
            }
        }
    }

    private IEnumerator WormAttackTimer()
    {
        yield return new WaitForSeconds(GameParameters.WormHeadbutTime); 
        IsWormAttacking = false;
        _attackCoroutine =  null;
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
                break;
            }
        }
    }
}

