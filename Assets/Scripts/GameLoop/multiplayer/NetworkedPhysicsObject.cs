using System;
using PurrNet;
using Unity.VisualScripting;
using UnityEngine;

namespace GameLoop.multiplayer
{
    public class NetworkedPhysicsObject : NetworkBehaviour
    {
        private NetworkRigidbody networkRigidbody;

        private void Awake()
        {
            networkRigidbody = GetComponent<NetworkRigidbody>();
            GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        }

        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned(asServer);
        }

        // void Update()
        // {
        //     if (Input.GetKey(KeyCode.B))
        //     {
        //         ApplyForce(new Vector3(100, 0, 0));
        //     }
        // }

        public void AddForce(Vector3 impulse)
        {
            networkRigidbody.AddForce(impulse); // Local prediction for all
            
            if (!isServer)
                ServerHandleCollision(impulse);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isSpawned) return;
            
            Vector3 impulse = Vector3.zero;
            foreach (ContactPoint contact in collision.contacts)
            {
                impulse += contact.normal;
            }
            
            impulse = impulse.normalized * collision.relativeVelocity.magnitude;
            AddForce(impulse);
        }
        
        [ServerRpc(requireOwnership: false)]
        private void ServerHandleCollision(Vector3 impulse)
        {
            networkRigidbody.AddForce(impulse);
        }
    }
}
