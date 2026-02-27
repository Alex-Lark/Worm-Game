using System;
using System.Collections;
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
            GameLoop.Instance.Reset();
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

        public void LoadGameLobbyScene()
        {
            SceneManager.LoadScene("GameLobbyScene");
        }
    
        public void LoadPartSelectionScene()
        {
            SceneManager.LoadScene("PartSelectionScene");
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
                player.playerSpawning.SpawnInGameScene();
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
    }
}
