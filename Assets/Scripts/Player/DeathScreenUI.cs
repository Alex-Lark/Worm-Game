using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Player
{
    public class DeathScreenUI : MonoBehaviour
    {
        public GameObject deathText;
        public TextMeshProUGUI respawnText;
        
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
            respawnText.gameObject.SetActive(true);
            GetComponent<Image>().color = new Color32(255, 0, 0, 15);
        }

        public void DisableDeathUI()
        {
            deathText.SetActive(false);
            respawnText.gameObject.SetActive(false);
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
