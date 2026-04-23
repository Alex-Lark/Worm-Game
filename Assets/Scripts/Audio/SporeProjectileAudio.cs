using CreatureParts;
using UnityEngine;

namespace Audio
{
    public class SporeProjectileAudio : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip projectileHitAudio;
        
        void Start()
        {
            GetComponent<FiredProjectile>().OnProjectileHit += OnProjectileHit;
        }

        void OnDestroy()
        {
            GetComponent<FiredProjectile>().OnProjectileHit -= OnProjectileHit;
        }

        private void OnProjectileHit()
        {
            audioSource.clip = projectileHitAudio;
            audioSource.Play();
        }
    }
}
