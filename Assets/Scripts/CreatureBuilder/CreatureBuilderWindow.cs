using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Cinemachine;

public class CreatureBuilderWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameObject cinemachineCamera;
    private bool isMouseOver = false;
    private bool wasCameraEnabled = false;
    private bool isDragging = false;
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
        
        // Stop dragging if mouse leaves
        isDragging = false;
    }

    // Called when mouse button is pressed down
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isMouseOver)
        {
            isDragging = true;
            cinemachineCamera.SetActive(true);
            Debug.Log("Started dragging");
        }
    }

    // Called when mouse button is released
    public void OnPointerUp(PointerEventData eventData)
    {
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
}