using UnityEngine;

namespace WormLeague
{
    public class Ball : MonoBehaviour
    {
        public Player lastTouchingPlayer { get; private set; }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("CreaturePart") || collision.gameObject.CompareTag("WormBodySegment"))
            {
                Debug.Log("Ball touching player.");
                lastTouchingPlayer = collision.gameObject.GetComponentInParent<Player>();
                print(lastTouchingPlayer.PlayerName);
            }
        }
    }
}
