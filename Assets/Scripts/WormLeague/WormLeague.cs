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
        public List<GameObject> redSpawnPoints;
        public List<GameObject> blueSpawnPoints;
        
        #endregion
        
        #region Private Variables
        
        private List<PlayerID> teamBlue = new();
        private List<PlayerID> teamRed = new();
        
        #endregion
        
        #region Built-In Methods
    
        void Start()
        {
            //StartCoroutine(Ddebug());
            AssignPlayerTeams();
            AssignPlayerSpawnPoints();
            
            LoadingScreenManager.LoadingScreenForSelf(false);
        }
        
        public void OnDestroy()
        {
            PlayerRegister.ClearPlayerTeams();
            GameOver();
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
                
                if (Network.instance == null) return;
                if (Network.instance.manager == null) return;
                
                Network.instance.manager.SendToAll(packet);

            }
            else if (team == "red")
            {
                teamBlueScore++;
                //wormLeagueUI.GoalScored("blue", scoringPlayer.name);
                
                WormLeagueUI.GoalScoredPacket packet;
                packet.playerName = scoringPlayer.name;
                packet.goalName = "blue";
                
                if (Network.instance == null) return;
                if (Network.instance.manager == null) return;
                
                Network.instance.manager.SendToAll(packet);
            }

            PlayerRegister.Players[scoringPlayer.playerID] = scoringPlayer;
            Network.instance.manager.SendToAll(scoringPlayer);
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
                    Network.instance.manager.SendToAll(playerData);
                    
                    
                }
            }
            else if (teamRedScore < teamBlueScore)
            {
                foreach (PlayerID player in teamBlue)
                {
                    PlayerRegister.PlayerData playerData = PlayerRegister.Players[player];
                    playerData.score += 10;
                    PlayerRegister.Players[playerData.playerID] = playerData;
                    Network.instance.manager.SendToAll(playerData);
                    
                }
            }
            else
            {
                foreach (PlayerID player in teamBlue)
                {
                    PlayerRegister.PlayerData playerData = PlayerRegister.Players[player];
                    playerData.score += 5;
                    PlayerRegister.Players[playerData.playerID] = playerData;
                    Network.instance.manager.SendToAll(playerData);
                    
                }
                foreach (PlayerID player in teamRed)
                {
                    PlayerRegister.PlayerData playerData = PlayerRegister.Players[player];
                    playerData.score += 5;
                    PlayerRegister.Players[playerData.playerID] = playerData;
                    Network.instance.manager.SendToAll(playerData);
                    
                }
            }
        }
        
        #endregion
        
        #region Private Methods
    
        private void AssignPlayerTeams()
        {
            List<Player.Player> playerObjects = new List<Player.Player>(FindObjectsByType<Player.Player>(FindObjectsSortMode.None));

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
            //TODO: improve logic because the double for loop feels gross
            if (!isServer) return;
            
            List<Player.Player> playerObjects = new List<Player.Player>(FindObjectsByType<Player.Player>(FindObjectsSortMode.None));
            Debug.Log("Assigning spawnpoint to player as server");

            List<PlayerID> teamRedCopy = teamRed;
            Debug.Log($"redcopy length: {teamRedCopy.Count}");
            foreach(GameObject redSpawnPoint in redSpawnPoints)
            {
                if (teamRedCopy.Count == 0) break;
    
                int random = Random.Range(0, teamRedCopy.Count);
                PlayerID randomPlayer = teamRedCopy[random];
                teamRedCopy.RemoveAt(random);
                
                Debug.Log($"assigning {redSpawnPoint} to {randomPlayer}");
                foreach (Player.Player player in playerObjects)
                {
                    if (player.owner == randomPlayer)
                    {
                        player.GetComponent<PlayerSpawning>().SetSpawnPoint(redSpawnPoint);
                    }
                }
            }
            
            List<PlayerID> teamBlueCopy = teamBlue;
            foreach(GameObject blueSpawnPoint in blueSpawnPoints)
            {
                if (teamBlueCopy.Count == 0) break;
    
                int random = Random.Range(0, teamBlueCopy.Count);
                PlayerID randomPlayer = teamBlueCopy[random];
                teamBlueCopy.RemoveAt(random);
                
                foreach (Player.Player player in playerObjects)
                {
                    if (player.owner == randomPlayer)
                    {
                        player.GetComponent<PlayerSpawning>().SetSpawnPoint(blueSpawnPoint);
                    }
                }
            }
        }
        
        #endregion
    }
}
