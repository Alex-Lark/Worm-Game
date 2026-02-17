using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameLoop.GameLobby
{
    public class ColorSelection : MonoBehaviour
    {
        #region Public Variables
        
        public GameObject colorSelectionPanel;
        public GameObject buttonPrefab; 
        
        public Material wormMaterial;
        public Material wormHeadMaterial;
        
        public Image selectColorButtonColor;
        
        public List<Button> colorButtons = new List<Button>();

        public List<ColorPair> availableColors = new List<ColorPair> { };
        
        #endregion
        
        private static HashSet<Color> takenColors = new HashSet<Color>();
        private int currentColorIndex = -1;
        
        #region Built-In Methods
        
        void Start()
        {
            selectColorButtonColor.color = wormMaterial.color;
            CreateColorButtons();
        }
        
        #endregion
        
        #region Public Methods
        
        public void RefreshButtons()
        {
            UpdateColorButtons();
        }
        
        public void SetColor(int colorIndex)
        {
            if (colorIndex < 0 || colorIndex >= availableColors.Count)
            {
                Debug.LogError($"Invalid color index: {colorIndex}");
                return;
            }
    
            ColorPair selectedColor = availableColors[colorIndex];
            
            if (takenColors.Contains(selectedColor.bodyColor))
            {
                Debug.LogWarning("This color is already taken!");
                return;
            }
            
            if (currentColorIndex >= 0 && currentColorIndex < availableColors.Count)
            {
                takenColors.Remove(availableColors[currentColorIndex].bodyColor);
            }
            
            wormMaterial.color = selectedColor.bodyColor;
            wormHeadMaterial.color = selectedColor.headColor;
            selectColorButtonColor.color = selectedColor.bodyColor;
            
            currentColorIndex = colorIndex;
            takenColors.Add(selectedColor.bodyColor);
            
            PlayerRegister.UpdateColor(selectedColor.bodyColor);
            UpdateColorButtons();
            GetComponent<GameLobby>().CloseColorSelectionPanel();
        }
        
        public void SetInitialColor()
        {
            Color desiredColor = wormMaterial.color;
            
            for (int i = 0; i < availableColors.Count; i++)
            {
                if (availableColors[i].bodyColor == desiredColor && !takenColors.Contains(desiredColor))
                {
                    SetColor(i);
                    return;
                }
            }
            
            for (int i = 0; i < availableColors.Count; i++)
            {
                if (!takenColors.Contains(availableColors[i].bodyColor))
                {
                    SetColor(i);
                    return;
                }
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void CreateColorButtons()
        {
            foreach (Transform child in colorSelectionPanel.transform)
            {
                Destroy(child.gameObject);
            }
            colorButtons.Clear();
            
            for (int i = 0; i < availableColors.Count; i++)
            {
                ColorPair colorPair = availableColors[i];
                
                GameObject buttonObj = Instantiate(buttonPrefab, colorSelectionPanel.transform);
                Button button = buttonObj.GetComponent<Button>();
                
                button.image.color = colorPair.bodyColor;
                if (buttonObj.transform.childCount > 0)
                {
                    Image headImage = buttonObj.transform.GetChild(0).GetComponent<Image>();
                    if (headImage != null)
                    {
                        headImage.color = colorPair.headColor;
                    }
                }
                
                // Add click listener
                int index = i;
                button.onClick.AddListener(() => SetColor(index));
                
                colorButtons.Add(button);
            }
            
            UpdateColorButtons();
        }
        
        private void UpdateColorButtons()
        {
            for (int i = 0; i < colorButtons.Count && i < availableColors.Count; i++)
            {
                Button button = colorButtons[i];
                ColorPair colorPair = availableColors[i];
                
                bool isTaken = takenColors.Contains(colorPair.bodyColor);
                button.interactable = !isTaken;
                
                // Optional: Visual feedback for taken colors (dim them)
                if (isTaken)
                {
                    Color dimmedBody = colorPair.bodyColor;
                    dimmedBody.a = 0.3f;
                    button.image.color = dimmedBody;
                    
                    if (button.transform.childCount > 0)
                    {
                        Image headImage = button.transform.GetChild(0).GetComponent<Image>();
                        if (headImage != null)
                        {
                            Color dimmedHead = colorPair.headColor;
                            dimmedHead.a = 0.3f;
                            headImage.color = dimmedHead;
                        }
                    }
                }
                else
                {
                    button.image.color = colorPair.bodyColor;
                    
                    if (button.transform.childCount > 0)
                    {
                        Image headImage = button.transform.GetChild(0).GetComponent<Image>();
                        if (headImage != null)
                        {
                            headImage.color = colorPair.headColor;
                        }
                    }
                }
            }
        }
        
        #endregion
        
        // Call this when a player disconnects to free up their color
        public static void FreeColor(Color color)
        {
            takenColors.Remove(color);
        }
        
        // Call this when receiving color updates from other players
        public static void MarkColorAsTaken(Color color)
        {
            takenColors.Add(color);
        }
        
        // Clear all taken colors (useful when returning to menu)
        public static void ResetTakenColors()
        {
            takenColors.Clear();
        }
    }
}
