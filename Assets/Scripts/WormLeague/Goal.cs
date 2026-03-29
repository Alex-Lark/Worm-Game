using System;
using UnityEngine;

namespace WormLeague
{
    public class Goal : MonoBehaviour
    {
        public WormLeague wormLeague;
        public string team;
        
        public event Action OnGoalScored;
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("ball"))
            {
                OnGoalScored?.Invoke();
                
                wormLeague.OnGoalScored(team);
            }
        }
    }
}
