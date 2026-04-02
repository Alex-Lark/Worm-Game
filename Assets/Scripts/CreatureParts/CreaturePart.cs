using System;
using System.Collections;
using System.Linq;
using Player;
using PurrNet;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CreatureParts
{
    public class CreaturePart : NetworkBehaviour
    {
        #region Public Variables
        [Header("Public Variables")]
        
        public bool IsGrounded { get; private set; }
        public GameObject GroundObject { get; private set; }
        public Vector3 GroundNormal { get; private set; }
        public float TimeSinceLastGrounded { get; private set; }
        
        #endregion

        #region Private Variables
        [Header("Private Variables")]
        
        private Collider partCollider;
    
        private readonly Collider[] results = new Collider[GameParameters.GroundColliderMaxHeldCollisions];
        private readonly float verticalDetectionOffset = GameParameters.GroundingColliderVerticalDetectionOffset;
        private readonly float detectionRadiusScale = GameParameters.GroundColliderDetectionRadiusScale;
        
        #endregion

        #region Built-In Methods
        
        protected void Awake()
        {
            partCollider = GetComponent<Collider>();
            GroundObject = null;
        }

        protected virtual void FixedUpdate()
        {
            CheckGrounded();
        }
        
        protected override void OnSpawned(bool asServer)
        {
            if (!asServer) return;
            
            if (owner != null)
            {
                return;
            }

            var parentPlayer = GetComponentInParent<Player.Player>();

            if (parentPlayer == null)
            {
                return;
            }

            if (parentPlayer.owner.HasValue)
            {
                GiveOwnership(parentPlayer.owner.Value);
            }
            else
            {
                StartCoroutine(WaitForParentAndClaimOwnership());
            }
        }

        private IEnumerator WaitForParentAndClaimOwnership()
        {
            Player.Player parentPlayer = null;
            PlayerID? parentOwner = null;

            float elapsed = 0f;
            while (elapsed < 3f)
            {
                parentPlayer = GetComponentInParent<Player.Player>();

                if (parentPlayer != null && parentPlayer.owner.HasValue)
                {
                    parentOwner = parentPlayer.owner;
                    break;
                }

                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (parentOwner == null)
            {
                yield break;
            }

            if (owner == null)
            {
                GiveOwnership(parentOwner.Value);
            }
        }

        protected override void OnOwnerChanged(PlayerID? previousOwner, PlayerID? newOwner, bool asServer)
        {
            Debug.Log($"[WormSegment] OnOwnerChanged '{gameObject.name}' | asServer={asServer} | prev={previousOwner} | new={newOwner}");
        }

        #endregion
        
        #region Public Methods

        public virtual void Jump()
        {
        
        }

        public virtual void MoveForward()
        {
        
        }
        
        public void SetMaterial(Material material)
        {
            GetComponent<MeshRenderer>().material = material;
        }
        
        #endregion
        
        #region Private Methods

        private void CheckGrounded()
        {
            Vector3 bottom = partCollider.bounds.center - new Vector3(0, partCollider.bounds.extents.y, 0);
            Vector3 checkPos = bottom + Vector3.down * verticalDetectionOffset;
            float radius = partCollider.bounds.extents.x * detectionRadiusScale;
        
            int hitCount = Physics.OverlapSphereNonAlloc(checkPos, radius, results, ~0, QueryTriggerInteraction.Ignore);

            IsGrounded = false;
            GroundObject = null;
            GroundNormal = Vector3.up;
        
            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = results[i];
                if (hit.transform.root != transform.root)
                {
                    IsGrounded = true;
                    GroundObject = hit.gameObject;
                    GroundNormal = GetGroundNormal(hit);
                
                    break;
                }
            }
        }
    
        private Vector3 GetGroundNormal(Collider groundCollider)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
            {
                if (hit.collider == groundCollider)
                {
                    TimeSinceLastGrounded = 0f;
                    return hit.normal;
                }
            }

            TimeSinceLastGrounded += Time.fixedDeltaTime;
            return Vector3.up;
        }
        
        private void OnCollisionEnter(Collision other)
        {
            if (LocalPlayer.Instance == null) return;

            if (!isOwner) return; 
            
            if ((other.gameObject.GetComponent<CreaturePart>() != null) &&
                (other.gameObject.GetComponent<CreaturePart>().owner != owner))
            {
                Player.Player thisPlayer = gameObject.GetComponentInParent<Player.Player>();
                Player.Player otherPlayer = other.gameObject.GetComponentInParent<Player.Player>();
                //Debug.Log("collision with other player. This player: " + thisPlayer.PlayerName + " " + gameObject.name + " Other player: " + otherPlayer.PlayerName + " " + other.gameObject.name + " force: " + other.impulse.magnitude);
            }

            LocalPlayer.Instance.DamagePlayer(other, gameObject);
        }
        
        #endregion
    }
}