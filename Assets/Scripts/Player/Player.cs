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
        public SyncList<Transform> wormBodySegments = new(false);
        public List<GameObject> attachedWormParts;
        public List<GameObject> wormPartsInInventory;

        public PlayerSpawning playerSpawning;
        public WormForwardMovement wormForwardMovement;
        
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
            PlayerRegister.Players.TryGetValue(playerID, out RegisterData);
            //Debug.Log(RegisterData.name);
            
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
            
            if (wormBodySegments.Any(s => s.gameObject == hitGameObject) || 
                attachedWormParts.Contains(hitGameObject))
            {
                return;
            }
            
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
                Debug.Log($"Blunt collision between {hitGameObject.name} and {other.gameObject.name} with force: {collisionForce}", other.gameObject);
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
            DisablePartForDeath(wormHead.gameObject);

            if (wormVisualHead.GetComponent<MeshRenderer>() != null) wormVisualHead.GetComponent<MeshRenderer>().enabled = false;
            GameObject visualHeadWithMaterial = wormHead.GetComponent<WormHead>().wormVisualHeadWithMaterial;
            for (int i = 0; i < visualHeadWithMaterial.transform.childCount; i++)
            {
                GameObject childGameobject = visualHeadWithMaterial.transform.GetChild(i).gameObject;
                if (childGameobject.GetComponent<MeshRenderer>() != null)
                {
                    childGameobject.GetComponent<MeshRenderer>().enabled = false;
                }
            }

            foreach (Transform bodySegment in wormBodySegments)
            {
                DuplicatePartForDeath(bodySegment.gameObject);
                DisablePartForDeath(bodySegment.gameObject);
            }
    
            foreach (GameObject attachedPart in attachedWormParts)
            {
                DuplicatePartForDeath(attachedPart);
                attachedPart.SetActive(false);
            }

            if (isOwner && owner == localPlayer)
            {
                playerSpawning.SetKinematicStateServer(true, this);
            }
        }

        private void DisablePartForDeath(GameObject part)
        {
            MeshRenderer meshrenderer = part.GetComponent<MeshRenderer>();
            Rigidbody rigidbody = part.GetComponent<Rigidbody>();
            Collider collider = part.GetComponent<Collider>();
            CreaturePart creaturePart = part.GetComponent<CreaturePart>();
            CreatureBodySegment creatureBodySegment = part.GetComponent<CreatureBodySegment>();
            AttachablePart attachablePart = part.GetComponent<AttachablePart>();

            if (meshrenderer != null)
            {
                meshrenderer.enabled = false;
            }

            if (collider != null)
            {
                collider.enabled = false;
            }

            if (creaturePart != null)
            {
                creaturePart.enabled = false;
            }

            if (creatureBodySegment != null)
            {
                creatureBodySegment.visualBodySegment.SetActive(false);
            }

            if (attachablePart != null)
            {
                for (int i = 0; i < part.transform.childCount; i++)
                {
                    GameObject childGameobject = part.transform.GetChild(i).gameObject;
                    if (childGameobject.GetComponent<MeshRenderer>() != null)
                    {
                        childGameobject.GetComponent<MeshRenderer>().enabled = false;
                    }
                    if (childGameobject.GetComponent<Collider>() != null)
                    {
                        childGameobject.GetComponent<Collider>().enabled = false;
                    }
                }
            }
        }
        
        public void EnablePartForRespawn(GameObject part)
        {
            MeshRenderer meshrenderer = part.GetComponent<MeshRenderer>();
            Collider collider = part.GetComponent<Collider>();
            CreaturePart creaturePart = part.GetComponent<CreaturePart>();
            CreatureBodySegment creatureBodySegment = part.GetComponent<CreatureBodySegment>();
            AttachablePart attachablePart = part.GetComponent<AttachablePart>();

            if (meshrenderer != null)
            {
                meshrenderer.enabled = true;
            }

            if (collider != null)
            {
                collider.enabled = true;
            }

            if (creaturePart != null)
            {
                creaturePart.enabled = true;
            }
            
            if (creatureBodySegment != null)
            {
                creatureBodySegment.visualBodySegment.SetActive(true);
            }
            
            if (attachablePart != null)
            {
                for (int i = 0; i < part.transform.childCount; i++)
                {
                    GameObject childGameobject = part.transform.GetChild(i).gameObject;
                    if (childGameobject.GetComponent<MeshRenderer>() != null)
                    {
                        childGameobject.GetComponent<MeshRenderer>().enabled = true;
                    }
                    if (childGameobject.GetComponent<Collider>() != null)
                    {
                        childGameobject.GetComponent<Collider>().enabled = true;
                    }
                }
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
                copyRb.isKinematic = false;
                copyRb.useGravity = true;
                copyRb.linearVelocity = originalRb.linearVelocity * GameParameters.DeadPartVelocityMultiplier;
                copyRb.angularVelocity = originalRb.angularVelocity * GameParameters.DeadPartVelocityMultiplier;
            }
            
            if (!original.TryGetComponent<AttachablePart>(out _))
            {
                if (copy.TryGetComponent<CreatureBodySegment>(out var segment))
                {
                    segment.SetMaterial(DeadBodyPartMaterial);
                    segment.visualBodySegment.GetComponent<MeshRenderer>().enabled = true;
                }
                else
                    foreach (MeshRenderer renderer in copy.GetComponentsInChildren<MeshRenderer>())
                        renderer.material = DeadBodyPartMaterial;
            }
            
            foreach (Joint joint in copy.GetComponents<Joint>())
                Destroy(joint);
            foreach (Joint joint in copy.GetComponentsInChildren<Joint>())
                Destroy(joint);

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