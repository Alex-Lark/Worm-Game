using System.Collections.Generic;
using PurrNet;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameLoop.GameLobby
{
    public class GameLobby : MonoBehaviour
    {
        #region Public Variables
        
        public Image playerList;

        public GameObject colorSelectionPanel;
        public GameObject startGameButton;

        public GameObject playerUsernameTextPrefab;
        
        public PlayerRegister playerRegister;

        public ColorSelection colorSelection;
        #endregion

        #region Built-In Methods

        void Start()
        {
            Debug.Log("Game Lobby start method called");
            playerRegister = gameObject.GetOrAddComponent<PlayerRegister>();
            PlayerRegister.OnPlayerRegistered += UpdatePlayerList;
            
            PlayerRegister.UserNameRequest name = new PlayerRegister.UserNameRequest();
            name.name = Player.Player.Instance.PlayerName;
            Network.instance.manager.SendToServer<PlayerRegister.UserNameRequest>(name);
            ToggleStartGameButton();
            colorSelection.SetInitialColor();
        }

        #endregion

        #region Public Methods

        public void OpenColorSelectionPanel()
        {
            colorSelectionPanel.SetActive(true);
            colorSelection.RefreshButtons();
            
        }

        public void CloseColorSelectionPanel()
        {
            colorSelectionPanel.SetActive(false);
        }

        public void StartGame()
        {
            GameLoop.Instance.StartGame();
        }

        public void UpdatePlayerList(PlayerID _)
        {
            
            foreach (Transform child in playerList.transform)
            {
                 Destroy(child.gameObject);
            }

            foreach (KeyValuePair<PlayerID,string> name  in PlayerRegister.UserNames)
            {
                Debug.Log("Player register name: " + name);
                
                GameObject textObject = Instantiate(playerUsernameTextPrefab, playerList.transform);
                TextMeshProUGUI tmpText = textObject.GetComponent<TextMeshProUGUI>();
                tmpText.text = name.Value;
            }
        }
        
        #endregion
        
        private void ToggleStartGameButton()
        {
            bool isHost = Network.instance.manager.isServer;
            startGameButton.SetActive(isHost);
        }
        
    }
}
