using System;
using System.Collections;
using System.Collections.Generic;
using CreatureParts;
using PurrNet;
using Unity.VisualScripting;
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

    public class Player : NetworkBehaviour
    {
        
        #region Public Properties
        [Header("Public Properties")]

        public string PlayerName = "Player1";
        
        public WormState CurrentState { get; set; }
        
        public bool IsWormGrounded { get; private set; }
        
        public float MaxVelocity { get; set; }
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

            if (currentPlayerHealth < maxPlayerHealth && CurrentState != WormState.Dead)
            {
                currentPlayerHealth += GameParameters.PlayerHealthRegen;
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
            if (other.gameObject.GetComponent<ShellPart>() != null)
            {
                collisionForce *= GameParameters.ShellDamageReduction;
            }
            if (other.gameObject.GetComponent<SpikePart>() != null)
            {
                if (collisionForce > GameParameters.MinSpikeCollisionForceToDamage)
                {
                    float damage = collisionForce * GameParameters.SpikeForceToDamageMultiplier;
                    currentPlayerHealth -= damage;
                }
            }
            if (other.gameObject.GetComponent<FiredProjectile>() != null)
            {
                if (collisionForce > GameParameters.MinProjectileCollisionForceToDamage)
                {
                    float damage = collisionForce * GameParameters.ProjectileForceToDamageMultiplier;
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
        
        public IEnumerator SetWormInCreatureBuilderScene()
        {
            yield return new WaitForSeconds(0.2f);
            yield return null;
    
            var wormPhysics = GetComponent<WormPhysics>();
            
            wormPhysics.ResetWormPhysics();
            
            yield return null;
            
            wormPhysics.ResetWormOrientation();
            
            wormPhysics.PositionWormSegments(new Vector3(0, 2, 0));
            
            yield return null;
            
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
            
            playerSpawning.deathScreenUI.EnableDeathUI();
            
            ServerSideDeath();
            
            playerSpawning.TryToRespawn();
        }

        [ServerRpc]
        private void ServerSideDeath()
        {
            GetComponent<WormRenderer>().enabled = false;
            GetComponent<LineRenderer>().enabled = false;
            
            if (transform.Find("WormMesh").gameObject)
            {
                Destroy(transform.Find("WormMesh").gameObject);
            }
            
            wormHeadCopy = DuplicatePart(wormHead.gameObject);
            wormHead.gameObject.SetActive(false);

            foreach (Transform bodySegment in wormBodySegments)
            {
                DuplicatePart(bodySegment.gameObject);
                bodySegment.gameObject.SetActive(false);
            }
            
            foreach (GameObject attachedPart in attachedWormParts)
            {
                DuplicatePart(attachedPart);
                attachedPart.gameObject.SetActive(false);
                attachedPart.GetComponent<AttachablePart>().enabled = false;
            }
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
            playerSpawning = GetComponent<PlayerSpawning>();

            wormBodySegments.Clear();
            wormConstructor = new WormConstructor(wormHead, wormBodySegments, wormSegmentPrefab, transform, wormSegmentCount, maxPartDistance);
            wormConstructor.CreateWormSegments();
            wormConstructor.ConstructWorm();
            GetComponent<WormPhysics>().AddCollidersToSegments();
            GetComponent<WormPhysics>().ResetWormPhysics();

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