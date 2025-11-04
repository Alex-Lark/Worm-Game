using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CreatureBuilder
{
    public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public String partName;
        
        private GameObject _creatureBuilderWindow;
        private CreatureBuilder _creatureBuilder;
    
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
            
            transform.SetParent(_canvas.transform);
            
            _canvasGroup.alpha = 0.6f;
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.position = eventData.position;
            
            CheckIfOverCreatureBuilderWindow(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            
            bool droppedOnSlot = false;
            if (eventData.pointerEnter != null)
            {
                InventorySlot slot = eventData.pointerEnter.GetComponent<InventorySlot>();
                if (slot != null)
                {
                    droppedOnSlot = true;
                }
            }
            
            if (!droppedOnSlot)
            {
                transform.SetParent(originalSlot.transform);
                _rectTransform.anchoredPosition = Vector2.zero;
            }
        }
        
        private void CheckIfOverCreatureBuilderWindow(PointerEventData eventData)
        {
            foreach (var raycastResult in eventData.hovered)
            {
                if (raycastResult == _creatureBuilderWindow || raycastResult.transform.IsChildOf(_creatureBuilderWindow.transform))
                {
                    OnEnterCreatureBuilder();
                }
            }
        }
        
        private void OnEnterCreatureBuilder()
        {
            print("entered creature builder");
            _creatureBuilder.SwitchTo3DPart(partName);
            Destroy(gameObject);
        }

        public void SetNewSlot(InventorySlot newSlot)
        {
            if (_currentSlot != null)
            {
                _currentSlot.ClearItem();
            }
            
            transform.SetParent(newSlot.transform);
            _rectTransform.anchoredPosition = Vector2.zero;
            
            _currentSlot = newSlot;
            newSlot.SetItem(this);
        }
    }
}