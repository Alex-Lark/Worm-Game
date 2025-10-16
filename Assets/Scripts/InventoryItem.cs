using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public InventorySlot originalSlot;
    private InventorySlot currentSlot;
    private Transform originalParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 startPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        // Add CanvasGroup if not present
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Initialize(InventorySlot slot)
    {
        currentSlot = slot;
        originalSlot = slot;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = rectTransform.position;
        originalParent = transform.parent;
        originalSlot = currentSlot;
        
        // Move to root canvas so it renders on top
        transform.SetParent(canvas.transform);
        
        // Make it semi-transparent and non-blocking
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Check if we dropped on a valid slot
        bool droppedOnSlot = false;
        if (eventData.pointerEnter != null)
        {
            InventorySlot slot = eventData.pointerEnter.GetComponent<InventorySlot>();
            if (slot != null)
            {
                droppedOnSlot = true;
            }
        }

        // If not dropped on a slot, snap back
        if (!droppedOnSlot)
        {
            transform.SetParent(originalSlot.transform);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    public void SetNewSlot(InventorySlot newSlot)
    {
        // Clear from current slot
        if (currentSlot != null)
        {
            currentSlot.ClearItem();
        }

        // Move to new slot
        transform.SetParent(newSlot.transform);
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Update references
        currentSlot = newSlot;
        newSlot.SetItem(this);
    }
}