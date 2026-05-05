using System;
using PurrNet;
using PurrNet.Prediction;
using Unity.VisualScripting;
using UnityEngine;

namespace GameLoop.multiplayer
{
    public class NetworkedPhysicsObject : NetworkBehaviour
    {
        private PredictedRigidbody networkRigidbody;

        private void Awake()
        {
            networkRigidbody = GetComponent<PredictedRigidbody>();
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