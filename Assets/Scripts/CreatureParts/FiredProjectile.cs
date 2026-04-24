using System;
using UnityEngine;

namespace CreatureParts
{
    public class FiredProjectile : MonoBehaviour
    {
        private bool canCollide = false;
        public GameObject firingPlayer;
        
        public event Action OnProjectileHit;

        void Start()
        {
            Invoke(nameof(EnableCollision), 0.05f); //50 milliseconds
            Destroy(gameObject, 5f);
        }

        void EnableCollision()
        {
            canCollide = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!canCollide) return;

            if (collision.gameObject.GetComponent<CreaturePart>() != null)
            {
                OnProjectileHit?.Invoke();
            }
            
            Destroy(gameObject);
        }
    }
}
