using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using GameLoop;
using Player;
using PurrNet;
using Unity.VisualScripting;

namespace WormLeague
{
    public class WormLeague : NetworkBehaviour
    {
        #region Public Variables
        
        public WormLeagueUI wormLeagueUI;
        
        public Ball ball;

        public int teamRedScore = 0;
        public int teamBlueScore = 0;

        public List<GameObject> spawnPoints;
        
        #endregion
        
        #region Private Variables
        
        private List<PlayerID> teamBlue = new();
        private List<PlayerID> teamRed = new();
        
        #endregion
        
        #region Built-In Methods
    
        void Start()
        {
            AssignPlayerTeams();
            AssignPlayerSpawnPoints();
        }

        #endregion
        
        #region Public Methods

        public void OnGoalScored(string team)
        {
            PlayerRegister.PlayerData scoringPlayer = ball.LastTouchingPlayer.RegisterData;
            scoringPlayer.score += 1;
            ball.Reset();
            
            if (team == "blue")
            {
                teamRedScore++;
                wormLeagueUI.GoalScored("red", scoringPlayer.name);

            }
            else if (team == "red")
            {
                teamBlueScore++;
                wormLeagueUI.GoalScored("blue", scoringPlayer.name);
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
                foreach (PlayerID player in teamRed)
                {
                    PlayerRegister.PlayerData playerData = PlayerRegister.Players[player];
                    playerData.score += 10;
                }
            }
            else if (teamRedScore < teamBlueScore)
            {
                foreach (PlayerID player in teamBlue)
                {
                    PlayerRegister.PlayerData playerData = PlayerRegister.Players[player];
                    playerData.score += 10;
                }
            }
            else
            {
                foreach (PlayerID player in teamBlue)
                {
                    PlayerRegister.PlayerData playerData = PlayerRegister.Players[player];
                    playerData.score += 5;
                }
                foreach (PlayerID player in teamRed)
                {
                    PlayerRegister.PlayerData playerData = PlayerRegister.Players[player];
                    playerData.score += 5;
                }
            }
        }
        
        #endregion
        
        #region Private Methods
    
        private void AssignPlayerTeams()
        {

            //currently this always assigns the first random player to team red, so worm is always red with 1 player
            List<PlayerID> players = new List<PlayerID>(PlayerRegister.Players.Keys.AsReadOnlyList());
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
        
        private void AssignPlayerSpawnPoints()
        {
            Debug.Log("Assigning spawnpoint to player " + localPlayer);
            //TODO: make it by player, make each spawnpoint assigned only once, maybe go in order of spawnpoints and randomize players so no random positions
            
            GameObject spawnpoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            LocalPlayer.Instance.GetComponent<PlayerSpawning>().SetSpawnPoint(spawnpoint);
        }
        
        #endregion
    }
}
