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
        newName = newName.Replace("[A-Za-z0-9_-]", "");
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
    }
    
    private void OnUsernameRequest(PlayerID player, UserNameRequest name, bool asServer)
    {
        if (asServer)   // The broadcast was sent to the Server from a Client
        {
            // Send the broadcast down to the Clients
            name.name = FixUserName(name.name);
            Network.instance.manager.SendToAll<UserNameRequest>(name);
        }
        else    // The broadcast was sent to the Clients from the Server
        {
            RegisterUsername(name.name, player);
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
