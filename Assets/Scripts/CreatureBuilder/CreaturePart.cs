using UnityEngine;

public class CreaturePart : MonoBehaviour
{
    [SerializeField] private float dragDistance = 5f;
    
    public Camera targetCamera;
    public RectTransform creatureBuilderWindow;
    
    private bool isSelected;
    private bool isDragging;
    
    void Start()
    {
        isSelected = true;
        isDragging = true;
        
    }
    
    void Update()
    {
        if (isDragging)
        {
            Drag();
        }
        if (Input.GetMouseButtonUp(0))
        {
            StopDragging();
        }
    }

    private void Drag()
    {
        // Get the screen-space corners of the CreatureBuilderWindow
        RectTransform creatureBuilderWindow = GameObject.Find("Creature Builder Window").GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        creatureBuilderWindow.GetWorldCorners(corners);
    
        Vector2 mousePos = Input.mousePosition;
    
        // Calculate normalized position within the window (0-1 range)
        float viewportX = Mathf.InverseLerp(corners[0].x, corners[2].x, mousePos.x);
        float viewportY = Mathf.InverseLerp(corners[0].y, corners[2].y, mousePos.y);
    
        // Clamp to 0-1 range
        viewportX = Mathf.Clamp01(viewportX);
        viewportY = Mathf.Clamp01(viewportY);

        // Create a ray from the 3D camera through the viewport point
        Ray ray = targetCamera.ViewportPointToRay(new Vector3(viewportX, viewportY, 0));
        transform.position = ray.GetPoint(dragDistance);
    }
    
    private void StopDragging()
    {
        isSelected = false;
        isDragging = false;
    }
}
