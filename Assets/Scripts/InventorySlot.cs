using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    private CreatureBuilderPartInventory inventory;
    public InventoryItem currentItem;

    public void Initialize(CreatureBuilderPartInventory inv)
    {
        inventory = inv;
        
        // Check if there's already an item in this slot
        currentItem = GetComponentInChildren<InventoryItem>();
        if (currentItem != null)
        {
            currentItem.Initialize(this);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItem draggedItem = eventData.pointerDrag?.GetComponent<InventoryItem>();
        
        if (draggedItem != null)
        {
            // If this slot is empty, accept the item
            if (currentItem == null)
            {
                draggedItem.SetNewSlot(this);
            }
            // If slot has an item, swap them
            else
            {
                InventorySlot oldSlot = draggedItem.originalSlot;
                draggedItem.SetNewSlot(this);
                currentItem.SetNewSlot(oldSlot);
            }
        }
    }

    public void SetItem(InventoryItem item)
    {
        currentItem = item;
    }

    public void ClearItem()
    {
        currentItem = null;
    }
}