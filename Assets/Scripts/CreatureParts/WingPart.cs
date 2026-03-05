using UnityEngine;
using Player;
using UnityEngine.SocialPlatforms;

namespace CreatureParts
{
    public class WingPart : AttachablePart
    {
        [SerializeField] private float baseForce;
        [SerializeField] private float wingForceDebuff;
        [SerializeField] private Transform sourcePoint;

        private Rigidbody wormRb;
        private float currentForce;

        private void Awake()
        {
            if (LocalPlayer.Instance != null)
            {
                wormRb = LocalPlayer.Instance.wormHead.GetComponent<Rigidbody>();
            }
            
            base.Awake();
        }

        public override void Jump()
        {
            if (LocalPlayer.Instance.IsWormGrounded)
            {
                currentForce = baseForce;
            }
            
            ApplyThrust();
        }

        private void ApplyThrust()
        {
            if (wormRb == null || sourcePoint == null) return;
            if (currentForce >= 0)
            {
                wormRb.AddForce(sourcePoint.forward * currentForce, ForceMode.Force);
                currentForce = currentForce - wingForceDebuff;
                if (currentForce < 0)
                {
                    currentForce = 0;
                }
            }
        }
    }
}
