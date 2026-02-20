using System;
using System.Collections.Generic;
using PurrNet;
using PurrNet.Transports;
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
            PlayerRegister.OnPlayerRegisterChanged += UpdatePlayerList;
            
            PlayerRegister.PlayerData name = new PlayerRegister.PlayerData();
            name.name = Player.Player.Instance.PlayerName;
            Network.instance.manager.SendToServer<PlayerRegister.PlayerData>(name);
            
            ToggleStartGameButton();
            colorSelection.SetInitialColor();
        }

        private void OnDestroy()
        {
            PlayerRegister.OnPlayerRegisterChanged -= UpdatePlayerList;
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

        public void UpdatePlayerList(PlayerID playerID, bool connected)
        {
            
            foreach (Transform child in playerList.transform)
            {
                 Destroy(child.gameObject);
            }

            foreach (KeyValuePair<PlayerID,PlayerRegister.PlayerData> player  in PlayerRegister.Players)
            {
                if(player.Value.isDisconected)continue;
                Debug.Log("Player register name: " + player.Value.name);
                
                GameObject textObject = Instantiate(playerUsernameTextPrefab, playerList.transform);
                TextMeshProUGUI tmpText = textObject.GetComponent<TextMeshProUGUI>();
                tmpText.text = player.Value.name;
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
