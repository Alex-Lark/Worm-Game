using UnityEngine;
using UnityEngine.SceneManagement;
using Player;

namespace CreatureParts
{ 
    public class ProjectilePart : AttachablePart 
    { 
        public GameObject projectilePrefab;
        public Transform firePoint;
        public float shootForce = 20f;
        public KeyCode shootKey = KeyCode.R;

        void Update()
        {
            if (transform.parent != null && Input.GetKeyDown(shootKey))
            {
                Shoot();
            }
        }

        void Shoot()
        {
            GameObject projectile = Instantiate(
                projectilePrefab,
                firePoint.position,
                firePoint.rotation
            );

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.AddForce(firePoint.forward * shootForce, ForceMode.Impulse);
        }
    }
}
