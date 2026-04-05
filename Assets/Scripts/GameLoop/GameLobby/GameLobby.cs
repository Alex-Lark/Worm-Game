using System;
using System.Collections.Generic;
using PurrNet;
using PurrNet.Transports;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Linq;

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
        
        public event Action OnGameStart;
        #endregion

        #region Built-In Methods
        
        void Start()
        {
            Debug.Log("Game Lobby start method called");
            playerRegister = Network.instance.gameObject.GetOrAddComponent<PlayerRegister>();
            PlayerRegister.OnPlayerRegisterChanged += OnPlayerRegisterChanged;
            PlayerRegister.OnPlayerRegistered += OnPlayerRegistered;

            if (Player.LocalPlayer.Instance != null)
            {
                RegisterLocalPlayer();
            }
            else
            {
                // Wait for the local player to finish spawning and registering
                Player.LocalPlayer.OnLocalPlayerReady += OnLocalPlayerReady;
            }
        }

        private void OnLocalPlayerReady()
        {
            Player.LocalPlayer.OnLocalPlayerReady -= OnLocalPlayerReady;
            RegisterLocalPlayer();
        }

        private void RegisterLocalPlayer()
        {
            PlayerRegister.PlayerData name = new PlayerRegister.PlayerData();
            name.name = Player.LocalPlayer.Instance.PlayerName;
            if (Network.instance != null)
            {
                Network.instance.manager.SendToServer<PlayerRegister.PlayerData>(name);
            }
        }

        private void OnDestroy()
        {
            PlayerRegister.OnPlayerRegisterChanged -= OnPlayerRegisterChanged;
            PlayerRegister.OnPlayerRegistered -= OnPlayerRegistered;
            Player.LocalPlayer.OnLocalPlayerReady -= OnLocalPlayerReady; // safety cleanup
        }

        #endregion
        
        #region Multiplayer Events
        
        public void OnPlayerRegistered(PlayerID playerID)
        {
            RefreshColorSelection();
            colorSelection.SetInitialColor();
        }

        public void OnPlayerRegisterChanged(PlayerID playerID, bool connected)
        {
            Debug.Log("Player Register changed");
            UpdatePlayerList(playerID, connected);

            if (connected && playerID == Network.instance.manager.localPlayer)
                colorSelection.SetInitialColor();

            RefreshColorSelection();
            ToggleStartGameButton();
        }
        
        private void RefreshColorSelection()
        {
            HashSet<int> taken = new HashSet<int>(
                PlayerRegister.Players.Values
                    .Where(p => p.colorIndex >= 0 && p.colorIndex < colorSelection.availableColors.Count)
                    .Select(p => p.colorIndex)
            );
            colorSelection.RefreshTakenColors(taken);
        }
        
        #endregion

        #region Public Methods

        public void OpenColorSelectionPanel()
        {
            colorSelectionPanel.SetActive(true);
            colorSelection.UpdateColorButtons();
        }

        public void CloseColorSelectionPanel()
        {
            colorSelectionPanel.SetActive(false);
        }

        public void StartGame()
        {
            GameLoop.Instance.StartGame();
            OnGameStart?.Invoke();
        }

        public void UpdatePlayerList(PlayerID playerID, bool connected)
        {
            Debug.Log("player list updating");
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
