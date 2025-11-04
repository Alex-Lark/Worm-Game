using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CreatureBuilder
{
    public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public String partName;
        
        private GameObject _creatureBuilderWindow;
        private CreatureBuilder _creatureBuilder;
        private bool _isOverCreatureBuilder = false;
    
        public InventorySlot originalSlot;
        private InventorySlot _currentSlot;
        private Transform _originalParent;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Vector3 _startPosition;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
        
            // Add CanvasGroup if not present
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            _creatureBuilderWindow = GameObject.Find("Creature Builder Window");
            _creatureBuilder = GameObject.Find("CreatureBuilder").GetComponent<CreatureBuilder>();
        }

        public void Initialize(InventorySlot slot)
        {
            _currentSlot = slot;
            originalSlot = slot;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPosition = _rectTransform.position;
            _originalParent = transform.parent;
            originalSlot = _currentSlot;
        
            // Move to root canvas so it renders on top
            transform.SetParent(_canvas.transform);
        
            // Make it semi-transparent and non-blocking
            _canvasGroup.alpha = 0.6f;
            _canvasGroup.blocksRaycasts = false;
            
            _isOverCreatureBuilder = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.position = eventData.position;
            
            // Check if currently over the Creature Builder Window
            bool wasOver = _isOverCreatureBuilder;
            _isOverCreatureBuilder = IsOverCreatureBuilderWindow(eventData);
            
            // Detect when entering or leaving the window
            if (_isOverCreatureBuilder && !wasOver)
            {
                Debug.Log("Entered Creature Builder Window");
                OnEnterCreatureBuilder();
            }
            else if (!_isOverCreatureBuilder && wasOver)
            {
                Debug.Log("Left Creature Builder Window");
                OnExitCreatureBuilder();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;

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
                _rectTransform.anchoredPosition = Vector2.zero;
            }
            
            _isOverCreatureBuilder = false;
        }
        
        private bool IsOverCreatureBuilderWindow(PointerEventData eventData)
        {
            if (_creatureBuilderWindow == null) return false;
            
            // Check if the pointer is over the Creature Builder Window
            foreach (var raycastResult in eventData.hovered)
            {
                if (raycastResult == _creatureBuilderWindow || raycastResult.transform.IsChildOf(_creatureBuilderWindow.transform))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private void OnEnterCreatureBuilder()
        {
            // Get the prefab this instance was created from
            GameObject prefabSource = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
    
            if (prefabSource != null)
            {
                _creatureBuilder.SwitchTo3DPart(partName);
            }
            else
            {
                Debug.LogWarning($"Could not find prefab source for: {gameObject.name}");
            }
            
            //TODO: delete
        }
        
        private void OnExitCreatureBuilder()
        {
            // Visual feedback when leaving the window
            _canvasGroup.alpha = 0.6f;
        }

        public void SetNewSlot(InventorySlot newSlot)
        {
            // Clear from current slot
            if (_currentSlot != null)
            {
                _currentSlot.ClearItem();
            }

            // Move to new slot
            transform.SetParent(newSlot.transform);
            _rectTransform.anchoredPosition = Vector2.zero;
            
            // Update references
            _currentSlot = newSlot;
            newSlot.SetItem(this);
        }
    }
}