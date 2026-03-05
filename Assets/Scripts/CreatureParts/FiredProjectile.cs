using UnityEngine;

namespace CreatureParts
{
    public class FiredProjectile : MonoBehaviour
    {
        private bool canCollide = false;

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

            Destroy(gameObject);
        }
    }
}
