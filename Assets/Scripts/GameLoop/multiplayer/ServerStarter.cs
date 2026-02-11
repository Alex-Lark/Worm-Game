using GameLoop;
using UnityEngine;
using PurrNet;
using PurrNet.Transports;
using TMPro;
using Unity.VisualScripting;

public class ServerStarter : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public UDPTransport udpTransport;
    public NetworkManager manager;
    public static ServerStarter instance;
    
    bool hasStarted = false;

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
        hasStarted = true;
        networkObject = Instantiate(Resources.Load<GameObject>("Multiplayer/Network-Host"));

        manager = networkObject.GetComponent<NetworkManager>();
        udpTransport = manager.transport as UDPTransport;
        if (udpTransport == null)
        {
            Debug.LogError("No UDP Transport found");
            return;
        }
        
        DontDestroyOnLoad(networkObject);
        udpTransport.serverPort = 5001;
        //manager.StartServer();
        
        new GameObject().AddComponent<WormGameSceneSwitcher>().LoadGameLobbyScene();
    }
}
