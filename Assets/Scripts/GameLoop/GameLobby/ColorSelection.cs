using System.Collections.Generic;
using GameLoop.multiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace GameLoop.GameLobby
{
    public class ColorSelection : MonoBehaviour
    {
        public GameObject colorSelectionPanel;
        public GameObject buttonPrefab; 
        
        public Material wormMaterial;
        public Material wormHeadMaterial;
        public Image selectColorButtonColor;
        
        public List<ColorPair> availableColors = new List<ColorPair>();
        
        private List<Button> colorButtons = new List<Button>();
        private int currentColorIndex = -1;
        private HashSet<Color> takenColors = new HashSet<Color>();
        
        void Start()
        {
            selectColorButtonColor.color = wormMaterial.color;
            CreateColorButtons();
        }
        
        public void UpdateMultiplayerColors(List<Color> playerColors)
        {
            takenColors.Clear();
            foreach (Color color in playerColors)
            {
                takenColors.Add(color);
                Debug.Log("Updating colors from multiplayer with color" + color);
            }
            UpdateColorButtons();
        }
        
        public void SetColor(int colorIndex)
        {
            ColorPair selectedColor = availableColors[colorIndex];
    
            if (takenColors.Contains(selectedColor.bodyColor))
            {
                Debug.LogWarning("This color is already taken!");
                return;
            }
    
            // Remove old color from taken set
            if (currentColorIndex >= 0)
            {
                takenColors.Remove(availableColors[currentColorIndex].bodyColor);
            }
    
            // Apply new color
            ApplyColor(selectedColor);
    
            // Mark new color as taken
            currentColorIndex = colorIndex;
            takenColors.Add(selectedColor.bodyColor);
    
            // Send color update via network
            FindFirstObjectByType<ColorSync>().SendColorUpdate(selectedColor.bodyColor);
    
            UpdateColorButtons();
            GetComponent<GameLobby>().CloseColorSelectionPanel();
        }
        
        public void SetInitialColor()
        {
            Color desiredColor = wormMaterial.color;
            
            // Try to use current color if available
            int colorIndex = FindColorIndex(desiredColor);
            if (colorIndex >= 0 && !takenColors.Contains(desiredColor))
            {
                SetColor(colorIndex);
                return;
            }
            
            // Otherwise find first available color
            for (int i = 0; i < availableColors.Count; i++)
            {
                if (!takenColors.Contains(availableColors[i].bodyColor))
                {
                    SetColor(i);
                    return;
                }
            }
        }
        
        private void CreateColorButtons()
        {
            ClearButtons();
            
            for (int i = 0; i < availableColors.Count; i++)
            {
                CreateButton(i);
            }
            
            UpdateColorButtons();
        }
        
        private void CreateButton(int index)
        {
            ColorPair colorPair = availableColors[index];
            GameObject buttonObj = Instantiate(buttonPrefab, colorSelectionPanel.transform);
            Button button = buttonObj.GetComponent<Button>();
            
            SetButtonColors(button, colorPair, isAvailable: true);
            button.onClick.AddListener(() => SetColor(index));
            
            colorButtons.Add(button);
        }
        
        public void UpdateColorButtons()
        {
            for (int i = 0; i < colorButtons.Count; i++)
            {
                ColorPair colorPair = availableColors[i];
                bool isAvailable = !takenColors.Contains(colorPair.bodyColor);
                
                colorButtons[i].interactable = isAvailable;
                SetButtonColors(colorButtons[i], colorPair, isAvailable);
            }
        }
        
        private void SetButtonColors(Button button, ColorPair colorPair, bool isAvailable)
        {
            float alpha = isAvailable ? 1f : 0.3f;
            
            Color bodyColor = colorPair.bodyColor;
            bodyColor.a = alpha;
            button.image.color = bodyColor;
            
            if (button.transform.childCount > 0)
            {
                Image headImage = button.transform.GetChild(0).GetComponent<Image>();
                if (headImage != null)
                {
                    Color headColor = colorPair.headColor;
                    headColor.a = alpha;
                    headImage.color = headColor;
                }
            }
        }
        
        private void ApplyColor(ColorPair colorPair)
        {
            wormMaterial.color = colorPair.bodyColor;
            wormHeadMaterial.color = colorPair.headColor;
            selectColorButtonColor.color = colorPair.bodyColor;
        }
        
        private void ClearButtons()
        {
            foreach (Transform child in colorSelectionPanel.transform)
            {
                Destroy(child.gameObject);
            }
            colorButtons.Clear();
        }
        
        private int FindColorIndex(Color color)
        {
            for (int i = 0; i < availableColors.Count; i++)
            {
                if (availableColors[i].bodyColor == color)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}