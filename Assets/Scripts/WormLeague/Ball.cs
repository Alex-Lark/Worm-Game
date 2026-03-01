 using System;
 using PurrNet;
 using UnityEngine;

namespace WormLeague
{
    public class Ball : NetworkBehaviour
    {
        public Player.Player LastTouchingPlayer { get; private set; }

        public void Reset()
        {
            Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
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
                if (LastTouchingPlayer == null) return;
                
                Vector3 impulse = Vector3.zero;
                foreach (ContactPoint contact in collision.contacts)
                    impulse += contact.normal;
            
                impulse = -impulse.normalized * collision.relativeVelocity.magnitude;

                // Send collision to server
                if (isServer)
                {
                    ApplyCollisionForce(impulse, LastTouchingPlayer.PlayerName);
                }
                else
                {
                    ServerHandleCollision(impulse, LastTouchingPlayer.PlayerName);
                }
            }
        }
        
        [ServerRpc(requireOwnership: false)]
        private void ServerHandleCollision(Vector3 impulse, string playerName)
        {
            ApplyCollisionForce(impulse, playerName);
        }
        
        private void ApplyCollisionForce(Vector3 impulse, string playerName)
        {
            GetComponent<Rigidbody>().AddForce(impulse, ForceMode.Impulse);
            //UpdateLastTouchingPlayer(playerName);
        }
    }
}
