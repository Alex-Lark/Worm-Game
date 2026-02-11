using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameLoop
{
    public class GameLobby : MonoBehaviour
    {
        #region Public Variables
        
        public Material wormMaterial;
        public Material wormHeadMaterial;
        
        public Image selectColorButtonColor;
        public Image playerList;

        public GameObject colorSelectionPanel;
        public Transform chatImage;

        public GameObject playerUsernameTextPrefab;
        public GameObject chatTextPrefab;
        
        public TMP_InputField chatInputField;

        public int maxMessages = 10;
        
        #endregion
        
        #region Built-In Methods

        void Start()
        {
            selectColorButtonColor.color = wormMaterial.color;
            UpdatePlayerList();
        }
        
        #endregion
        
        #region Public Methods

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

        public void SendChatMessage(string message)
        {
            if (message == "")
            {
                return;
            }
            
            string finalMessage = "<" + Player.Player.Instance.PlayerName + "> " + message;
            
            GameObject messageObject = Instantiate(chatTextPrefab, chatImage);
            TextMeshProUGUI messageText = messageObject.GetComponent<TextMeshProUGUI>();
            messageText.text = finalMessage;
            
            if (chatImage.childCount > maxMessages)
            {
                Destroy(chatImage.GetChild(0).gameObject);
            }
            
            chatInputField.text = "";
            chatInputField.ActivateInputField();
        }

        public void DeactivateInputField()
        {
            chatInputField.DeactivateInputField();
        }
        
        #endregion
    }
}
