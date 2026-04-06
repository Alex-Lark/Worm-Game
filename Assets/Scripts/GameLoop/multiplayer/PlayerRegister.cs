using System;
using System.Collections.Generic;
using PurrNet;
using PurrNet.Packing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class PlayerRegister : PurrMonoBehaviour
{
    public static Dictionary<PlayerID, PlayerData> Players = new Dictionary<PlayerID, PlayerData>();
    public static UnityEvent<PlayerID, bool> OnPlayerRegisterChanged = new UnityEvent<PlayerID, bool>();
    public static UnityEvent OnPlayerRegistered = new UnityEvent();

    public static PlayerRegister Instance;

    private void Start()
    { 
        DontDestroyOnLoad(this);
        Instance = this;
    }

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

    public enum Team : byte
    {
        None = 0,
        Red = 1,
        Blue = 2
    }
    
    public struct PlayerData : IPackedAuto
    {
        public string name;
        public PlayerID playerID;
        public int colorIndex;
        public ushort score;
        public Team team;
        public bool isDisconected;
    }
    
    private void OnPlayerDataRequest(PlayerID playerID, PlayerData player, bool asServer)
    {
        if (String.IsNullOrEmpty(player.name))
        {
            Debug.LogWarning("Player packet recived with no name!");
        }
        if (player.team != 0)
        {
            Debug.LogWarning("Team found!");
        }
        if (asServer)
        {
            bool isUpdate = Players.ContainsKey(player.playerID) && !string.IsNullOrEmpty(Players[player.playerID].name);

            if (isUpdate)
            {
                //Players[playerID] = player;
                
            }
            else
            {
                // Store the sender's ID and validate name
                RejoinLogic(player.playerID, player.name);
                player.name = FixUserName(player.name);
            }
            
            // Register on server first
            RegisterPlayerData(player, player.playerID);
        
            // Now broadcast ALL players to ALL clients (including the new one)
            foreach (var existingPlayer in Players)
            {
                Network.instance.manager.SendToAll(existingPlayer.Value);
            }
        }
        else
        {
            RegisterPlayerData(player, player.playerID);
        }
    }

    private void RegisterPlayerData(PlayerData player, PlayerID playerID)
    {
        Players[player.playerID] = player;
        OnPlayerRegisterChanged.Invoke(playerID, true);
        Debug.Log("Recived Player Data for player "+player.playerID.id+ " Name: "+player.name);
    }
    
    public static void RemoveClient(PlayerID playerID, bool _)
    {
        Debug.Log("Delisting client: "+playerID.id.value);
        PlayerData playerData = Players[playerID];
        playerData.isDisconected = true;
        Players[playerID] = playerData;
        
        OnPlayerRegisterChanged.Invoke(playerID,false);
    }
    
    public static void RegisterClient(PlayerID playerID, bool firstJoin, bool _)
    {
        Debug.Log("Registering client: "+playerID.id.value);
        if(!Players.ContainsKey(playerID)) Players[playerID] = new PlayerData { colorIndex = -1 };
        PlayerData playerData = Players[playerID];
        playerData.isDisconected = false;
        Players[playerID] = playerData;
    
        OnPlayerRegistered?.Invoke();
    }

    private static void RejoinLogic(PlayerID playerID, string newName)
    {
        List<KeyValuePair<PlayerID, PlayerData>> playerValues = Players.ToListPooled();
        for(int i = 0; i < playerValues.Count; i++)
        {
            
            if(!playerValues[i].Value.isDisconected) continue;
            if (playerValues[i].Value.name == newName)
            {
                Players[playerID] = playerValues[i].Value;
                Players.Remove(playerValues[i].Value.playerID);
            }
        }
    }
    
    public override void Subscribe(NetworkManager manager, bool asServer)
    {
        manager.Subscribe<PlayerData>(OnPlayerDataRequest, asServer);
    }
        
    public override void Unsubscribe(NetworkManager manager, bool asServer)
    {
        manager.Unsubscribe<PlayerData>(OnPlayerDataRequest, asServer);
    }

    public static void ResetRegister()
    {
        if (Network.instance?.manager == null)return;
        
        Network.instance.manager.onPlayerJoined -= RegisterClient;
        Network.instance.manager.onPlayerLeft -= RemoveClient;
        Players.Clear();
    }

    public static void InitRegister()
    {
        if (Network.instance.manager == null)
        {
            Debug.LogError("Network manager is null! Can not initialize player register.");
            return;
        }
        Network.instance.manager.onPlayerJoined += RegisterClient;
        Network.instance.manager.onPlayerLeft += RemoveClient;
    }
    
    public static void UpdateColor(int colorIndex)
    {
        PlayerID localPlayerID = Network.instance.manager.localPlayer;

        if (!Players.ContainsKey(localPlayerID))
        {
            Debug.LogError("Local player not found in Players dictionary!");
            return;
        }

        PlayerData myData = Players[localPlayerID];
        myData.colorIndex = colorIndex;
        Players[localPlayerID] = myData;

        Network.instance.manager.SendToServer(myData);

        Debug.Log($"Updated {myData.name} colorIndex to {colorIndex}");
    }

    private void OnDestroy()
    {
        Debug.LogError("REGISTER DESTROYED!!!!!!!!!!!");
    }
}
