using CreatureBuilder;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Cinemachine;

public class CreatureBuilderWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameObject cinemachineCamera;
    public GameObject targetCameraObject;
    
    private Camera targetCamera;
    private bool isMouseOver = false;
    private bool wasCameraEnabled = false;
    private bool isDragging = false;
    private bool isDraggingPart = false;
    private CinemachineInputProvider inputProvider;

    void Start()
    {
        // Store the initial camera state
        if (cinemachineCamera != null)
        {
            wasCameraEnabled = cinemachineCamera.activeSelf;
            
            // Get the input provider component
            inputProvider = cinemachineCamera.GetComponent<CinemachineInputProvider>();
            
            // Start with camera disabled if not over the image
            if (!isMouseOver)
            {
                cinemachineCamera.SetActive(false);
            }
        }
        targetCamera = targetCameraObject.GetComponent<Camera>();
    }

    void Update()
    {
        // Enable/disable input provider based on drag state
        if (inputProvider != null)
        {
            inputProvider.enabled = isDragging && isMouseOver;
        }
    }

    // Called when mouse enters the UI element
    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        
        if (cinemachineCamera != null)
        {
            Debug.Log("Mouse entered - Camera enabled");
        }
    }

    // Called when mouse exits the UI element
    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        
        if (cinemachineCamera != null)
        {
            Debug.Log("Mouse exited - Camera disabled");
        }

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
                Debug.Log($"Clicked on creature part: {hitPart.name}");
                hitPart.GetComponent<CreaturePart>().StartDragging();
            }
            else
            {
                isDragging = true;
                cinemachineCamera.SetActive(true);
                Debug.Log("Started dragging");
            }
        }
    }
    
    // Called when mouse button is released
    public void OnPointerUp(PointerEventData eventData)
    {
        isDraggingPart = false;
        isDragging = false;
        cinemachineCamera.SetActive(false);
        Debug.Log("Stopped dragging");
    }

    void OnDisable()
    {
        // Restore camera state when this UI element is disabled
        if (cinemachineCamera != null)
        {
            cinemachineCamera.SetActive(wasCameraEnabled);
        }
        
        isDragging = false;
        if (inputProvider != null)
        {
            inputProvider.enabled = false;
        }
    }
    
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
        RaycastHit hit;
    
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("CreaturePart"))
            {
                hitObject = hit.collider.gameObject;
                return true;
            }
        }
    
        return false;
    }
}