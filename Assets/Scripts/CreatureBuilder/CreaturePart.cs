using System.Collections.Generic;
using UnityEngine;

namespace CreatureBuilder
{
    public class CreaturePart : MonoBehaviour
    { 
        public float dragDistance = 0f;

        public GameObject prefab;
        public Camera targetCamera;
        public RectTransform creatureBuilderWindow;
        public Transform endPoint;

        private GameObject falseWormBody;
        
        private Vector3 lastValidPosition;
        private Vector2 lastValidViewport;
        
        private Vector3 dragOffset;
        
        private CreatureBuilder _creatureBuilder;
        private List<GameObject> outlineObjects = new List<GameObject>();
        private Color outlineColor = Color.cyan;
        private float outlineWidth = 0.03f;

        public bool isClamped;
        private bool isSelected;
        private bool isDragging;
    
        void Start()
        {
            _creatureBuilder = GameObject.Find("CreatureBuilder").GetComponent<CreatureBuilder>();
            falseWormBody = GameObject.Find("falseWormBody");
            StartDragging();
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

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if (isSelected)
                {
                    _creatureBuilder.SwitchTo2DCard(prefab);
                    Destroy(gameObject);
                }
            }
        }

        public void StartDragging()
        {
            isSelected = true;
            isDragging = true;
    
            // Calculate the initial drag distance from the camera
            if (dragDistance == 0f)
            {
                dragDistance = Vector3.Distance(targetCamera.transform.position, transform.position);
            }
    
            lastValidPosition = transform.position;
    
            // Calculate initial viewport position based on current position
            Vector3 viewportPos = targetCamera.WorldToViewportPoint(transform.position);
            lastValidViewport = new Vector2(viewportPos.x, viewportPos.y);
    
            HighlightPart();
        }
    
        public void StopDragging()
        {
            isSelected = false;
            isDragging = false;
            RemoveHighlight();
        }

private void Drag() {
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
    
    // If we just started dragging, calculate the offset
    if (!isDragging) {
        isDragging = true;
        // Update dragDistance based on current object position
        Vector3 cameraToObject = transform.position - targetCamera.transform.position;
        dragDistance = Vector3.Dot(cameraToObject, targetCamera.transform.forward);
        
        // Find where the ray intersects the plane at the object's current distance
        Vector3 rayPoint = ray.GetPoint(dragDistance);
        dragOffset = transform.position - rayPoint;
    }
    
    // Always use the current object's distance from camera, not a fixed dragDistance
    Vector3 currentCameraToObject = transform.position - targetCamera.transform.position;
    float currentDragDistance = Vector3.Dot(currentCameraToObject, targetCamera.transform.forward);
    
    // Calculate target position with the offset maintained
    Vector3 targetPosition = ray.GetPoint(currentDragDistance) + dragOffset;
    
    // Apply smoothing when far from camera to reduce jitter
    float distanceFromCamera = currentCameraToObject.magnitude;
    float smoothingFactor = Mathf.Clamp01(distanceFromCamera / 50f); // Adjust 50f based on your scale
    targetPosition = Vector3.Lerp(targetPosition, transform.position, smoothingFactor * 0.3f);
    
    // Rotate to falseCreatureBody
    RotateTowardWormBody();
    
    // Check if we can move to the target position
    if (CanMoveTo(targetPosition))
    {
        transform.position = targetPosition;
        lastValidPosition = targetPosition;
        lastValidViewport = new Vector2(viewportX, viewportY);
    }
    else
    {
        // Use last valid viewport coordinates instead
        ray = targetCamera.ViewportPointToRay(new Vector3(lastValidViewport.x, lastValidViewport.y, 0));
        transform.position = ray.GetPoint(currentDragDistance) + dragOffset;
    }
    
    // Clamp to worm body if close enough
    TryToClampToWormBody();
}

