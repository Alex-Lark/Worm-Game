using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    
    public string PlayerName { get; private set; }
    
    public int PlayerScore { get; set; }
    public bool IsWormMovingForward { get; private set; }
    public bool IsWormJumping { get; private set; }
    public bool IsWormGrounded { get; private set; }
    public bool IsWormAttacking { get; private set; }
    public bool IsWormInAttackCooldown { get; private set; }
    
    public GameObject thirdPersonCamera;
    public GameObject wormSegmentPrefab;
    public Transform wormHead;
    public Transform wormVisualHead;
    public List<Transform> wormBodySegments;
    public List<GameObject> wormPartsInInventory;

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
        PlayerName = "player1";
        
        IsWormMovingForward = false;
        IsWormGrounded = false;

        _wormForwardMovement = gameObject.GetComponent<WormForwardMovement>();
        _wormJump = gameObject.GetComponent<WormJump>();
        _wormHeadBut = gameObject.GetComponent<WormHeadBut>();
        
        wormBodySegments.Clear();
        CreateWormSegments();
        ConstructWorm();
        gameObject.GetComponent<WormPhysics>().AddCollidersToSegments();

        if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
        {
            SetWormInGameScene();
        }
    }

    private void FixedUpdate()
    {
        if (_isPlayerActive)
        {
            setWormGrounding();
            RotateVisualHead();

            if (IsWormAttacking)
            {
                _wormHeadBut.ReadyHeadbut();
            }

            if (IsWormInAttackCooldown)
            {
                _wormHeadBut.WormheadbutCoolDown();
            }
        }
    }

    public void SetWormInGameScene()
    {
        print("set worm in game scene");
        
        wormHead.GetComponent<Rigidbody>().isKinematic = false;
        foreach (Transform wormPart in wormBodySegments)
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
    
        // Get the active scene
        Scene activeScene = SceneManager.GetActiveScene();
        Debug.Log($"Searching for camera in scene: {activeScene.name}");
    
        // Search for camera specifically in the active scene
        GameObject foundCamera = null;
        GameObject[] rootObjects = activeScene.GetRootGameObjects();
    
        foreach (GameObject rootObj in rootObjects)
        {
            Debug.Log($"Checking root object: {rootObj.name}");
        
            // Check this object and all children for a Camera component
            Camera cam = rootObj.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                foundCamera = cam.gameObject;
                Debug.Log($"Found camera: {foundCamera.name}");
                break;
            }
        }
    
        if (foundCamera != null)
        {
            thirdPersonCamera = foundCamera;
            Debug.Log($"Camera successfully set to {thirdPersonCamera.name}");
        }
        else
        {
            Debug.LogError($"Could not find any camera in scene {activeScene.name}!");
            Debug.Log($"Total root objects in scene: {rootObjects.Length}");
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

            wormHead.GetComponent<Rigidbody>().useGravity = true;
            wormHead.GetComponent<Rigidbody>().isKinematic = false;
            for (int i = 0; i < wormBodySegments.Count; i++)
            {
                currentPos += backDir * _maxPartDistance;

                Transform part = wormBodySegments[i];
                part.position = currentPos;

                part.rotation = wormHead.rotation;
                Rigidbody partRigidbody = part.GetComponent<Rigidbody>();
                partRigidbody.useGravity = true;
                partRigidbody.isKinematic = false;
                partRigidbody.angularVelocity = Vector3.zero;
                partRigidbody.linearVelocity = Vector3.zero;
            }
        }
        _wormForwardMovement.SetVariables();
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
        if (_isPlayerActive && !IsWormJumping && !IsWormAttacking && !IsWormInAttackCooldown)
        {
            
        }

        if ( _isPlayerActive && !IsWormJumping && !IsWormAttacking && !IsWormInAttackCooldown)
        {
            _wormForwardMovement.MoveHead();
            _wormForwardMovement.MoveWormBody();
        }
    }
    
    public void Jump()
    {
        if (IsWormGrounded  && !IsWormAttacking && !IsWormInAttackCooldown)
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
            if (!IsWormAttacking && !IsWormInAttackCooldown)
            {
                IsWormAttacking = true;
                
                if (_attackCoroutine == null)
                {
                    _attackCoroutine =  StartCoroutine(WormAttackTimer());
                }
            }
        }
    }

    private IEnumerator WormAttackTimer()
    {
        yield return new WaitForSeconds(GameParameters.WormHeadbutTime); 
        IsWormAttacking = false;
        _attackCoroutine =  null;
        _wormHeadBut.EndHeadBut();
        IsWormInAttackCooldown = true;
        StartCoroutine(WormCoolDownTimer());

    }

    private IEnumerator WormCoolDownTimer()
    {
        yield return new WaitForSeconds(GameParameters.WormHeadButCoolDown);
        IsWormInAttackCooldown = false;
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
            wormBodySegments.Add(newWormSegment.transform);
        }
    }

    private void ConstructWorm()
    {
        Vector3 currentPos = wormHead.position;
        Vector3 backDir = -wormHead.forward;

        Rigidbody previousSegmentRigidBody = wormHead.gameObject.GetComponent<Rigidbody>();

        for (int i = 0; i < wormBodySegments.Count; i++)
        {
            currentPos += backDir * _maxPartDistance;

            Transform part = wormBodySegments[i];
            part.position = currentPos;
            
            part.rotation = wormHead.rotation;

            previousSegmentRigidBody = part.GetComponent<WormBodySegment>().AddJoint(wormBodySegments[i], previousSegmentRigidBody);
        }
    }

    private void setWormGrounding()
    {
        IsWormGrounded = false; 
        
        foreach (var part in wormBodySegments)
        {
            if (part.GetComponent<WormPart>().IsGrounded)
            {
                IsWormGrounded = true;
                break;
            }
        }
    }
}

