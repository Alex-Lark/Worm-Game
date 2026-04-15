using PurrNet;
using PurrNet.Packing;
using UnityEngine;

public class LoadingScreenManager : PurrMonoBehaviour
{
    public static LoadingScreenManager instance;
    private Canvas canvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(this);
        instance = this;
        canvas = GetComponent<Canvas>();
        canvas.enabled = false;
    }
    

    void OnLoadingScreenRequest(PlayerID player, LoadingScreenRequest request, bool isServer)
    {
        canvas.enabled = request.display;
    }
    
    public static void LoadingScreenForSelf(bool display)
    {
        instance.canvas.enabled = display;
    }
    
    public static void LoadingScreenForAll(bool display)
    {
        if (!Network.instance.manager.isServer&&!Network.instance.manager.isHost)return;
        LoadingScreenRequest request;
        request.display = display;
        Network.instance.manager.SendToAll<LoadingScreenRequest>(request);
    }

    struct LoadingScreenRequest : IPackedAuto
    {
        public bool display;
    }
    
    public override void Subscribe(NetworkManager manager, bool asServer)
    {
        manager.Subscribe<LoadingScreenRequest>(OnLoadingScreenRequest, asServer);
    }

    public override void Unsubscribe(NetworkManager manager, bool asServer)
    {
        manager.Unsubscribe<LoadingScreenRequest>(OnLoadingScreenRequest, asServer);
    }
}
