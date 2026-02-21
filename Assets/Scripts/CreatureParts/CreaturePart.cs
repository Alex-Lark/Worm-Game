using System;
using UnityEngine;

namespace CreatureParts
{
    public class CreaturePart : MonoBehaviour
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
        
        private void Awake()
        {
            partCollider = GetComponent<Collider>();
            GroundObject = null;
        }

        protected virtual void FixedUpdate()
        {
            CheckGrounded();
        }

        private void OnCollisionEnter(Collision other)
        {
            float collisionForce = other.impulse.magnitude;
                
            if (other.gameObject.GetComponent<SpikePart>() != null)
            {
                Debug.Log("Hit Spike. force: " + collisionForce);

                if (collisionForce > GameParameters.MinSpikeCollisionForceToDamage)
                {
                    Debug.Log("Large collision force");
                    float damage = collisionForce * GameParameters.SpikeForceToDamageMultiplier;
                    Player.Player.Instance.currentPlayerHealth -= damage;
                }
            }
            else if (collisionForce > GameParameters.MinBluntCollisionForceToDamage)
            {
                Debug.Log("Large blunt collision force: " + collisionForce);
                float damage = collisionForce * GameParameters.BluntForceToDamageMultiplier;
                Player.Player.Instance.currentPlayerHealth -= damage;
            }
        }

        #endregion
        
        #region Public Methods

        public virtual void Jump()
        {
        
        }

        public virtual void MoveForward()
        {
        
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
        
        #endregion
    }
}