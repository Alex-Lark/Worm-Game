using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Player;

namespace CreatureParts
{ 
    public class ProjectilePart : AttachablePart 
    { 
        public GameObject projectilePrefab;
        public Transform firePoint;
        public float recoilForce = 0.5f;
        public float fireCooldown = 0.5f;
        public float shootForce = 0.05f;
        public KeyCode shootKey = KeyCode.R;
        [SerializeField] private Animator animator;
        
        public event Action OnCannonShoot;
        
        private float lastFireTime;
        private Rigidbody wormRb;
        
        
        private void Awake()
        {
            if (LocalPlayer.Instance != null)
            {
                wormRb = LocalPlayer.Instance.wormHead.GetComponent<Rigidbody>();
            }
            
            base.Awake();
        }

        void Update()
        {
            if (transform.parent != null &&
                Input.GetKeyDown(shootKey) &&
                Time.time >= lastFireTime + fireCooldown)
            {
                if (animator != null)
                {
                    animator.ResetTrigger("PlayAnimation");
                    animator.SetTrigger("PlayAnimation");
                }
                Shoot();
                lastFireTime = Time.time;
            }
        }

        void Shoot()
        {
            OnCannonShoot?.Invoke();
            
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
