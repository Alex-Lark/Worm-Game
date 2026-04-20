using UnityEngine;

public class GameJoiner : MonoBehaviour
{
    public void Join()
    {
        LoadingScreenManager.LoadingScreenForSelf(true);
        GameObject Client = new GameObject();
        Client.AddComponent<Network>().StartClient();
        DontDestroyOnLoad(Client);
    }
    
    public void Host()
    {
        LoadingScreenManager.LoadingScreenForSelf(true);
        GameObject Host = new GameObject();
        Host.AddComponent<Network>().StartServer();
        DontDestroyOnLoad(Host);
    }
}
