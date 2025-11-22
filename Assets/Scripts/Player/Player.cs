using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }
    
        public string PlayerName { get; private set; }

        public int PlayerScore = 1;
        public bool IsWormMovingForward { get; private set; }
        public bool IsWormJumping { get; private set; }
        public bool IsWormGrounded { get; private set; }
        public bool IsWormAttacking { get; private set; }
        public bool IsWormInAttackCooldown { get; private set; }
        
        public float MaxVelocity { get; set; }
    
        public GameObject thirdPersonCamera;
        public GameObject wormSegmentPrefab;
        public Transform wormHead;
        public Transform wormVisualHead;
        public List<Transform> wormBodySegments;
        public List<GameObject> attachedWormParts;
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

            MaxVelocity = GameParameters.WormMaxVelocity;
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

        public void SetWormInCreatureBuilderScene()
        {
            ResetWormPhysics();
            ResetWormOrientation();
            PositionWormSegments(new Vector3(0, 2, 0));
            DeactivatePlayer();
        }

        private void ResetWormPhysics()
        {
            SetSegmentPhysics(wormHead, isKinematic: true, useGravity: false);
    
            foreach (Transform segment in wormBodySegments)
            {
                SetSegmentPhysics(segment, isKinematic: true, useGravity: false);
            }
        }

        private void ResetWormOrientation()
        {
            wormVisualHead.rotation = Quaternion.identity;
            wormHead.rotation = Quaternion.identity;
    
            foreach (Transform segment in wormBodySegments)
            {
                segment.rotation = Quaternion.identity;
            }
        }

        private void SetSegmentPhysics(Transform segment, bool isKinematic, bool useGravity)
        {
            Rigidbody rb = segment.GetComponent<Rigidbody>();
            if (rb == null) return;
    
            rb.isKinematic = isKinematic;
            rb.useGravity = useGravity;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        private void PositionWormSegments(Vector3 headPosition)
        {
            wormHead.position = headPosition;
            Vector3 currentPosition = headPosition;
            Vector3 backDirection = -wormHead.forward;
    
            for (int i = 0; i < wormBodySegments.Count; i++)
            {
                currentPosition += backDirection * _maxPartDistance;
                Transform segment = wormBodySegments[i];
        
                segment.position = currentPosition;
                segment.rotation = wormHead.rotation;
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
            //SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
            //print($"Player moved to scene: {SceneManager.GetActiveScene().name}");
    
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
                wormHead.position = new Vector3(0, 2, 0);
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
            if ( _isPlayerActive && !IsWormJumping && !IsWormAttacking && !IsWormInAttackCooldown)
            {
                _wormForwardMovement.MoveHead();
                _wormForwardMovement.MoveWormBody();

                foreach (var part in attachedWormParts)
                {
                    part.GetComponent<WormPart>().MoveForward();
                }
            }
        }
    
        public void Jump()
        {
            if (IsWormGrounded  && !IsWormAttacking && !IsWormInAttackCooldown)
            {
                IsWormJumping = true;
                _wormJump.Jump();
                foreach (var part in attachedWormParts)
                {
                    part.GetComponent<WormPart>().Jump();
                }
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
            if (thirdPersonCamera == null)
            {
                Debug.LogWarning("RotateVisualHead: thirdPersonCamera is NULL!");
                return;
            }

            Vector3 forward = thirdPersonCamera.transform.forward;
        
            Vector3 cameraForward = forward;
            cameraForward.y += GameParameters.VisualHeadVerticalOffset;
            cameraForward.Normalize();
        
            if (cameraForward.sqrMagnitude < 0.01f)
                return;
        
            float signedAngle = Vector3.SignedAngle(wormHead.forward, cameraForward, Vector3.up);

            float maxAngle = 90f;
        
            float clampedSigned = Mathf.Clamp(signedAngle, -maxAngle, maxAngle);
        
            Quaternion clampedRotation = Quaternion.AngleAxis(clampedSigned, Vector3.up) * wormHead.rotation;

            wormVisualHead.rotation = clampedRotation;
        }

        private void CreateWormSegments() {
            WormPart previousSegment = wormHead.GetComponent<WormPart>();
    
            // First pass: create all segments and set previousSegment
            for (int i = 0; i < _wormSegmentCount; i++)
            {
                GameObject newWormSegment = Instantiate(wormSegmentPrefab, transform);
                newWormSegment.GetComponent<WormBodySegment>().previousSegment = previousSegment;
                wormBodySegments.Add(newWormSegment.transform);
        
                previousSegment = newWormSegment.GetComponent<WormBodySegment>();
            }
            
            for (int i = 0; i < wormBodySegments.Count - 1; i++)
            {
                wormBodySegments[i].GetComponent<WormBodySegment>().nextSegment = 
                    wormBodySegments[i + 1].GetComponent<WormBodySegment>();
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

            foreach (var part in attachedWormParts)
            {
                if (part.GetComponent<WormPart>().IsGrounded)
                {
                    IsWormGrounded = true;
                    break;
                }
            }
        }
    }
}

