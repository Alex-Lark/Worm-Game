using UnityEngine;
using CreatureBuilder;

public class AxisRotationHandler : MonoBehaviour
{
    public Vector3 localAxis = Vector3.up;
    public float rotationSpeed = 0.4f;
    public Transform targetPart;
    public GameObject hostPart;

    private bool isDragging;
    private Vector3 lastMousePosition;
    private PartDragging partDragging;

    void Awake()
    {
        partDragging = GetComponentInParent<PartDragging>();
    }

    void Update()
    {
        if (!isDragging || partDragging == null || partDragging.targetCamera == null)
            return;

        Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
        lastMousePosition = Input.mousePosition;

        Vector3 worldAxis = targetPart.TransformDirection(localAxis);

        float angle = Vector3.Dot(mouseDelta, partDragging.targetCamera.transform.right) * rotationSpeed;

        targetPart.Rotate(worldAxis, angle, Space.World);
    }
    
    public void StartRotation()
    {
        if (partDragging == null) return;
        
        isDragging = true;
        lastMousePosition = Input.mousePosition;

        if (partDragging != null)
            partDragging.enabled = false;
    }

    public void StopRotation()
    {
        if (partDragging == null) return;
        
        isDragging = false;

        if (partDragging != null)
            partDragging.enabled = true;
    }
}
