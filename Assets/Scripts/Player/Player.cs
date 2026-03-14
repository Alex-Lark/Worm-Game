using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CreatureParts;
using JamesFrowen.SimpleWeb;
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
        
        public bool IsWormGrounded { get; set; }
        
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
        
        public WormJump wormJump;
        public WormHeadBut wormHeadBut;
    
        public readonly int WormSegmentCount = GameParameters.WormSegmentCount;
        public readonly float MaxPartDistance = GameParameters.SegmentMaxPartDistance;
        
        #endregion
        
        #region private variables

        private bool isPlayerActive = false;
        
        #endregion
    
        #region Built-In Methods

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
            if (!isOwner) return;
            
            if (wormBodySegments.Any(s => s.gameObject == hitGameObject) || 
                attachedWormParts.Contains(hitGameObject))
            {
                return;
            }
            
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
                Debug.Log("Blunt collision between " + other.gameObject + " and " + hitGameObject);
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
            
            CurrentState = WormState.Dead;
            if (isOwner) currentPlayerHealth = 0;

            thirdPersonCamera.GetComponent<CinemachineBrain>().enabled = false;
            
            playerSpawning.deathScreenUI.EnableDeathUI();
            
            ServerSideDeath();
            
            playerSpawning.TryToRespawn();
        }

        [ServerRpc(requireOwnership: true)]
        private void ServerSideDeath()
        {
            ObserversSideDeath();
        }

        [ObserversRpc(runLocally: true)]
        private void ObserversSideDeath()
        {
            GetComponent<WormRenderer>().enabled = false;
            GetComponent<LineRenderer>().enabled = false;
    
            if (transform.Find("WormMesh") != null)
                Destroy(transform.Find("WormMesh").gameObject);
    
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