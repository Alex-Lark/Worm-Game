using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Audio
{
    public class MainMenuMusic : MonoBehaviour
    {
        public static MainMenuMusic Instance { get; private set; }
        
        void Start()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenuScene" || scene.name == "SettingsScene" || scene.name == "CreateGameScene" ||
                scene.name == "JoinGameScene")
            {
                return;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
