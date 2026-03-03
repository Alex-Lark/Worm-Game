using System;
using GameLoop.multiplayer;
using PurrNet;
using UnityEngine;

namespace WormLeague
{
    public class Ball : NetworkBehaviour
    {
        public Player.Player LastTouchingPlayer { get; private set; }

        public void Reset()
        {
            NetworkRigidbody rigidBody = gameObject.GetComponent<NetworkRigidbody>();
            rigidBody.angularVelocity = new Vector3(0,0,0);
            rigidBody.linearVelocity = new Vector3(0,0,0);
            rigidBody.rotation = Quaternion.identity;
            gameObject.transform.position = new Vector3(0,2,0);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("CreaturePart") || collision.gameObject.CompareTag("WormBodySegment"))
            {
                LastTouchingPlayer = collision.gameObject.GetComponentInParent<Player.Player>();
            }
        }
        
        [ServerRpc]
        public void ApplyForceToObject(NetworkedPhysicsObject obj, Vector3 force, Vector3 position)
        {
            obj.AddForceAtPosition(force, position);
        }
    }
}