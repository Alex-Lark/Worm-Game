using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Player
{
    public class UsernameSaving : MonoBehaviour
    {
        public string username;
        public TMP_InputField usernameInputField;
        
        private static UsernameSaving Instance { get; set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        public void SaveUsername()
        {
            if (usernameInputField != null)
            {
                username = usernameInputField.text;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "GameLobbyScene")
            {
                Debug.Log("switched to game lobby in username saving, username: " + username);
                Player.Instance.PlayerName = username;
                Destroy(gameObject);
            }
        }
        
        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}