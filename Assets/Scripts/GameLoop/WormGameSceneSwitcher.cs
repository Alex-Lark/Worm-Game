using System;
using System.Collections;
using PurrNet;
using PurrNet.Packing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameLoop
{
    public class WormGameSceneSwitcher : PurrMonoBehaviour
    {
        public event Action OnSceneLoaded;
    
        public void LoadMainMenuScene()
        {
            SceneManager.LoadScene("MainMenuScene");
            GameLoop.Instance?.Reset();
            Destroy(Network.instance);
            Destroy(GameLoop.Instance?.gameObject);
            Destroy(Player.LocalPlayer.Instance?.gameObject);
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
    
        public void LoadPartSelectionScene()
        {
            SendSceneChangeRequest("PartSelectionScene");
        }
    
        public void LoadCreatureBuilderScene()
        {
            SceneManager.LoadScene("CreatureBuilderScene");
        }
    
        public void LoadGameScene()
        {
            SceneManager.LoadScene(GameSceneList.GetRandomGameScene());
            
            foreach (Player.Player player in GameLoop.Instance.players)
            {
                player.SetWormInGameScene();
            }
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

        public void SendSceneChangeRequest(string sceneName)
        {
            if (!Network.instance.manager.isServer)
            {
                Debug.LogError("Cannot send scene change request as client: "+sceneName);
                return;
            }
            SceneChange scene = new SceneChange();
            scene.name = sceneName;
            Network.instance.manager.SendToAll<SceneChange>(scene);
        }
        void OnSceneChangeRequest(PlayerID player, SceneChange scene, bool asServer)
        {
            SceneManager.LoadScene(scene.name);
        }
        
        public struct SceneChange : IPackedAuto
        {
            public string name;
        }
    
        public override void Subscribe(NetworkManager manager, bool asServer)
        {
            manager.Subscribe<SceneChange>(OnSceneChangeRequest, asServer);
        }
        
        public override void Unsubscribe(NetworkManager manager, bool asServer)
        {
            manager.Unsubscribe<SceneChange>(OnSceneChangeRequest, asServer);
            
        }
    }

    
}
