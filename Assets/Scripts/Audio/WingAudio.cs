using CreatureParts;
using UnityEngine;

namespace Audio
{
    public class WingAudio : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip wingAudio;
        
        void Start()
        {
            GetComponent<WingPart>().OnWingFlap += OnWingFlap;
        }

        void OnDestroy()
        {
            GetComponent<WingPart>().OnWingFlap -= OnWingFlap;
        }
        
        private void OnWingFlap()
        {
            audioSource.clip = wingAudio;
            audioSource.Play();
        }
    }
}
