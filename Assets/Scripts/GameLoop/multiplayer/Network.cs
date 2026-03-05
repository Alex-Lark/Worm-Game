using System;
using System.Collections;
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
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Network : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public UDPTransport udpTransport;
    public NetworkManager manager;
    
    public static Network instance;
    

    
    bool Init = false;

    private GameObject networkObject;

    private void OnDestroy()
    {
        {
            PlayerRegister.ResetRegister();
            manager?.StopClient();
            if(manager?.isServer == true) manager.StopServer();
            Destroy(manager?.gameObject);
        }
    }

    public void StartServer()
    {
        StartCommon();
        manager.StartHost();
    }
    
    public void StartClient(string address = null)
    {
        if(address == null)address = "127.0.0.1";
        StartCommon();
        manager.StartClient();
    }

    void StartCommon()
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

            manager.onClientConnectionState += OnNetworkChangeScene;
            manager.onServerConnectionState += OnNetworkChangeScene;

            PlayerRegister.InitRegister();

            gameObject.AddComponent<WormGameSceneSwitcher>();
        }
    }

    private void OnNetworkChangeScene(ConnectionState state)
    {
        if (state == ConnectionState.Connected)
        {
            WormGameSceneSwitcher switcher = gameObject.GetOrAddComponent<WormGameSceneSwitcher>();
            StartCoroutine(switcher.LoadGameLobbyScene(0));
        }

        if (state == ConnectionState.Disconnected)
        {
            gameObject.GetOrAddComponent<WormGameSceneSwitcher>().LoadMainMenuScene();
            Destroy(gameObject);
        }
    }
    
    public bool AllClientsReady()
    {
        manager.sceneModule.TryGetSceneID(SceneManager.GetActiveScene(), out SceneID scene);
        foreach (PlayerID player in manager.players)
        {
            if(!manager.scenePlayersModule.IsPlayerLoadedInScene(player,scene))return false;
        }
        return true;
    }
    
}
