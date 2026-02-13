using System.Collections.Generic;
using PurrNet;
using PurrNet.Packing;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
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

        public GameObject playerUsernameTextPrefab;
        
        public PlayerRegister playerRegister;
        #endregion

        #region Built-In Methods

        void Start()
        {
            selectColorButtonColor.color = wormMaterial.color;
            playerRegister = gameObject.GetOrAddComponent<PlayerRegister>();
            PlayerRegister.OnPlayerRegistered += UpdatePlayerList;
            
            PlayerRegister.UserNameRequest name = new PlayerRegister.UserNameRequest();
            name.name = Player.Player.Instance.PlayerName;
            Network.instance.manager.SendToServer<PlayerRegister.UserNameRequest>(name);
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

        public void UpdatePlayerList(PlayerID _)
        {
            foreach (Transform child in playerList.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (KeyValuePair<PlayerID,string> name  in PlayerRegister.UserNames)
            {
                GameObject textObject = Instantiate(playerUsernameTextPrefab, playerList.transform);
                TextMeshProUGUI tmpText = textObject.GetComponent<TextMeshProUGUI>();
                tmpText.text = name.Value;
            }
        }
    }
    #endregion
}
