using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

namespace WormLeague
{
    public class Goal : NetworkBehaviour
    {
        public WormLeague wormLeague;
        public string team;

        public List<GameObject> particlePrefabs;
        public GameObject particlePoint;
        
        public event Action OnGoalScored;
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("ball"))
            {
                OnGoalScored?.Invoke();
                
                wormLeague.OnGoalScored(team);

                GoalScoredParticlesServerRpc();
            }
        }
        
        [ServerRpc]
        public void GoalScoredParticlesServerRpc()
        {
            GoalScoredParticlesObserverRpc();
        }
        
        [ObserversRpc]
        public void GoalScoredParticlesObserverRpc()
        {
            foreach (GameObject particlePrefab in particlePrefabs)
            {
                Instantiate(particlePrefab, particlePoint.transform);
            }
        }
    }
}
