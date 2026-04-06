using System;
using System.Collections;
using Player;
using PurrNet;
using PurrNet.Packing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameLoop
{
    public class WormGameSceneSwitcher : MonoBehaviour
    {
        public event Action OnSceneLoaded;
    
        public void LoadMainMenuScene()
        {
            SceneManager.LoadScene("MainMenuScene");
            GameLoop.Instance?.Reset();
            Destroy(Network.instance);
            if(GameLoop.Instance!=null)Destroy(GameLoop.Instance?.gameObject);
            if(LocalPlayer.Instance)Destroy(LocalPlayer.Instance?.gameObject);
            if(PlayerRegister.Instance != null)Destroy(PlayerRegister.Instance);
        }

        public void LoadSettingsScene()
        {
            SceneManager.LoadScene("SettingsScene");
        }
    
        public void LoadJoinGameScene()
        {
            SceneManager.LoadScene("JoinGameScene");
        }

        public void LoadCreateGameScene()
        {
            SceneManager.LoadScene("CreateGameScene");
        }

        public IEnumerator LoadGameLobbyScene(float delay = -1)
        {
            if (delay >= 0) yield return new WaitForSeconds(delay);
            SceneManager.LoadScene("GameLobbyScene");
        }
        public void LoadCreatureBuilderScene()
        {
            SceneManager.LoadScene("CreatureBuilderScene");
        }
    
        public void LoadGameScene()
        {
            SceneManager.LoadScene(GameSceneList.GetRandomGameScene());
        }
    
        public void LoadLeaderboardScene()
        {
            SceneManager.LoadScene("LeaderboardScene");
            
            foreach (Player.Player player in GameLoop.Instance.players)
            {
                player.DeactivatePlayer();
            }
        }
    
        public void LoadGameEndScene()
        {
            SceneManager.LoadScene("GameEndScene");
            
            foreach (Player.Player player in GameLoop.Instance.players)
            {
                player.DeactivatePlayer();
            }
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #else
                            Application.Quit();
            #endif
        }
        
        
        void OnSceneChangeRequest(PlayerID player, SceneChange scene, bool asServer)
        {
            SceneManager.LoadScene(scene.name);
        }
        
        public struct SceneChange : IPackedAuto
        {
            public string name;
        }
    }

    
}
