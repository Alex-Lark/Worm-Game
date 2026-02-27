using System.Collections;
using System.Collections.Generic;
using CreatureParts;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public enum WormState
    {
        Idle,
        Moving,
        Jumping,
        Attacking,
        AttackCooldown,
        Dead
    }

    public class Player : MonoBehaviour
    {
        
        #region Public Properties
        [Header("Public Properties")]
        
        public static Player Instance { get; private set; }
        public string PlayerName { get; private set; }
        
        public WormState CurrentState { get; set; }
        
        public bool IsWormGrounded { get; private set; }
        
        public float MaxVelocity { get; set; }
        public bool IsWormMovingForward => CurrentState == WormState.Moving;
        public bool IsWormJumping => CurrentState == WormState.Jumping;
        public bool IsWormAttacking => CurrentState == WormState.Attacking;
        public bool IsWormInAttackCooldown => CurrentState == WormState.AttackCooldown;
        
        #endregion
        
        #region public variables
    
        public int playerScore = 1;
        public float maxPlayerHealth = GameParameters.DefaultPlayerHealth;
        public float currentPlayerHealth = GameParameters.DefaultPlayerHealth;
        public GameObject thirdPersonCamera;
        public GameObject wormSegmentPrefab;
        public Transform wormHead;
        public Transform wormVisualHead;
        public List<Transform> wormBodySegments;
        public List<GameObject> attachedWormParts;
        public List<GameObject> wormPartsInInventory;

        public PlayerSpawning playerSpawning;
        public WormForwardMovement wormForwardMovement;
        public WormConstructor wormConstructor;
        
        public GameObject wormHeadCopy;
        
        #endregion
        
        #region private variables

        private bool isPlayerActive = false;
        private WormJump wormJump;
        private WormHeadBut wormHeadBut;
    
        private readonly int wormSegmentCount = GameParameters.WormSegmentCount;
        private readonly float maxPartDistance = GameParameters.SegmentMaxPartDistance;
        
        #endregion
    
        #region Built-In Methods
        
        void Awake()
        {
            //TODO: reconfigure usage of instance once multiplayer is introduced
            
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
            CurrentState = WormState.Idle;
            IsWormGrounded = false;
            MaxVelocity = GameParameters.WormMaxVelocity;

            wormForwardMovement = GetComponent<WormForwardMovement>();
            wormJump = GetComponent<WormJump>();
            wormHeadBut = GetComponent<WormHeadBut>();
            playerSpawning = GetComponent<PlayerSpawning>();
        
            wormBodySegments.Clear();
            wormConstructor = new WormConstructor(wormHead, wormBodySegments, wormSegmentPrefab, transform, wormSegmentCount, maxPartDistance);
            wormConstructor.CreateWormSegments();
            wormConstructor.ConstructWorm();
            
            GetComponent<WormPhysics>().AddCollidersToSegments();
        }

        private void Update()
        {
            //temp
            if (Input.GetKeyDown(KeyCode.K))
            {
                OnPlayerDeath();
            }
        }

        private void FixedUpdate()
        {
            if (!isPlayerActive) return;
            
            SetWormGrounding();
            RotateVisualHead();

            if (IsWormAttacking)
            {
                wormHeadBut.ReadyHeadbut();
            }

            if (IsWormInAttackCooldown)
            {
                wormHeadBut.WormheadbutCoolDown();
            }

            if (currentPlayerHealth < maxPlayerHealth && CurrentState != WormState.Dead)
            {
                currentPlayerHealth += GameParameters.PlayerHealthRegen;
            }
        }
        
        #endregion

        #region Public Methods
        
        public void ActivatePlayer() => isPlayerActive = true;
        public void DeactivatePlayer() => isPlayerActive = false;
        
        public void StartWormMoving()
        {
            if (CurrentState == WormState.Dead) return;
            CurrentState = WormState.Moving;
        }
        
        public void StopWormMoving()
        {
            if (CurrentState == WormState.Dead) return;
            CurrentState = WormState.Idle;
        }
        
        public void MoveForward()
        {
            if (!isPlayerActive || IsWormJumping || IsWormAttacking || IsWormInAttackCooldown || CurrentState == WormState.Dead) return;
            
            wormForwardMovement.MoveHead();
            wormForwardMovement.MoveWormBody();

            foreach (var part in attachedWormParts)
            {
                part.GetComponent<CreaturePart>().MoveForward();
            }
        }

        public void DamagePlayer(Collision other, GameObject hitGameObject)
        {
            float collisionForce = other.impulse.magnitude;

            if (hitGameObject.GetComponent<WormHead>() != null)
            {
                if ((CurrentState == WormState.Attacking || CurrentState == WormState.AttackCooldown))
                {
                    collisionForce *= GameParameters.HeadbutDamageReductionOnHead;
                }
                else
                {
                    collisionForce *= GameParameters.HeadDamageMultiplier;
                }
            }
                
            if (other.gameObject.GetComponent<SpikePart>() != null)
            {
                if (collisionForce > GameParameters.MinSpikeCollisionForceToDamage)
                {
                    float damage = collisionForce * GameParameters.SpikeForceToDamageMultiplier;
                    currentPlayerHealth -= damage;
                }
            }
            else if (collisionForce > GameParameters.MinBluntCollisionForceToDamage)
            {
                Debug.Log("Blunt collision between " + other.gameObject + " and " + hitGameObject);
                float damage = collisionForce * GameParameters.BluntForceToDamageMultiplier;
                currentPlayerHealth -= damage;
            }

            if (CurrentState != WormState.Dead && currentPlayerHealth < 0)
            {
                OnPlayerDeath();
            }
        }

        public void Jump()
        {
            if (!IsWormGrounded || IsWormAttacking || IsWormInAttackCooldown || CurrentState == WormState.Dead) return;
            
            wormJump.Jump();
            foreach (var part in attachedWormParts)
            {
                part.GetComponent<CreaturePart>().Jump();
            }
        }

        public void Attack()
        {
            if (!IsWormGrounded || IsWormAttacking || IsWormInAttackCooldown || CurrentState == WormState.Dead) return;
            
            CurrentState = WormState.Attacking;
            StartCoroutine(AttackSequence());
        }

        public void ResetPlayer()
        {
            foreach (GameObject part in attachedWormParts)
            {
                Destroy(part);
            }
            attachedWormParts.Clear();

            foreach (GameObject part in wormPartsInInventory)
            {
                Destroy(part);
            }
            
            wormPartsInInventory.Clear();
            
            CurrentState = WormState.Idle;
            DeactivatePlayer();
        }
        
        public void OnPlayerDeath()
        {
            if (!GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                return;
            }
            if (CurrentState == WormState.Dead) return;
            
            CurrentState = WormState.Dead;
            currentPlayerHealth = 0;

            thirdPersonCamera.GetComponent<CinemachineBrain>().enabled = false;
            
            GetComponent<WormRenderer>().enabled = false;
            GetComponent<LineRenderer>().enabled = false;
            
            if (transform.Find("WormMesh").gameObject)
            {
                Destroy(transform.Find("WormMesh").gameObject);
            }
            
            playerSpawning.deathScreenUI.EnableDeathUI();
            
            wormHeadCopy = DuplicatePart(wormHead.gameObject);
            wormHead.gameObject.SetActive(false);

            foreach (Transform bodySegment in wormBodySegments)
            {
                bodySegment.gameObject.SetActive(false);
            }
            
            foreach (GameObject attachedPart in attachedWormParts)
            {
                attachedPart.gameObject.SetActive(false);
                attachedPart.GetComponent<AttachablePart>().enabled = false;
            }
            
            playerSpawning.TryToRespawn();
        }
        
        public void CancelDeath()
        {
            if (CurrentState != WormState.Dead) return;
            GetComponent<WormRenderer>().enabled = true;
            GetComponent<WormRenderer>().Restart();
    
            wormHead.gameObject.SetActive(true);
            foreach (Transform bodySegment in wormBodySegments)
                bodySegment.gameObject.SetActive(true);
            foreach (GameObject attachedPart in attachedWormParts)
            {
                attachedPart.SetActive(true);
                attachedPart.GetComponent<AttachablePart>().enabled = true;
            }

            if (wormHeadCopy != null)
                Destroy(wormHeadCopy);

            CurrentState = WormState.Idle;
        }
        
        #endregion
        
        #region Private Methods
        
        private GameObject DuplicatePart(GameObject original)
        {
           GameObject copy = Instantiate(original.gameObject, original.transform.position, original.transform.rotation);
           copy.AddComponent<DeadBodyPart>();
           Rigidbody originalRb = original.GetComponent<Rigidbody>();
           Rigidbody copyRb = copy.GetComponent<Rigidbody>();
    
            if (originalRb != null && copyRb != null)
            {
                copyRb.linearVelocity = originalRb.linearVelocity * GameParameters.DeadPartVelocityMultiplier;
                copyRb.angularVelocity = originalRb.angularVelocity * GameParameters.DeadPartVelocityMultiplier;
            }

            return copy;
        }
        
        private IEnumerator AttackSequence()
        {
            yield return new WaitForSeconds(GameParameters.WormHeadbutTime);
            CurrentState = WormState.AttackCooldown;
            wormHeadBut.EndHeadBut();
            yield return new WaitForSeconds(GameParameters.WormHeadButCoolDown);
            CurrentState = WormState.Idle;
        }
        
        private void SetWormGrounding()
        {
            IsWormGrounded = false;
            
            foreach (var segment in wormBodySegments)
            {
                if (segment.GetComponent<CreaturePart>().IsGrounded)
                {
                    IsWormGrounded = true;
                    return;
                }
            }

            foreach (var part in attachedWormParts)
            {
                if (part.GetComponent<CreaturePart>().IsGrounded)
                {
                    IsWormGrounded = true;
                    return;
                }
            }
        }

        private void RotateVisualHead()
        {
            if (thirdPersonCamera == null)
            {
                Debug.LogWarning("RotateVisualHead: thirdPersonCamera is NULL!");
                return;
            }

            Vector3 cameraForward = thirdPersonCamera.transform.forward;
            cameraForward.y += GameParameters.VisualHeadVerticalOffset;
            cameraForward.Normalize();
        
            if (cameraForward.sqrMagnitude < 0.01f) return;
        
            float signedAngle = Vector3.SignedAngle(wormHead.forward, cameraForward, Vector3.up);
            float clampedAngle = Mathf.Clamp(signedAngle, -90f, 90f);
            wormVisualHead.rotation = Quaternion.AngleAxis(clampedAngle, Vector3.up) * wormHead.rotation;
        }
        
        #endregion
    }
}