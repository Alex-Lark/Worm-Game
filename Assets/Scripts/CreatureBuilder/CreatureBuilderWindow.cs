using NUnit.Framework.Constraints;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CreatureBuilder
{
    public class CreatureBuilderWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IInputAxisController
    {
        #region public variables
        [Header("Public Variables")]
        
        public GameObject cinemachineCamera;
        public GameObject targetCameraObject;
        
        #endregion
        
        #region private variables
        [Header("Private Variables")]
        
        private Camera targetCamera;
        private bool isMouseOver = false;
        private bool wasCameraEnabled = false;
        private bool isDragging = false;
        private bool isDraggingPart = false;
        private IInputAxisController inputProvider;
        private GameObject selectedPart;
        
        #endregion

        #region Built-In Methods
        
        void Start()
        {
            if (cinemachineCamera != null)
            {
                wasCameraEnabled = cinemachineCamera.activeSelf;
                inputProvider = cinemachineCamera.GetComponent<IInputAxisController>();
                
                if (!isMouseOver)
                {
                    cinemachineCamera.SetActive(false);
                }
                
            }
            targetCamera = targetCameraObject.GetComponent<Camera>();
        }
        
        void OnDisable()
        {
            if (cinemachineCamera != null)
            {
                cinemachineCamera.SetActive(wasCameraEnabled);
            }
            isDragging = false;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            isMouseOver = true;
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseOver = false;
            
            if (!isDraggingPart)
            {
                isDragging = false;
            }
        }
    
        public void OnPointerDown(PointerEventData eventData)
        {
            if (isMouseOver)
            {
                if (IsOverCreaturePart(out GameObject hitPart))
                {
                    isDraggingPart = true;
                    hitPart.GetComponent<PartDragging>().StartDragging();
                    selectedPart = hitPart;
                }
                else
                {
                    selectedPart.GetComponent<PartDragging>().DeselectPart();
                    isDragging = true;
                    cinemachineCamera.SetActive(true);
                }
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            isDraggingPart = false;
            isDragging = false;
            cinemachineCamera.SetActive(false);
        }

        public bool ControllersAreValid()
        {
            return true;
        }
        
        public void SynchronizeControllers()
        {
            
        }
        
        #endregion
    
        #region Private Methods
        
        private bool IsOverCreaturePart(out GameObject hitObject)
        {
            hitObject = null;

            if (targetCamera == null || gameObject.GetComponent<RectTransform>() == null)
                return false;

            Vector3[] corners = new Vector3[4];
            gameObject.GetComponent<RectTransform>().GetWorldCorners(corners);

            Vector2 mousePos = Input.mousePosition;

            float viewportX = Mathf.InverseLerp(corners[0].x, corners[2].x, mousePos.x);
            float viewportY = Mathf.InverseLerp(corners[0].y, corners[2].y, mousePos.y);

            viewportX = Mathf.Clamp01(viewportX);
            viewportY = Mathf.Clamp01(viewportY);

            Ray ray = targetCamera.ViewportPointToRay(new Vector3(viewportX, viewportY, 0));
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Transform current = hit.collider.transform;
                
                int safetyCounter = 0; // prevent infinite loops
                while (current != null && safetyCounter < 10)
                {
                    if (current.CompareTag("CreaturePart"))
                    {
                        hitObject = current.gameObject;
                        return true;
                    }
                    current = current.parent;
                    safetyCounter++;
                }
            }

            return false;
        }
        
        #endregion
    }
}