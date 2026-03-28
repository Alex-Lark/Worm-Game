using GameLoop.GameLobby;
using UnityEngine;

namespace Audio
{
    public class GameLobbyAudio : MonoBehaviour
    {
        void Start()
        {
            GetComponent<GameLobby>().OnGameStart += OnGameStart;
        }

        void OnDestroy()
        {
            GetComponent<GameLobby>().OnGameStart -= OnGameStart;
        }

        private void OnGameStart()
        {
            //play audio
        }
    }
}
