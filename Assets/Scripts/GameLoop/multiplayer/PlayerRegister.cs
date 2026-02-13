using System;
using System.Collections.Generic;
using PurrNet;
using PurrNet.Packing;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerRegister : PurrMonoBehaviour
{
    public static Dictionary<PlayerID, PlayerData> Players = new Dictionary<PlayerID, PlayerData>();
    public static event Action<PlayerID> OnPlayerRegistered;
    private string FixUserName(string newName)
    {
        List<string> takenNames = new List<string>();
        foreach (PlayerData player in Players.Values)
        {
            takenNames.Add(player.name);
        }
        
        newName ??= "Player";
        newName = System.Text.RegularExpressions.Regex.Replace(newName, "[^A-Za-z0-9_-]", "");
        if (newName.Length < 2 || newName.Length > 16) newName = "Player";
        while (takenNames.Contains(newName))
        {
            newName += (int)(Random.value*10);
            if (newName.Length < 2 || newName.Length > 16) newName = "Player";
        }
        return newName;
    }
    
    public struct PlayerData : IPackedAuto
    {
        public string name;
        public PlayerID playerID;
        public Color color;
        public ushort score;
        public bool isDead;
        public bool isDisconected;
    }
    
    private void OnUsernameRequest(PlayerID playerID, PlayerData player, bool asServer)
    {
        if (asServer)
        {
            // Store the sender's ID and validate name
            player.playerID = playerID;
            player.name = FixUserName(player.name);
        
            // Register on server first
            RegisterPlayer(player, player.playerID);
        
            // Now broadcast ALL players to ALL clients (including the new one)
            foreach (var existingPlayer in Players)
            {
                PlayerData playerData = new PlayerData
                {
                    playerID = existingPlayer.Key,
                    name = existingPlayer.Value.name
                };
                Network.instance.manager.SendToAll(playerData);
            }
        }
        else
        {
            RegisterPlayer(player, player.playerID);
        }
    }

    private void RegisterPlayer(PlayerData player, PlayerID playerID)
    {
        Players[playerID] = player;
        OnPlayerRegistered?.Invoke(playerID);
    }
    
    public static void RemoveClient(PlayerID playerID, bool _)
    {
        Debug.Log("Delisting client: "+playerID.id.value);
        PlayerData playerData = Players[playerID];
        playerData.isDisconected = true;

    }
    
    public static void RegisterClient(PlayerID playerID, bool firstJoin, bool _)
    {
        Debug.Log("Registering client: "+playerID.id.value);
        if(firstJoin)Players[playerID] = new PlayerData();
    }
    
    public override void Subscribe(NetworkManager manager, bool asServer)
    {
        manager.Subscribe<PlayerData>(OnUsernameRequest, asServer);
    }
        
    public override void Unsubscribe(NetworkManager manager, bool asServer)
    {
        manager.Unsubscribe<PlayerData>(OnUsernameRequest, asServer);
            
    }
}
