using System.Collections.Generic;
using UnityEngine;

namespace WormLeague
{
    public class WormLeague : MonoBehaviour
    {
        public WormLeagueUI wormLeagueUI;

        public Ball ball;

        public int teamRedScore = 0;
        public int teamBlueScore = 0;
        
        private List<Player> teamBlue = new List<Player>();
        private List<Player> teamRed = new List<Player>();
    
        void Start()
        {
            AssignPlayerTeams();
        }

        public void OnGoalScored(string team)
        {
            Player scoringPlayer = ball.lastTouchingPlayer;
            scoringPlayer.PlayerScore += 1;
            //give player goal
            print(scoringPlayer.PlayerName + "scored");
            ball.Reset();
            
            if (team == "blue")
            {
                print("red scored");
                teamRedScore++;
                wormLeagueUI.GoalScored("red", scoringPlayer.PlayerName);

            }
            else if (team == "red")
            {
                print("blue scored");
                teamBlueScore++;
                wormLeagueUI.GoalScored("blue", scoringPlayer.PlayerName);
            }
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
