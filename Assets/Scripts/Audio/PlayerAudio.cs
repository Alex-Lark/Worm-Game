using Player;
using PurrNet;
using UnityEngine;

namespace Audio
{
    public class PlayerAudio : MonoBehaviour
    {
        private Player.Player player;
        
        void Start()
        {
            player = GetComponent<Player.Player>();
            
            player.OnWormMoveForwardStart += OnWormForwardStart;
            player.OnWormMoveForwardEnd += OnWormForwardEnd;
            player.OnWormJump += OnWormJump;
            player.OnWormHeadbutCharge += OnWormHeadbutCharge;
            player.OnWormHeadbutLaunch += OnWormHeadbutLaunch;
            player.OnWormHeadbutHitBall += OnWormHeadbutHitBall;
            player.OnWormHeadbutHitPlayer += OnWormHeadbutHitPlayer;
            player.OnWormHeadbutHitShell += OnWormHeadbutHitShell;
            player.OnWormHeadbutHitOther += OnWormHeadbutHitOther;
            player.OnWormDeath += OnWormDeath;
            
            GetComponent<PlayerSpawning>().OnWormRespawn += OnWormRespawn;
        }

        void OnDestroy()
        {
            player = GetComponent<Player.Player>();
            
            player.OnWormMoveForwardStart -= OnWormForwardStart;
            player.OnWormMoveForwardEnd -= OnWormForwardEnd;
            player.OnWormJump -= OnWormJump;
            player.OnWormHeadbutCharge -= OnWormHeadbutCharge;
            player.OnWormHeadbutLaunch -= OnWormHeadbutLaunch;
            player.OnWormHeadbutHitBall -= OnWormHeadbutHitBall;
            player.OnWormHeadbutHitPlayer -= OnWormHeadbutHitPlayer;
            player.OnWormHeadbutHitShell -= OnWormHeadbutHitShell;
            player.OnWormHeadbutHitOther -= OnWormHeadbutHitOther;
            
            player.OnWormDeath -= OnWormDeath;
            
            GetComponent<PlayerSpawning>().OnWormRespawn -= OnWormRespawn;
        }

        private void OnWormRespawn()
        {
            //respawn sound effect
        }

        private void OnWormDeath()
        {
            //death sound effect
        }

        private void OnWormHeadbutHitOther()
        {
            //stop whooshing headbut sound, play hitOther sound
        }

        private void OnWormHeadbutHitShell()
        {
            //stop whoosing headbut sound, play hitshell sound
        }

        private void OnWormHeadbutHitPlayer()
        {
            //stop whooshing headbut sound, play hitPlayer sound
        }

        private void OnWormHeadbutHitBall(Vector3 vector3)
        {
            //stop whooshing headbut sound, play hitBall sound
        }

        private void OnWormHeadbutLaunch()
        {
            //worm headbut launch sound effect
        }

        private void OnWormHeadbutCharge()
        {
            //worm headbut charge sound effect
        }

        private void OnWormForwardStart()
        {
            //worm forward movement sound effects, possibly using speed and amount of grounded segments for variation
        }
        
        private void OnWormForwardEnd()
        {
            //stop movement sound effects
        }
        
        private void OnWormJump()
        {
            // use isWormGroundedbysegments to determine jump sound effect
        }
    }
}
