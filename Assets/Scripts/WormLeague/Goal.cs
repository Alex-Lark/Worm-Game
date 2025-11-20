using UnityEngine;

namespace WormLeague
{
    public class Goal : MonoBehaviour
    {
        public global::WormLeague.WormLeague wormLeague;
        public string team;
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("ball"))
            {
                wormLeague.OnGoalScored(team);
            }
        }
    }
}
