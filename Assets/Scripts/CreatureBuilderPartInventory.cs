using UnityEngine;

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
    }

    public InventorySlot[] GetSlots()
    {
        return slots;
    }
}