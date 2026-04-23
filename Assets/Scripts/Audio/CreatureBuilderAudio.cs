using UnityEngine;

namespace Audio
{
    public class CreatureBuilderAudio : MonoBehaviour
    {
        public AudioSource audioSource;
        
        void Start()
        {
            GetComponent<CreatureBuilder.CreatureBuilder>().OnPartTo3D += OnPartTo3D;
        }

        void OnDestroy()
        {
            GetComponent<CreatureBuilder.CreatureBuilder>().OnPartTo3D -= OnPartTo3D;
        }

        private void OnPartTo3D()
        {
            audioSource.Play();
        }
    }
}
