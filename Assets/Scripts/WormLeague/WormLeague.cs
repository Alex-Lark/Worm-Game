using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using GameLoop;
using PurrNet;

namespace WormLeague
{
    public class WormLeague : NetworkBehaviour
    {
        #region Public Variables
        
        public WormLeagueUI wormLeagueUI;
        
        public Ball ball;

        public int teamRedScore = 0;
        public int teamBlueScore = 0;
        
        #endregion
        
        #region Private Variables
        
        private List<Player.Player> teamBlue = new List<Player.Player>();
        private List<Player.Player> teamRed = new List<Player.Player>();
        
        #endregion
        
        #region Built-In Methods
    
        void Start()
        {
            AssignPlayerTeams();
        }
        
        #endregion
        
        #region Public Methods

        public void OnGoalScored(string team)
        {
            Player.Player scoringPlayer = ball.LastTouchingPlayer;
            scoringPlayer.playerScore += 1;
            ball.Reset();
            
            if (team == "blue")
            {
                teamRedScore++;
                wormLeagueUI.GoalScored("red", scoringPlayer.PlayerName);

            }
            else if (team == "red")
            {
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
                foreach (Player.Player player in teamRed)
                {
                    player.playerScore += 10;
                }
            }
            else
            {
                foreach (Player.Player player in teamBlue)
                {
                    player.playerScore += 10;
                }
            }
        }
        
        #endregion
        
        #region Private Methods
    
        private void AssignPlayerTeams()
        {
            List<Player.Player> players = new List<Player.Player>(GameLoop.GameLoop.Instance.players);

            //currently this always assigns the first random player to team red, so worm is always red with 1 player
            while (players.Count > 0)
            {
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
        
        #endregion
    }
}
