using PurrNet;
using Unity.VisualScripting;
using UnityEngine;

namespace GameLoop.multiplayer
{
    public class NetworkedPhysicsObject : NetworkBehaviour
    {
        private Rigidbody rigibody;
        
        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned(asServer);
            rigibody = GetComponent<Rigidbody>();
            rigibody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void ApplyForce(Vector3 impulse)
        {
            rigibody.AddForce(impulse, ForceMode.Impulse); // Local prediction for all
            
            if (!isServer)
                ServerHandleCollision(impulse);
        }

        private void OnCollisionEnter(Collision collision)
        {
            Vector3 impulse = Vector3.zero;
            foreach (ContactPoint contact in collision.contacts)
            {
                impulse += contact.normal;
            }
            
            impulse = impulse.normalized * collision.relativeVelocity.magnitude;
            ApplyForce(impulse);
        }
        
        [ServerRpc(requireOwnership: false)]
        private void ServerHandleCollision(Vector3 impulse)
        {
            rigibody.AddForce(impulse, ForceMode.Impulse);
        }
    }
}
