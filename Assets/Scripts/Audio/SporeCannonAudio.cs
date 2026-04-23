using CreatureParts;
using UnityEngine;

namespace Audio
{
    public class SporeCannonAudio : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip cannonFireAudio;
        
        void Start()
        {
            GetComponent<ProjectilePart>().OnCannonShoot += OnCannonShoot;
        }

        void OnDestroy()
        {
            GetComponent<ProjectilePart>().OnCannonShoot -= OnCannonShoot;
        }

        private void OnCannonShoot()
        {
            audioSource.clip = cannonFireAudio;
            audioSource.Play();
        }
    }
}
