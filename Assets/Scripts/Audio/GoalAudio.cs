using CreatureParts;
using UnityEngine;
using WormLeague;

namespace Audio
{
    public class GoalAudio : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip goalScored;
        
        void Start()
        {
            GetComponent<Goal>().OnGoalScored += OnGoalScored;
        }

        void OnDestroy()
        {
            GetComponent<Goal>().OnGoalScored -= OnGoalScored;
        }

        private void OnGoalScored()
        {
            audioSource.clip = goalScored;
            audioSource.Play();
        }
    }
}
