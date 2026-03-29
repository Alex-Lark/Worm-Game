using CreatureParts;
using UnityEngine;
using WormLeague;

namespace Audio
{
    public class GoalAudio : MonoBehaviour
    {
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
            //goal scored audio
        }
    }
}
