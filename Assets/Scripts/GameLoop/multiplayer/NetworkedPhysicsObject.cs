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
            
            if (asServer) //if this is being called from the server creation
            {
                return;
            }

            if (!isServer)
            {
                gameObject.AddComponent<NetworkRigidbody>();
                rigibody.isKinematic = true;
            }
        }

        public void ApplyForce(Vector3 impulse)
        {
            if (isServer)
            {
                ApplyForceAsServer(impulse);
            }
            else
            {
                ServerHandleCollision(impulse);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (isServer) return;
            
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
            ApplyForceAsServer(impulse);
        }
        
        //this method should only ever be called from the server
        private void ApplyForceAsServer(Vector3 impulse)
        {
            if (!isServer)
            {
                return;
            }
            
            rigibody.AddForce(impulse, ForceMode.Impulse);
        }
    }
}
