using CreatureParts;
using UnityEngine;

namespace Audio
{
    public class SporeProjectileAudio : MonoBehaviour
    {
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
            //projectile hit audio
        }
    }
}
