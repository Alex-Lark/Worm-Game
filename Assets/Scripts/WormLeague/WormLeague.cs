using System.Collections.Generic;
using UnityEngine;

namespace WormLeague
{
    public class WormLeague : MonoBehaviour
    {
        public List<Player> teamBlue;
        public List<Player> teamRed;
    
        void Start()
        {
            AssignPlayerTeams();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    
        private void AssignPlayerTeams()
        {
            List<Player> players = GameLoop.Instance.players;

            while (players.Count > 0)
            {
                int random =  Random.Range(0, players.Count);
                teamRed.Add(players[random]);
                //display team
                players.RemoveAt(random);
                if (players.Count > 0)
                {
                    random =  Random.Range(0, players.Count);
                    teamBlue.Add(players[random]);
                    //display team
                    players.RemoveAt(random);
                }
            }
        }
    }
}
