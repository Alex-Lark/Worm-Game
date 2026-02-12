using GameLoop;
using UnityEngine;
using PurrNet;
using PurrNet.Transports;
using TMPro;
using Unity.VisualScripting;

public class ClientStarter : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public UDPTransport udpTransport;
    public NetworkManager manager;
    bool Init = false;

    private GameObject networkObject;
    
    void Start()
    { 
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   
}