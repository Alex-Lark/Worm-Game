using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLoop
{
    public class GameLobby : MonoBehaviour
    {
        public Material wormMaterial;
        public Material wormHeadMaterial;
        
        public Image selectColorButtonColor;
        public Image playerList;

        public GameObject colorSelectionPanel;

        public GameObject playerUsernameTextPrefab;

        void Start()
        {
            selectColorButtonColor.color = wormMaterial.color;
            UpdatePlayerList();
        }

        public void OpenColorSelectionPanel()
        {
            colorSelectionPanel.SetActive(true);
        }

        public void CloseColorSelectionPanel()
        {
            colorSelectionPanel.SetActive(false);
        }
        
        public void StartGame()
        {
            GameLoop.Instance.StartGame();
        }

        public void SetColor(Button button)
        {
            wormMaterial.color = button.image.color;

            wormHeadMaterial.color = button.transform.GetChild(0).GetComponent<Image>().color;
            
            selectColorButtonColor.color = wormMaterial.color;
            
            CloseColorSelectionPanel();
        }

        public void UpdatePlayerList()
        {
            foreach (Transform child in playerList.transform)
            {
                Destroy(child.gameObject);
            }
            
            foreach (Player.Player player in GameLoop.Instance.players)
            {
                GameObject textObject = Instantiate(playerUsernameTextPrefab, playerList.transform);
                TextMeshProUGUI tmpText = textObject.GetComponent<TextMeshProUGUI>();
                tmpText.text = player.PlayerName;
            }
        }
    }
}
