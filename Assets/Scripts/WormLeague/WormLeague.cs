using System.Collections.Generic;
using UnityEngine;

namespace WormLeague
{
    public class WormLeague : MonoBehaviour
    {
        public WormLeagueUI wormLeagueUI;

        public Ball ball;
        
        private List<Player> teamBlue = new List<Player>();
        private List<Player> teamRed = new List<Player>();
    
        void Start()
        {
            AssignPlayerTeams();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void OnGoalScored(string team)
        {
            if (team == "blue")
            {
                OnRedGoal();
            }
            else if (team == "red")
            {
                OnBlueGoal();
            }

            Player scoringPlayer = ball.lastTouchingPlayer;
            print(scoringPlayer.PlayerName + "scored");
            //get scoring player from ball
            //reset ball to center
            //update ui
        }

        public void OnRedGoal()
        {
            print("red scored");
        }

        public void OnBlueGoal()
        {
            print("blue scored");
        }
    
        private void AssignPlayerTeams()
        {
            List<Player> players = GameLoop.Instance.players;

            while (players.Count > 0)
            {
                print("assigning player to red");
                int random =  Random.Range(0, players.Count);
                teamRed.Add(players[random]);
                wormLeagueUI.SetTeam("red");
                players.RemoveAt(random);
                if (players.Count > 0)
                {
                    random =  Random.Range(0, players.Count);
                    teamBlue.Add(players[random]);
                    wormLeagueUI.SetTeam("blue");
                    players.RemoveAt(random);
                }
            }
        }
    }
}
