using GameLoop;
using UnityEngine;

namespace Audio
{
    public class PartSelectionAudio : MonoBehaviour
    {
        public AudioSource audioSource;
        
        void Start()
        {
            GetComponent<PartSelection>().OnCardSelected += OnCardSelected;
        }

        void OnDisable()
        {
            GetComponent<PartSelection>().OnCardSelected -= OnCardSelected;
        }

        private void OnCardSelected()
        {
            audioSource.Play();
        }
    }
}
