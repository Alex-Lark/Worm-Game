using UnityEngine;
using UnityEngine.SceneManagement;
using Player;

namespace CreatureParts
{ 
    public class ProjectilePart : AttachablePart 
    { 
        public GameObject projectilePrefab;
        public Transform firePoint;
        public float recoilForce = 1000f;
        public float fireCooldown = 0.5f;
        public float shootForce = 20f;
        public KeyCode shootKey = KeyCode.R;
        
        private float lastFireTime;
        private Rigidbody wormRb;
        
        private void Awake()
        {
            if (Player.Player.Instance != null)
            {
                wormRb = Player.Player.Instance.wormHead.GetComponent<Rigidbody>();
            }
            
            base.Awake();
        }

        void Update()
        {
            if (transform.parent != null &&
                Input.GetKeyDown(shootKey) &&
                Time.time >= lastFireTime + fireCooldown)
            {
                Shoot();
                lastFireTime = Time.time;
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

            if (rb != null)
            {
                rb.AddForce(firePoint.forward * shootForce, ForceMode.Impulse);
            }

            ApplyRecoil();
        }
        
        void ApplyRecoil()
        {
            if (wormRb == null) return;
            print("yeet");

            Vector3 recoilDirection = -firePoint.forward;
            wormRb.AddForce(recoilDirection * recoilForce, ForceMode.Impulse);
        }
    }
}