        private void TryToClampToWormBody() {
            if (falseWormBody == null || endPoint == null) return;
    
            float clampDistance = GameParameters.distanceToClampPart;
    
            // Cast a ray from the END of this part towards the worm body center
            Vector3 directionToWorm = falseWormBody.transform.position - endPoint.position;
            float distanceToCenter = directionToWorm.magnitude;
    
            if (distanceToCenter < 0.001f) return;
    
            Ray ray = new Ray(endPoint.position, directionToWorm.normalized);
            RaycastHit hit;
    
            // Raycast towards the worm body
            if (Physics.Raycast(ray, out hit, distanceToCenter)) {
                // Check if we hit the worm body
                if (hit.collider.gameObject == falseWormBody) {
                    float distanceToSurface = hit.distance;
            
                    // If within clamp distance, move the part so endPoint touches the surface
                    if (distanceToSurface <= clampDistance) {
                        // Calculate offset: how far the endPoint is from transform center
                        Vector3 offset = endPoint.position - transform.position;
                
                        // Move the part so the endPoint is at the hit point
                        transform.position = hit.point - offset;
                        isClamped = true;
                    }
                    else
                    {
                        isClamped = false;
                    }
                }
            }
        }
        private void RotateTowardWormBody() {
            if (falseWormBody == null || endPoint == null) return;
    
            // Get the local direction from center to endPoint (in the part's local space)
            Vector3 localEndDirection = transform.InverseTransformPoint(endPoint.position).normalized;
    
            // Calculate direction from this object's center to the worm body
            Vector3 directionToTarget = (falseWormBody.transform.position - transform.position).normalized;
    
            if (directionToTarget.sqrMagnitude < 0.001f) return;
    
            // Calculate base rotation that points forward axis at target
            Quaternion targetRotation = Quaternion.LookRotation(-directionToTarget);
    
            // Calculate the offset needed to align the endPoint direction with forward
            Quaternion offsetRotation = Quaternion.FromToRotation(localEndDirection, Vector3.forward);
    
            // Apply both rotations
            transform.rotation = targetRotation * Quaternion.Inverse(offsetRotation);
        }

        private bool CanMoveTo(Vector3 targetPosition)
        {
            // Get all colliders on this object
            Collider[] colliders = GetComponentsInChildren<Collider>();
            
            foreach (Collider col in colliders)
            {
                // Calculate the offset from transform to collider
                Vector3 offset = col.bounds.center - transform.position;
                Vector3 newColliderCenter = targetPosition + offset;
                
                // Check for overlaps at the target position
                Collider[] overlaps = Physics.OverlapBox(
                    newColliderCenter,
                    col.bounds.extents,
                    transform.rotation
                );
                
                // Check if any overlapping colliders aren't part of this object
                foreach (Collider overlap in overlaps)
                {
                    if (!overlap.transform.IsChildOf(transform) && overlap.transform != transform)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        private void HighlightPart()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
        
            foreach (Renderer renderer in renderers)
            {
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    GameObject outlineObj = new GameObject(renderer.name + "_Outline");
                    outlineObj.transform.SetParent(renderer.transform);
                    outlineObj.transform.localPosition = Vector3.zero;
                    outlineObj.transform.localRotation = Quaternion.identity;
                    outlineObj.transform.localScale = Vector3.one;
                
                    MeshFilter outlineMF = outlineObj.AddComponent<MeshFilter>();
                    MeshRenderer outlineMR = outlineObj.AddComponent<MeshRenderer>();
                
                    // Create inverted mesh for outline
                    Mesh outlineMesh = CreateInvertedMesh(meshFilter.mesh, outlineWidth);
                    outlineMF.mesh = outlineMesh;
                
                    // Create unlit outline material
                    Material outlineMat = new Material(Shader.Find("Unlit/Color"));
                    outlineMat.color = outlineColor;
                    outlineMR.material = outlineMat;
                
                    // Render behind the original mesh
                    outlineMR.sortingOrder = -1;
                
                    outlineObjects.Add(outlineObj);
                }
            }
        }

        private Mesh CreateInvertedMesh(Mesh originalMesh, float thickness)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = originalMesh.vertices;
            mesh.normals = originalMesh.normals;
            mesh.uv = originalMesh.uv;
        
            // Invert triangles to flip faces inward
            int[] triangles = originalMesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int temp = triangles[i];
                triangles[i] = triangles[i + 2];
                triangles[i + 2] = temp;
            }
            mesh.triangles = triangles;
        
            // Expand vertices along inverted normals
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] += normals[i] * thickness;
            }
            mesh.vertices = vertices;
        
            mesh.RecalculateBounds();
            return mesh;
        }

        private void RemoveHighlight()
        {
            foreach (GameObject obj in outlineObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }
            outlineObjects.Clear();
        }
    
    }
}
