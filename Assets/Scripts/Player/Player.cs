using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CreatureParts;
using PurrNet;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using WormLeague;

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

        public string PlayerName = "";

        public PlayerID playerID;
        
        public WormState CurrentState { get; set; }
        
        public bool IsWormGrounded { get; set; }
        public bool IsWormGroundedBySegments { get; set; }
        public float MaxVelocity { get; set; }
        public bool IsWormJumping => CurrentState == WormState.Jumping;
        public bool IsWormAttacking => CurrentState == WormState.Attacking;
        public bool IsWormInAttackCooldown => CurrentState == WormState.AttackCooldown;

        public bool canDie = false;
        
        #endregion
        
        #region public variables

        public PlayerRegister.PlayerData RegisterData;
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
        
        public WormJump wormJump;
        public WormHeadBut wormHeadBut;
    
        public readonly int WormSegmentCount = GameParameters.WormSegmentCount;
        public readonly float MaxPartDistance = GameParameters.SegmentMaxPartDistance;

        public string playerTeam;
        
        public Material DeadBodyPartMaterial;
        
        #endregion
        
        #region Events
        
        public event Action<string> OnPlayerTeamChanged;
        public event Action OnWormMoveForwardStart;
        public event Action OnWormMoveForwardEnd;
        public event Action OnWormJump;
        
        public event Action OnWormHeadbutCharge;
        
        public event Action OnWormHeadbutLaunch;
        
        public event Action OnWormHeadbutHitBall;
        
        public event Action OnWormHeadbutHitPlayer;
        
        public event Action OnWormHeadbutHitShell;
        public event Action OnWormHeadbutHitOther;
        
        public event Action OnWormDeath;
        
        #endregion
        
        #region private variables

        private bool isPlayerActive = false;
        
        #endregion
    
        #region Built-In Methods
        
        
        private void FixedUpdate()
        {
            RegisterData = PlayerRegister.Players[playerID];
            
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

            if (isOwner)
            {
                if (currentPlayerHealth < maxPlayerHealth && CurrentState != WormState.Dead)
                {
                    currentPlayerHealth += GameParameters.PlayerHealthRegen;
                }
            }
        }
        
        #endregion

        #region Public Methods
        
        public void ActivatePlayer() => isPlayerActive = true;
        public void DeactivatePlayer() => isPlayerActive = false;
        
        public void SetPlayernameFromLobby(string username, PlayerID playerID)
        {
            if (playerID == owner)
            {
                PlayerName = username;
            }
        }
        
        public void StartWormMoving()
        {
            if (CurrentState == WormState.Dead) return;
            CurrentState = WormState.Moving;
            
            OnWormMoveForwardStart?.Invoke();
        }
        
        public void StopWormMoving()
        {
            if (CurrentState == WormState.Dead) return;
            CurrentState = WormState.Idle;
            
            OnWormMoveForwardEnd?.Invoke();
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
            if (!isOwner) return;
            
            float collisionForce = other.impulse.magnitude;
            
            if (other.gameObject.CompareTag("Untagged"))
            {
                //okay so the source of the mystery damage was in fact walls/ground and not the wings
                //for now i've just turned it off but if we want fall damage we can do multipliers here
                //return;
            }

            if (hitGameObject.GetComponent<WormHead>() != null)
            {
                if ((CurrentState == WormState.Attacking || CurrentState == WormState.AttackCooldown))
                {
                    collisionForce *= GameParameters.HeadbutDamageReductionOnHead;

                    if (other.gameObject.GetComponent<Ball>() != null)
                    {
                        OnWormHeadbutHitBall?.Invoke();
                    }
                    else if (other.gameObject.GetComponent<ShellPart>() != null)
                    {
                        OnWormHeadbutHitShell?.Invoke();
                    }
                    else if (other.gameObject.GetComponent<CreaturePart>() != null)
                    {
                        OnWormHeadbutHitPlayer?.Invoke();
                    }
                    else
                    {
                        OnWormHeadbutHitOther?.Invoke();
                    }
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
                    if (isOwner) currentPlayerHealth -= damage;
                }
            }
            if (other.gameObject.GetComponent<FiredProjectile>() != null)
            {
                if (collisionForce > GameParameters.MinProjectileCollisionForceToDamage)
                {
                    float damage = collisionForce * GameParameters.ProjectileForceToDamageMultiplier;
                    if (isOwner) currentPlayerHealth -= damage;
                }
            }
            else if (collisionForce > GameParameters.MinBluntCollisionForceToDamage)
            {
                Debug.Log("Blunt collision between " + hitGameObject  + " and " + other.gameObject + " with force: " + collisionForce);
                float damage = collisionForce * GameParameters.BluntForceToDamageMultiplier;
                if (LocalPlayer.Instance == this) currentPlayerHealth -= damage;
            }

            if (isOwner)
            {
                if (CurrentState != WormState.Dead && currentPlayerHealth < 0)
                {
                    OnPlayerDeath();
                }
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
            
            OnWormJump?.Invoke();
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
            Debug.Log("Player died");
            if (!GameSceneList.IsSceneAGameScene(SceneManager.GetActiveScene().name))
            {
                return;
            }
            if (CurrentState == WormState.Dead) return;
            if (!canDie)
            {
                return;
            }
            
            OnWormDeath?.Invoke();
            
            CurrentState = WormState.Dead;
            if (isOwner) currentPlayerHealth = 0;

            thirdPersonCamera.GetComponent<CinemachineBrain>().enabled = false;

            if (playerSpawning.deathScreenUI != null)
            {
                playerSpawning.deathScreenUI.EnableDeathUI();
            }
            else
            {
                playerSpawning.deathScreenUI = FindFirstObjectByType<DeathScreenUI>();
            }
            
            ServerSideDeath();
            
            playerSpawning.TryToRespawn();
        }

        public void SetColor(Material bodyMaterial, Material headMaterial, Material deadMaterial)
        {
            wormHead.GetComponent<WormHead>().SetMaterial(headMaterial);
            DeadBodyPartMaterial = deadMaterial;

            foreach (GameObject wormSegment in wormPartsInInventory)
            {
                wormSegment.GetComponent<CreatureBodySegment>().SetMaterial(bodyMaterial);
            }
            
            GetComponent<WormRenderer>().SetMaterial(bodyMaterial);
        }

        [ServerRpc(requireOwnership: true)]
        private void ServerSideDeath()
        {
            ObserversSideDeath();
        }

        public void SetPlayerTeam(string team)
        {
            playerTeam = team;
            OnPlayerTeamChanged?.Invoke(team);
            Debug.Log("setPlayerTeamCalled with team ");
        }
        
        #endregion
        
        #region Private Methods
        
        [ObserversRpc(runLocally: true)]
        private void ObserversSideDeath()
        {
            GetComponent<WormRenderer>().enabled = false;
            GetComponent<LineRenderer>().enabled = false;
    
            if (transform.Find("WormMesh") != null)
                Destroy(transform.Find("WormMesh").gameObject);
    
            wormHeadCopy = DuplicatePartForDeath(wormHead.gameObject);
            wormHead.gameObject.SetActive(false);

            foreach (Transform bodySegment in wormBodySegments)
            {
                DuplicatePartForDeath(bodySegment.gameObject);
                bodySegment.gameObject.SetActive(false);
            }
    
            foreach (GameObject attachedPart in attachedWormParts)
            {
                DuplicatePartForDeath(attachedPart);
                attachedPart.gameObject.SetActive(false);
                attachedPart.GetComponent<AttachablePart>().enabled = false;
            }
        }
        
        private GameObject DuplicatePartForDeath(GameObject original)
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

            if (!original.TryGetComponent<AttachablePart>(out _))
            {
                if (copy.TryGetComponent<CreatureBodySegment>(out var segment))
                    segment.SetMaterial(DeadBodyPartMaterial);
                else
                    foreach (MeshRenderer renderer in copy.GetComponentsInChildren<MeshRenderer>())
                        renderer.material = DeadBodyPartMaterial;
            }

            return copy;
        }
        
        private IEnumerator AttackSequence()
        {
            OnWormHeadbutCharge?.Invoke();
            yield return new WaitForSeconds(GameParameters.WormHeadbutTime);
            CurrentState = WormState.AttackCooldown;
            wormHeadBut.EndHeadBut();
            OnWormHeadbutLaunch?.Invoke();
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
                    IsWormGroundedBySegments = true;
                    return;
                }
            }

            IsWormGroundedBySegments = false;

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