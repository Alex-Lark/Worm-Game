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
        private HashSet<Material> takenMaterials = new HashSet<Material>();

        void Start()
        {
            CreateColorButtons();
        }

        public void UpdateMultiplayerColors(List<Material> playerMaterials)
        {
            takenMaterials.Clear();
            foreach (Material material in playerMaterials)
            {
                takenMaterials.Add(material);
                Debug.Log("Updating colors from multiplayer with material: " + material.name);
            }
            UpdateColorButtons();
        }

        public void SetColor(int colorIndex)
        {
            ColorPair selectedColor = availableColors[colorIndex];

            if (takenMaterials.Contains(selectedColor.bodyMaterial))
            {
                Debug.LogWarning("This color is already taken!");
                return;
            }

            // Remove old material from taken set
            if (currentColorIndex >= 0)
            {
                takenMaterials.Remove(availableColors[currentColorIndex].bodyMaterial);
            }

            // Apply new color
            ApplyColor(selectedColor);

            // Mark new material as taken
            currentColorIndex = colorIndex;
            takenMaterials.Add(selectedColor.bodyMaterial);

            // Send color update via network
            FindFirstObjectByType<ColorSync>().SendColorUpdate(selectedColor.bodyMaterial);

            UpdateColorButtons();
            GetComponent<GameLobby>().CloseColorSelectionPanel();
        }

        public void SetInitialColor() { }

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
                bool isAvailable = !takenMaterials.Contains(colorPair.bodyMaterial);

                colorButtons[i].interactable = isAvailable;
                SetButtonColors(colorButtons[i], colorPair, isAvailable);
            }
        }

        private void SetButtonColors(Button button, ColorPair colorPair, bool isAvailable)
        {
            float alpha = isAvailable ? 1f : 0.3f;

            Color bodyColor = colorPair.bodyMaterial.GetColor("_Base_Color");
            bodyColor.a = alpha;
            button.image.color = bodyColor;

            if (button.transform.childCount > 0)
            {
                Image headImage = button.transform.GetChild(0).GetComponent<Image>();
                if (headImage != null)
                {
                    Color headColor = colorPair.headMaterial.color; // head material may still use _Color
                    headColor.a = alpha;
                    headImage.color = headColor;
                }
            }
        }

        private void ApplyColor(ColorPair colorPair)
        {
            wormMaterial.SetColor("_Base_Color", colorPair.bodyMaterial.GetColor("_Base_Color"));
            wormHeadMaterial.color = colorPair.headMaterial.color;
            selectColorButtonColor.color = colorPair.bodyMaterial.GetColor("_Base_Color");
        }

        private void ClearButtons()
        {
            foreach (Transform child in colorSelectionPanel.transform)
            {
                Destroy(child.gameObject);
            }
            colorButtons.Clear();
        }

        private int FindMaterialIndex(Material material)
        {
            for (int i = 0; i < availableColors.Count; i++)
            {
                if (availableColors[i].bodyMaterial == material)
                    return i;
            }
            return -1;
        }
    }
}