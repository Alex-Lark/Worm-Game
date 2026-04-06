using System;
using System.Collections;
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
            if (!isHost && !isServer)
            {
                Destroy(this);
                return;
            }

            //StartCoroutine(Ddebug());
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
                //wormLeagueUI.GoalScored("red", scoringPlayer.name);
                WormLeagueUI.GoalScoredPacket packet;
                packet.playerName = scoringPlayer.name;
                packet.goalName = "red";
                Network.instance.manager.SendToAll(packet);

            }
            else if (team == "red")
            {
                teamBlueScore++;
                //wormLeagueUI.GoalScored("blue", scoringPlayer.name);
                
                WormLeagueUI.GoalScoredPacket packet;
                packet.playerName = scoringPlayer.name;
                packet.goalName = "blue";
                Network.instance.manager.SendToAll(packet);
            }

            PlayerRegister.Players[scoringPlayer.playerID] = scoringPlayer;
            Network.instance.manager.SendToAll(scoringPlayer);
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
                    PlayerRegister.Players[playerData.playerID] = playerData;
                    
                }
            }
            else if (teamRedScore < teamBlueScore)
            {
                foreach (PlayerID player in teamBlue)
                {
                    PlayerRegister.PlayerData playerData = PlayerRegister.Players[player];
                    playerData.score += 10;
                    PlayerRegister.Players[playerData.playerID] = playerData;
                }
            }
            else
            {
                foreach (PlayerID player in teamBlue)
                {
                    PlayerRegister.PlayerData playerData = PlayerRegister.Players[player];
                    playerData.score += 5;
                    PlayerRegister.Players[playerData.playerID] = playerData;
                    
                }
                foreach (PlayerID player in teamRed)
                {
                    PlayerRegister.PlayerData playerData = PlayerRegister.Players[player];
                    playerData.score += 5;
                    PlayerRegister.Players[playerData.playerID] = playerData;
                    
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

                {
                    PlayerRegister.PlayerData playerData = PlayerRegister.Players[players[random]];
                    playerData.team = PlayerRegister.Team.Red;
                    //PlayerRegister.Players[players[random]] = playerData;
                    Network.instance.manager.SendToServer<PlayerRegister.PlayerData>(playerData);
                    print(playerData.name +" is on "+playerData.team);
                }

                players.RemoveAt(random);
                if (players.Count > 0)
                {
                    random =  Random.Range(0, players.Count);
                    teamBlue.Add(players[random]);
                    
                    {
                        PlayerRegister.PlayerData playerData = PlayerRegister.Players[players[random]];
                        playerData.team = PlayerRegister.Team.Blue;
                        //PlayerRegister.Players[players[random]] = playerData;
                        Network.instance.manager.SendToServer<PlayerRegister.PlayerData>(playerData);
                        print(playerData.name +" is on "+playerData.team);
                    }
                    
                    players.RemoveAt(random);
                }
            }
            
        }

        private IEnumerator Ddebug()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                PlayerRegister.PlayerData playerData = new PlayerRegister.PlayerData();
                playerData.name = "DEBUG PLS WORK";
                Network.instance.manager.SendToServer<PlayerRegister.PlayerData>(playerData);
                
            }
        }
        
        private void AssignPlayerSpawnPoints()
        {
            Debug.Log("Assigning spawnpoint to player " + localPlayer);
            //TODO: make it by player, make each spawnpoint assigned only once, maybe go in order of spawnpoints and randomize players so no random positions
            
            GameObject spawnpoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            
            if (LocalPlayer.Instance != null) LocalPlayer.Instance.GetComponent<PlayerSpawning>().SetSpawnPoint(spawnpoint);
        }
        
        #endregion
    }
}
