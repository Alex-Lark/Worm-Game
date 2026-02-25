using System;
using System.Collections;
using System.Collections.Generic;
using CreatureParts;
using NUnit.Framework.Constraints;
using TMPro;
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
        
        public WormState CurrentState { get; private set; }
        
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
        
        #endregion
        
        #region private variables

        private bool isPlayerActive = false;
        private WormForwardMovement wormForwardMovement;
        private WormJump wormJump;
        private WormHeadBut wormHeadBut;
        private WormConstructor wormConstructor;
    
        private readonly int wormSegmentCount = GameParameters.WormSegmentCount;
        private readonly float maxPartDistance = GameParameters.SegmentMaxPartDistance;

        private GameObject wormHeadCopy;
        private List<Transform> wormBodySegmentsCopy = new List<Transform>();
        private List<GameObject> attachedWormPartsCopy = new List<GameObject>();
        
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
        
            wormBodySegments.Clear();
            wormConstructor = new WormConstructor(wormHead, wormBodySegments, wormSegmentPrefab, transform, wormSegmentCount, maxPartDistance);
            wormConstructor.CreateWormSegments();
            wormConstructor.ConstructWorm();
            
            GetComponent<WormPhysics>().AddCollidersToSegments();

            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                SetWormInGameScene();
            }
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
        
        public void SetWormInCreatureBuilderScene()
        {
            WormPhysics wormPhysics = GetComponent<WormPhysics>();
            wormPhysics.ResetWormPhysics();
            wormPhysics.ResetWormOrientation();
            wormPhysics.PositionWormSegments(new Vector3(0, 2, 0));
            DeactivatePlayer();
        }
        
        public void SetWormInGameScene()
        {
            wormHead.GetComponent<Rigidbody>().isKinematic = false;
            
            foreach (Transform segment in wormBodySegments)
            {
                segment.GetComponent<Rigidbody>().isKinematic = false;
            }
            
            StartCoroutine(SetupAfterSceneLoad());
            ActivatePlayer();
            currentPlayerHealth = GameParameters.DefaultPlayerHealth;
        }
        
        public void OnPlayerDeath()
        {
            CurrentState = WormState.Dead;
            currentPlayerHealth = 0;
            StartCoroutine(RespawnTimer());

            thirdPersonCamera.GetComponent<CinemachineBrain>().enabled = false;

            wormHeadCopy = DuplicatePart(wormHead.gameObject);
            wormHead.gameObject.SetActive(false);
            
            GetComponent<WormRenderer>().enabled = false;
            GetComponent<LineRenderer>().enabled = false;
            Destroy(transform.Find("WormMesh").gameObject);
            DeathScreenUI.Instance.EnableDeathUI();

            foreach (Transform bodySegment in wormBodySegments)
            {
                GameObject segmentCopy = DuplicatePart(bodySegment.gameObject);
                wormBodySegmentsCopy.Add(segmentCopy.transform);
                Destroy(segmentCopy.GetComponent<ConfigurableJoint>());
                bodySegment.gameObject.SetActive(false);
            }
            
            foreach (GameObject attachedPart in attachedWormParts)
            {
                GameObject partCopy = DuplicatePart(attachedPart);
                attachedWormPartsCopy.Add(partCopy);
                Destroy(partCopy.GetComponent<ConfigurableJoint>());
                attachedPart.gameObject.SetActive(false);
            }
            //TODO: update duplicate part collision ignores
        }
        
        #endregion
        
        #region Private Methods
        
        private GameObject DuplicatePart(GameObject original)
        {
           GameObject copy = Instantiate(original.gameObject, original.transform.position, original.transform.rotation);
            Rigidbody originalRb = original.GetComponent<Rigidbody>();
            Rigidbody copyRb = copy.GetComponent<Rigidbody>();
    
            if (originalRb != null && copyRb != null)
            {
                copyRb.linearVelocity = originalRb.linearVelocity;
                copyRb.angularVelocity = originalRb.angularVelocity;
            }

            return copy;
        }
        
        private IEnumerator RespawnTimer()
        {
            float timeLeft = GameParameters.PlayerRespawnTimeInSeconds;
    
            while (timeLeft > 0)
            {
                DeathScreenUI.Instance.respawnText.text = "Respawning in " + Mathf.Ceil(timeLeft);
                yield return new WaitForSeconds(1f);
                timeLeft -= 1f;
            }
    
            DeathScreenUI.Instance.respawnText.text = "Respawning...";
            RespawnPlayer();
        }

        private void RespawnPlayer()
        {
            CurrentState = WormState.Idle;
            currentPlayerHealth = 100f;
            thirdPersonCamera.GetComponent<CinemachineBrain>().enabled = true;
            DeathScreenUI.Instance.DisableDeathUI();

            wormHead.gameObject.SetActive(true);
            Destroy(wormHeadCopy);
            
            foreach (Transform bodySegmentCopy in wormBodySegmentsCopy)
            {
                Destroy(bodySegmentCopy.gameObject);
            }
            wormBodySegmentsCopy.Clear();
            
            foreach (Transform bodySegment in wormBodySegments)
            {
                bodySegment.gameObject.SetActive(true);
            }

            foreach (GameObject partCopy in attachedWormPartsCopy)
            {
                Destroy(partCopy.gameObject);
            }
            attachedWormPartsCopy.Clear();
            
            foreach (GameObject attachedPart in attachedWormParts)
            {
                attachedPart.SetActive(true);
            }

            if (GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                SetWormInGameScene();
            }
            
            GetComponent<WormRenderer>().enabled = true;
            GetComponent<WormRenderer>().Restart();
        }
        
        private IEnumerator AttackSequence()
        {
            yield return new WaitForSeconds(GameParameters.WormHeadbutTime);
            CurrentState = WormState.AttackCooldown;
            wormHeadBut.EndHeadBut();
            yield return new WaitForSeconds(GameParameters.WormHeadButCoolDown);
            CurrentState = WormState.Idle;
        }
        
        private IEnumerator SetupAfterSceneLoad()
        {
            yield return null;

            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();

            foreach (GameObject rootObj in rootObjects)
            {
                Camera cam = rootObj.GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    thirdPersonCamera = cam.gameObject;
                    Debug.Log($"Camera successfully set to {thirdPersonCamera.name}");
                    break;
                }
            }

            if (thirdPersonCamera == null)
            {
                Debug.LogError($"Could not find camera in scene {activeScene.name}!");
            }
            
            wormHead.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            wormHead.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            foreach (Transform segment in wormBodySegments)
            {
                Rigidbody rb = segment.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            foreach (GameObject part in attachedWormParts)
            {
                Rigidbody rb = part.GetComponent<Rigidbody>();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            GetComponent<WormPhysics>().ResetWormPosition();
            wormConstructor.ConstructWorm();
            
            yield return new WaitForFixedUpdate();

            wormConstructor.ConstructWorm();
            
            wormForwardMovement.SetVariables();
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