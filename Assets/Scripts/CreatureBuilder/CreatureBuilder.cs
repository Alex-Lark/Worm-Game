using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreatureBuilder
{
    [Serializable]
    public class PartPair
    {
        public GameObject cardPrefab;   // The card prefab GameObject
        public GameObject part3DPrefab; // The corresponding 3D model prefab
    }

    public class CreatureBuilder : MonoBehaviour
    {
        [SerializeField] private List<PartPair> partPairs = new List<PartPair>();
        
        public Camera targetCamera;
        public RectTransform creatureBuilderWindow;
        
        [SerializeField] private float spawnDistance = 5f;

        private Dictionary<string, GameObject> _prefabMapping = new Dictionary<string, GameObject>();

        private void Awake()
        {
            InitializePrefabMapping();
        }

        private void InitializePrefabMapping()
        {
            _prefabMapping.Clear();
            foreach (var pair in partPairs)
            {
                if (pair.cardPrefab != null && pair.part3DPrefab != null)
                {
                    // Use the prefab's name as the key
                    string keyName = pair.cardPrefab.name;
                    _prefabMapping[keyName] = pair.part3DPrefab;
                    Debug.Log($"Mapped: {keyName} -> {pair.part3DPrefab.name}");
                }
            }
        }

        public void SwitchTo3DPart(GameObject cardPrefab)
        {
            if (cardPrefab == null)
            {
                Debug.LogWarning("Card prefab is null");
                return;
            }

            // Use the name for lookup
            string keyName = cardPrefab.name.Replace("(Clone)", "").Trim();
            
            if (_prefabMapping.TryGetValue(keyName, out GameObject prefab3D))
            {
                Vector3 spawnPosition = CalculateWorldSpawnPosition();
                SpawnPartInWorld(prefab3D, spawnPosition);
            }
            else
            {
                Debug.LogWarning($"No 3D prefab mapping found for card: {keyName}");
            }
        }

        public void SwitchTo2DCard(GameObject partPrefab)
        {
            if (partPrefab == null)
            {
                Debug.LogWarning("Part prefab is null");
                return;
            }

            // Use the name for lookup
            string keyName = partPrefab.name.Replace("(Clone)", "").Trim();
    
            // Find the matching pair by searching for the 3D prefab
            foreach (var pair in partPairs)
            {
                if (pair.part3DPrefab != null && pair.part3DPrefab.name == keyName)
                {
                    // Found the matching card prefab
                    SpawnCardInInventory(pair.cardPrefab);
            
                    // Destroy the 3D part
                    Destroy(partPrefab);
                    return;
                }
            }
    
            Debug.LogWarning($"No 2D card mapping found for part: {keyName}");
        }
        
        private void SpawnCardInInventory(GameObject cardPrefab)
        {
            // Find the inventory to spawn the card back into
            CreatureBuilderPartInventory inventory = FindObjectOfType<CreatureBuilderPartInventory>();
    
            if (inventory != null)
            {
                bool success = inventory.AddCardToInventory(cardPrefab);
                if (!success)
                {
                    Debug.LogWarning("Failed to add card to inventory");
                }
            }
            else
            {
                Debug.LogWarning("Inventory not found");
            }
        }

        private Vector3 CalculateWorldSpawnPosition()
        {
            // Get the screen-space corners of the CreatureBuilderWindow
            Vector3[] corners = new Vector3[4];
            creatureBuilderWindow.GetWorldCorners(corners);
    
            // corners[0] = bottom-left, corners[1] = top-left, corners[2] = top-right, corners[3] = bottom-right
            Vector2 mousePos = Input.mousePosition;
    
            Debug.Log($"Mouse: {mousePos}, BottomLeft: {corners[0]}, TopRight: {corners[2]}");
    
            // Calculate normalized position within the window (0-1 range)
            float viewportX = Mathf.InverseLerp(corners[0].x, corners[2].x, mousePos.x);
            float viewportY = Mathf.InverseLerp(corners[0].y, corners[2].y, mousePos.y);
    
            // Clamp to 0-1 range in case mouse is outside bounds
            viewportX = Mathf.Clamp01(viewportX);
            viewportY = Mathf.Clamp01(viewportY);
    
            Debug.Log($"ViewportX: {viewportX}, ViewportY: {viewportY}");

            // Create a ray from the 3D camera through the viewport point
            Ray ray = targetCamera.ViewportPointToRay(new Vector3(viewportX, viewportY, 0));
            return ray.GetPoint(spawnDistance);
        }
        private void SpawnPartInWorld(GameObject prefab, Vector3 position)
        {
            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            instance.name = prefab.name;
            instance.GetComponent<CreaturePart>().targetCamera = targetCamera;
            instance.GetComponent<CreaturePart>().creatureBuilderWindow = creatureBuilderWindow;
        }
    }
}