using PurrNet;
using Unity.VisualScripting;
using UnityEngine;

namespace GameLoop.multiplayer
{
    public class NetworkedPhysicsObject : NetworkBehaviour
    {
        private NetworkRigidbody networkRigidbody;
        
        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned(asServer);
            networkRigidbody = GetComponent<NetworkRigidbody>();
            GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.B))
            {
                ApplyForce(new Vector3(100, 0, 0));
            }
        }

        public void ApplyForce(Vector3 impulse)
        {
            networkRigidbody.AddForce(impulse); // Local prediction for all
            
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
            networkRigidbody.AddForce(impulse);
        }
    }
}
