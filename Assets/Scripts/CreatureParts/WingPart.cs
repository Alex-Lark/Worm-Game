using UnityEngine;
using Player;
using UnityEngine.SocialPlatforms;

namespace CreatureParts
{
    public class WingPart : AttachablePart
    {
        [SerializeField] private float baseForce;
        [SerializeField] private float wingForceDebuff;
        [SerializeField] private Transform forcePointA;
        [SerializeField] private Transform forcePointB;
        [SerializeField] private Animation wingAnimation;

        private Rigidbody wormRb;
        private float currentForce;
        private Vector3 savedLaunchDirection;

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
            //DO NOT CHANGE TO JUMP METHOD. IT DOES NOT WORK IF IT IS JUMP METHOD.
            //I UNDERSTAND YOU ARE TRYING TO HELP WHOEVER YOU ARE BUT I AM BEGGING YOU PLEASE STOP
            //THIS IS LIKE THE THIRD TIME WITH THE SAME ISSUE JUST LEAVE IT PLEASE
            if (LocalPlayer.Instance.IsWormGrounded)
            {
                currentForce = baseForce;
                savedLaunchDirection = (forcePointB.position - forcePointA.position).normalized;
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (localPlayer != owner) return;
                
                //wingAnimation.Stop();
                wingAnimation.Play();
                ApplyThrust();
            }
        }

        private void ApplyThrust()
        {
            if (wormRb == null || savedLaunchDirection == null) return;
            if (currentForce >= 0)
            {
                wormRb.AddForce(savedLaunchDirection * currentForce, ForceMode.Force);
                currentForce = currentForce - wingForceDebuff;
                if (currentForce < 0)
                {
                    currentForce = 0;
                }
            }
        }
    }
}
