using System.Collections.Generic;
using UnityEngine;

namespace CreatureBuilder
{
    public class CreatureBuilderPartInventory : MonoBehaviour
    {
        private InventorySlot[] slots;

        void Awake()
        {
            // Get all child slots
            slots = GetComponentsInChildren<InventorySlot>();

            // Initialize each slot
            foreach (var slot in slots)
            {
                slot.Initialize(this);
            }
            
            AddStartingCardsToInventory();
        }
        
        public bool AddCardToInventory(GameObject cardPrefab)
        {
            InventorySlot emptySlot = GetEmptySlot();
            
            if (emptySlot != null)
            {
                // Instantiate the card as a child of the empty slot
                GameObject cardInstance = Instantiate(cardPrefab, emptySlot.transform);
                
                // Reset the card's RectTransform
                RectTransform cardRect = cardInstance.GetComponent<RectTransform>();
                if (cardRect != null)
                {
                    cardRect.anchoredPosition = Vector2.zero;
                    cardRect.localScale = Vector3.one;
                }
                
                // Initialize the card with the slot
                InventoryItem item = cardInstance.GetComponent<InventoryItem>();
                if (item != null)
                {
                    item.Initialize(emptySlot);
                    emptySlot.SetItem(item);
                    return true;
                }
                else
                {
                    Debug.LogWarning("Card prefab doesn't have InventoryItem component");
                    Destroy(cardInstance);
                    return false;
                }
            }
            else
            {
                Debug.LogWarning("No empty inventory slot available");
                return false;
            }
        }
        
        private void AddStartingCardsToInventory()
        {
            List<GameObject> partCards = Player.Player.Instance.wormPartsInInventory;
            foreach (var part in partCards)
            {
                AddCardToInventory(part);
            }
            Player.Player.Instance.wormPartsInInventory.Clear();
        }

        private InventorySlot GetEmptySlot()
        {
            foreach (var slot in slots)
            {
                if (slot.currentItem == null)
                {
                    return slot;
                }
            }

            return null;
        }
        
    }
}