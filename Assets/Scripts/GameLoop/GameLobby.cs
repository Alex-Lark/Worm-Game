using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GameLoop
{
    public class GameLobby : MonoBehaviour
    {
        public Material wormMaterial;
        public Material wormHeadMaterial;
        
        public Image selectColorButtonColor;

        public GameObject colorSelectionPanel;

        void Start()
        {
            selectColorButtonColor.color = wormMaterial.color;
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
    }
}
