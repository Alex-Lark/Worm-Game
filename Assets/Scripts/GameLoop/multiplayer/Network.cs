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
            manager.onPlayerJoined += PlayerRegister.RegisterClient;
            
            manager.onClientConnectionState += ToLobby;
        }
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
}
