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

        public void AddForce(Vector3 impulse)
        {
            networkRigidbody.AddForce(impulse);
        }

        public void AddForceAtPosition(Vector3 force, Vector3 position)
        {
            networkRigidbody.AddForceAtPosition(force, position);
        }

        [ServerRpc(requireOwnership: false)]
        private void ServerHandleCollision(Vector3 impulse)
        {
            networkRigidbody.AddForce(impulse, ForceMode.Impulse);
        }

        [ServerRpc(requireOwnership: false)]
        private void ServerHandleForceAtPosition(Vector3 force, Vector3 position)
        {
            networkRigidbody.AddForceAtPosition(force, position, ForceMode.Impulse);
        }
    } 
}