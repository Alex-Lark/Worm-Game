using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WormLeague
{
    public class WormLeague : MonoBehaviour
    {
        public WormLeagueUI wormLeagueUI;

        public Ball ball;

        public int teamRedScore = 1;
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

        public void OnDestroy()
        {
            GameOver();
        }

        public void GameOver()
        {
            if (teamRedScore > teamBlueScore)
            {
                foreach (Player player in teamRed)
                {
                    player.PlayerScore += 10;
                }
            }
            else
            {
                foreach (Player player in teamBlue)
                {
                    player.PlayerScore += 10;
                }
            }
        }
    
        private void AssignPlayerTeams()
        {
            List<Player> players = new List<Player>(GameLoop.Instance.players);


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
