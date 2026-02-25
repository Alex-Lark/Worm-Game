using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Player
{
    public class DeathScreenUI : MonoBehaviour
    {
        public static DeathScreenUI Instance { get; private set; }

        public GameObject deathText;
        public TextMeshProUGUI respawnText;
    
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
        }
        
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void EnableDeathUI()
        {
            deathText.SetActive(true);
            respawnText.enabled = true;
            GetComponent<Image>().color = new Color32(255, 0, 0, 15);
        }

        public void DisableDeathUI()
        {
            deathText.SetActive(false);
            respawnText.enabled = false;
            GetComponent<Image>().color = new Color(255,0, 0,0);
        }
    
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!GameSceneList.IsSceneAGameScene(scene.name))
            {
                Destroy(gameObject);
            }
        }
    }
}
