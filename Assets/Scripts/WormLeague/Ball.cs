 using System;
using UnityEngine;

namespace WormLeague
{
    public class Ball : MonoBehaviour
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
                print(LastTouchingPlayer.PlayerName);
            }
        }
    }
}
