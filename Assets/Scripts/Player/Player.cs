using System;
using System.Collections;
using System.Collections.Generic;
using CreatureParts;
using PurrNet;
using Unity.VisualScripting;
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
        AttackCooldown
    }

    public class Player : NetworkBehaviour
    {
        
        #region Public Properties
        [Header("Public Properties")]

        public string PlayerName = "Player1";
        
        public WormState CurrentState { get; private set; }
        
        public bool IsWormGrounded { get; private set; }
        
        public float MaxVelocity { get; set; }
        public bool IsWormJumping => CurrentState == WormState.Jumping;
        public bool IsWormAttacking => CurrentState == WormState.Attacking;
        public bool IsWormInAttackCooldown => CurrentState == WormState.AttackCooldown;
        
        #endregion
        
        #region public variables
    
        public int playerScore = 1;
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
        
        private bool hasBeenSetup = false;

        private bool isRegistered = false;
        
        #endregion
    
        #region Built-In Methods
        
        private void Start()
        {
            if (!isRegistered)
            {
                DontDestroyOnLoad(gameObject);
                LocalPlayer.Register(this); 
                OwnerSetup();
            }
            
            if (isOwner || !isRegistered)
                SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnSpawned(bool asServer)
        {
            isRegistered = true;
            if (isOwner && !asServer)
            {
                LocalPlayer.Register(this);
                if (!GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            
            bool shouldDoOwnerSetup = isOwner && (asServer || !isServer);
            bool shouldDoRemoteSetup = !isOwner || (asServer && !isOwner);

            if (shouldDoOwnerSetup)
            {
                OwnerSetup();
                return;
            }

            if (shouldDoRemoteSetup && !hasBeenSetup && GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                StartCoroutine(FindAndSetupRemoteWorm());
            }
        }

        private void FixedUpdate()
        {
            if (!isPlayerActive) return;
            
            SetWormGrounding();

            if (thirdPersonCamera != null)
            {
                RotateVisualHead();
            }

            if (IsWormAttacking)
            {
                wormHeadBut.ReadyHeadbut();
            }

            if (IsWormInAttackCooldown)
            {
                wormHeadBut.WormheadbutCoolDown();
            }
        }

        protected override void OnDespawned() {
            LocalPlayer.Unregister(this);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "CreatureBuilderScene")
            {
                StartCoroutine(SetWormInCreatureBuilderScene());
            }
            else if (GameSceneList.IsSceneAGameScene(scene.name))
            {
                SetWormInGameScene();
            }
        }
        
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        #endregion

        #region Public Methods
        public void StartWormMoving() => CurrentState = WormState.Moving;
        public void StopWormMoving() => CurrentState = WormState.Idle;
        
        public void ActivatePlayer() => isPlayerActive = true;
        public void DeactivatePlayer() => isPlayerActive = false;
        
        public void MoveForward()
        {
            if (!isPlayerActive || IsWormJumping || IsWormAttacking || IsWormInAttackCooldown) return;
            
            wormForwardMovement.MoveHead();
            wormForwardMovement.MoveWormBody();

            foreach (var part in attachedWormParts)
            {
                part.GetComponent<CreaturePart>().MoveForward();
            }
        }
    
        public void Jump()
        {
            if (!IsWormGrounded || IsWormAttacking || IsWormInAttackCooldown) return;
            
            wormJump.Jump();
            foreach (var part in attachedWormParts)
            {
                part.GetComponent<CreaturePart>().Jump();
            }
        }

        public void Attack()
        {
            if (!IsWormGrounded || IsWormAttacking || IsWormInAttackCooldown) return;
            
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
        
        public IEnumerator SetWormInCreatureBuilderScene()
        {
            yield return null;
    
            var wormPhysics = GetComponent<WormPhysics>();
            
            wormPhysics.ResetWormPhysics();
            wormPhysics.ResetWormOrientation();
            
            wormPhysics.PositionWormSegments(new Vector3(0, 2, 0));
            
            yield return null;
            
            wormConstructor.ConstructWorm();
    
            yield return null;
    
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
        }
        
        #endregion
        
        #region Private Methods
        
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

            GetComponent<WormPhysics>().ResetWormPosition();
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
        
        private void OwnerSetup()
        {
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
                SetWormInGameScene();
            else if (SceneManager.GetActiveScene().name == "CreatureBuilderScene" && gameObject.activeSelf)
            {
                StartCoroutine(SetWormInCreatureBuilderScene());
            }
        }
        
        private IEnumerator FindAndSetupRemoteWorm()
        {
            hasBeenSetup = true;
            
            yield return new WaitForSeconds(0.5f);
            RefreshSegmentsFromChildren();
            
            float elapsed = 0.5f;
            while (wormBodySegments.Count < wormSegmentCount && elapsed < 3f)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
                if (wormBodySegments.Count == 0) RefreshSegmentsFromChildren();
            }

            if (wormBodySegments.Count < wormSegmentCount)
            {
                Debug.LogError("FindAndSetupRemoteWorm timed out waiting for segments.");
                yield break;
            }

            SetSegmentsKinematic(true);
            yield return null;

            RebuildSegmentReferences();
            var physics = GetComponent<WormPhysics>();
            physics.AddCollidersToSegments();

            wormConstructor = new WormConstructor(wormHead, wormBodySegments, wormSegmentPrefab, transform, wormSegmentCount, maxPartDistance);
            wormConstructor.ConstructWorm();
            yield return null;

            physics.ResetWormPosition();
            SetSegmentsKinematic(false);
        }
        
        private void RefreshSegmentsFromChildren()
        {
            wormBodySegments.Clear();
            foreach (Transform child in transform)
            {
                if (child.GetComponent<CreatureBodySegment>() != null)
                    wormBodySegments.Add(child);
            }
        }
        
        private void SetSegmentsKinematic(bool kinematic)
        {
            var headRb = wormHead.GetComponent<Rigidbody>();
            headRb.isKinematic = kinematic;
            headRb.useGravity = !kinematic;

            foreach (Transform segment in wormBodySegments)
            {
                var rb = segment.GetComponent<Rigidbody>();
                rb.isKinematic = kinematic;
                rb.useGravity = !kinematic;
            }
        }

        private void RebuildSegmentReferences()
        {
            CreaturePart previous = wormHead.GetComponent<CreaturePart>();

            for (int i = 0; i < wormBodySegments.Count; i++)
            {
                var seg = wormBodySegments[i].GetComponent<CreatureBodySegment>();
                seg.previousSegment = previous;

                // Link next segment while we're already iterating
                if (i < wormBodySegments.Count - 1)
                    seg.nextSegment = wormBodySegments[i + 1].GetComponent<CreatureBodySegment>();

                previous = seg;
            }
        }
        
        #endregion
    }
}