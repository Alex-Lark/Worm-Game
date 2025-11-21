using UnityEngine;

public class GameLobby : MonoBehaviour
{
    public void StartGame()
    {
        GameLoop.Instance.StartGame();
    }
}
