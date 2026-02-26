using UnityEngine;
using Player;

namespace CreatureParts
{
    public class WingPart : MonoBehaviour
    {
        [SerializeField] private float baseForce;
        [SerializeField] private float wingForceDebuff;
        [SerializeField] private Transform sourcePoint;

        private Rigidbody wormRb;
        private float currentForce;

        private void Awake()
        {
            if (Player.Player.Instance != null)
            {
                wormRb = Player.Player.Instance.wormHead.GetComponent<Rigidbody>();
            }
        }

        private void Update()
        {
            if (Player.Player.Instance == null) return;
            
            if (Player.Player.Instance.IsWormGrounded)
            {
                currentForce = baseForce;
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ApplyThrust();
            }
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
