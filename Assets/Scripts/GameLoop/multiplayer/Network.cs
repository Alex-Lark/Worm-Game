using System;
using System.Collections.Generic;
using GameLoop;
using Player;
using UnityEngine;
using PurrNet;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Transports;
using TMPro;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class Network : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public UDPTransport udpTransport;
    public NetworkManager manager;
    
    public static Network instance;
    public static Dictionary<PlayerID, string> UserNames = new Dictionary<PlayerID, string>();
    
    bool Init = false;

    private GameObject networkObject;
    
    void Start()
    { 
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartServer()
    {
        if (!Init)
        {
            instance = this;
            Init = true;
            networkObject = Instantiate(Resources.Load<GameObject>("Multiplayer/Network-Config"));

            manager = networkObject.GetComponent<NetworkManager>();
            udpTransport = manager.transport as UDPTransport;
            if (udpTransport == null)
            {
                Debug.LogError("No UDP Transport found");
                return;
            }

            DontDestroyOnLoad(networkObject);
            udpTransport.serverPort = 5001;
            DontDestroyOnLoad(gameObject);
            
            manager.StartHost();
            manager.onPlayerJoined += RegisterClient;
            
        }
        
        new GameObject().AddComponent<WormGameSceneSwitcher>().LoadGameLobbyScene();
        
        UserNameRequest name = new UserNameRequest();
        name.name = UsernameSaving.username;
        Network.instance.manager.SendToServer<UserNameRequest>(name);
    }
    
    public void StartClient(string address = null)
    {
        if(address == null)address = "127.0.0.1";
        if (!Init)
        {
            instance = this;
            Init = true;
            networkObject = Instantiate(Resources.Load<GameObject>("Multiplayer/Network-Config"));

            manager = networkObject.GetComponent<NetworkManager>();
            udpTransport = manager.transport as UDPTransport;
            if (udpTransport == null)
            {
                Debug.LogError("No UDP Transport found");
                return;
            }

            DontDestroyOnLoad(networkObject);
            udpTransport.serverPort = 5001;
            
            manager.onClientConnectionState += ToLobby;

            gameObject.AddComponent<WormGameSceneSwitcher>();
        }

        manager.StartClient();
        
        UserNameRequest name = new UserNameRequest();
        name.name = UsernameSaving.username;
        Network.instance.manager.SendToServer<UserNameRequest>(name);

    }

    private void ToLobby(ConnectionState state)
    {
        if (state == ConnectionState.Connected)
        {
            GetComponent<WormGameSceneSwitcher>().LoadGameLobbyScene();
        }

        if (state == ConnectionState.Disconnected)
        {
            Destroy(gameObject);
        }

    }

    private void RegisterClient(PlayerID playerID, bool firstJoin, bool _)
    {
        Debug.Log("Registering client: "+playerID.id.value);
        if(firstJoin)UserNames[playerID] = "";
    }
    
    private static string FixUserName(string newName)
    {
        newName = newName.Replace("^[A-Za-z0-9_-]+$", "");
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
    }
}
