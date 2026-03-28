using System.Collections.Generic;
using GameLoop.multiplayer;
using Player;
using PurrNet;
using UnityEngine;
using UnityEngine.UI;

namespace GameLoop.GameLobby
{
    public class ColorSelection : MonoBehaviour
    {
        public GameObject colorSelectionPanel;
        public GameObject buttonPrefab;
        public Image selectColorButtonColor;
        public Material wormMaterial;
        public List<ColorPair> availableColors = new List<ColorPair>();

        private List<Button> colorButtons = new List<Button>();
        private HashSet<int> takenIndices = new HashSet<int>();

        void Start() => CreateColorButtons();

        public void RefreshTakenColors(HashSet<int> taken)
        {
            takenIndices = taken;
            UpdateColorButtons();
        }

        public void SetInitialColor()
        {
            // Try to claim the color matching the worm's current material
            for (int i = 0; i < availableColors.Count; i++)
            {
                if (availableColors[i].bodyMaterial == wormMaterial && !takenIndices.Contains(i))
                {
                    SetColor(i);
                    return;
                }
            }

            // Fall back to first available
            for (int i = 0; i < availableColors.Count; i++)
            {
                if (!takenIndices.Contains(i))
                {
                    SetColor(i);
                    return;
                }
            }

            Debug.LogWarning("SetInitialColor: no available colors.");
        }

        public void SetColor(int colorIndex)
        {
            if (takenIndices.Contains(colorIndex))
            {
                Debug.LogWarning("Color already taken.");
                return;
            }

            FindFirstObjectByType<ColorSync>().SendColorUpdate(colorIndex, LocalPlayer.Instance);
            GetComponent<GameLobby>().CloseColorSelectionPanel();
        }

        private void CreateColorButtons()
        {
            foreach (Transform child in colorSelectionPanel.transform)
                Destroy(child.gameObject);
            colorButtons.Clear();

            for (int i = 0; i < availableColors.Count; i++)
            {
                int index = i;
                GameObject obj = Instantiate(buttonPrefab, colorSelectionPanel.transform);
                Button button = obj.GetComponent<Button>();
                button.onClick.AddListener(() => SetColor(index));
                colorButtons.Add(button);
            }

            UpdateColorButtons();
        }

        public void UpdateColorButtons()
        {
            for (int i = 0; i < colorButtons.Count; i++)
            {
                ColorPair pair = availableColors[i];
                bool available = !takenIndices.Contains(i);
                float alpha = available ? 1f : 0.3f;

                colorButtons[i].interactable = available;

                Color body = pair.bodyMaterial.GetColor("_Base_Color");
                body.a = alpha;
                colorButtons[i].image.color = body;

                if (colorButtons[i].transform.childCount > 0)
                {
                    Image headImg = colorButtons[i].transform.GetChild(0).GetComponent<Image>();
                    if (headImg != null)
                    {
                        Color head = pair.headMaterial.color;
                        head.a = alpha;
                        headImg.color = head;
                    }
                }
            }
        }
    }
}