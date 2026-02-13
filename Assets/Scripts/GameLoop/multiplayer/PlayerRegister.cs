using System;
using System.Collections.Generic;
using PurrNet;
using PurrNet.Packing;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerRegister : PurrMonoBehaviour
{
    public static Dictionary<PlayerID, string> UserNames = new Dictionary<PlayerID, string>();

    public static event Action<PlayerID> OnPlayerRegistered;
    private string FixUserName(string newName)
    {
        newName = System.Text.RegularExpressions.Regex.Replace(newName, "[^A-Za-z0-9_-]", "");
        if (newName.Length < 2 || newName.Length > 16) newName = "Player";
        while (UserNames.ContainsValue(newName))
        {
            newName += (int)(Random.value*10);
            if (newName.Length < 2 || newName.Length > 16) newName = "Player";
        }
        return newName;
    }
    
    public struct UserNameRequest : IPackedAuto
    {
        public string name;
        public PlayerID playerID;
    }
    
    private void OnUsernameRequest(PlayerID player, UserNameRequest name, bool asServer)
    {
        if (asServer)
        {
            // Store the sender's ID and validate name
            name.playerID = player;
            name.name = FixUserName(name.name);
        
            // Register on server first
            RegisterUsername(name.name, name.playerID);
        
            // Now broadcast ALL players to ALL clients (including the new one)
            foreach (var existingPlayer in UserNames)
            {
                UserNameRequest playerData = new UserNameRequest
                {
                    playerID = existingPlayer.Key,
                    name = existingPlayer.Value
                };
                Network.instance.manager.SendToAll(playerData);
            }
        }
        else
        {
            RegisterUsername(name.name, name.playerID);
        }
    }

    private void RegisterUsername(string nameName, PlayerID playerID)
    {
        UserNames[playerID] = nameName;
        OnPlayerRegistered?.Invoke(playerID);
    }
    
    public static void RegisterClient(PlayerID playerID, bool firstJoin, bool _)
    {
        Debug.Log("Registering client: "+playerID.id.value);
        if(firstJoin)UserNames[playerID] = "";
    }
    
    public override void Subscribe(NetworkManager manager, bool asServer)
    {
        manager.Subscribe<UserNameRequest>(OnUsernameRequest, asServer);
    }
        
    public override void Unsubscribe(NetworkManager manager, bool asServer)
    {
        manager.Unsubscribe<UserNameRequest>(OnUsernameRequest, asServer);
            
    }
}
