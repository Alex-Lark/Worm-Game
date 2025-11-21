using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreatureBuilder
{
    public class CreaturePart : MonoBehaviour
    { 
        public float dragDistance = 0f;

        public CreaturePartData partData;
        public GameObject prefab => partData != null ? partData.prefab : null;
        public Camera targetCamera;
        public RectTransform creatureBuilderWindow;
        public Transform endPoint;

        private GameObject falseWormBody;
        
        private Vector3 lastValidPosition;
        private Vector3 lastMouseWorldPos;
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
            if (!isClamped)
            {
                StartDragging();
            }
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

        private void OnDisable()
        {
            isSelected = false;
            isDragging = false;
            isClamped = false;
        }

        public void Clamp()
        {
            isClamped = true;
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
        
        lastMouseWorldPos = rayPoint;
    }
    
    // Always use the current object's distance from camera
    Vector3 currentCameraToObject = transform.position - targetCamera.transform.position;
    float currentDragDistance = Vector3.Dot(currentCameraToObject, targetCamera.transform.forward);
    
    // Calculate target position with the offset maintained
    Vector3 currentMouseWorldPos = ray.GetPoint(currentDragDistance);
    Vector3 targetPosition = currentMouseWorldPos + dragOffset;
    
    // If currently clamped, project movement along the surface
    if (isClamped) {
        Vector3 mouseDelta = currentMouseWorldPos - lastMouseWorldPos;
        DragAlongSurface(mouseDelta);
    }
    else {
        // Apply smoothing when far from camera to reduce jitter
        float distanceFromCamera = currentCameraToObject.magnitude;
        float smoothingFactor = Mathf.Clamp01(distanceFromCamera / 50f);
        targetPosition = Vector3.Lerp(targetPosition, transform.position, smoothingFactor * 0.3f);
        
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
        
        // Try to clamp to surface
        TryToClampToWormBody();
    }
    
    lastMouseWorldPos = currentMouseWorldPos;
    
    // Rotate to falseCreatureBody (after positioning)
    RotateTowardWormBody();
}

private void DragAlongSurface(Vector3 mouseDelta) {
    if (falseWormBody == null || endPoint == null) {
        isClamped = false;
        return;
    }
    
    Collider wormCollider = falseWormBody.GetComponent<Collider>();
    if (wormCollider == null) {
        isClamped = false;
        return;
    }
    
    // Get current position on surface
    Vector3 currentClosest = wormCollider.ClosestPoint(endPoint.position);
    Vector3 surfaceNormal = (endPoint.position - currentClosest).normalized;
    
    // Project mouse delta onto the tangent plane (perpendicular to surface normal)
    Vector3 tangentDelta = mouseDelta - Vector3.Dot(mouseDelta, surfaceNormal) * surfaceNormal;
    
    // Move the part by this tangent delta
    Vector3 newPosition = transform.position + tangentDelta;
    
    // Now snap back to the surface
    Vector3 offset = endPoint.position - transform.position;
    Vector3 newEndPoint = newPosition + offset;
    Vector3 newClosest = wormCollider.ClosestPoint(newEndPoint);
    
    transform.position = newClosest - offset;
    
    // Check if we should unclamp (dragged too far from surface)
    float clampDistance = GameParameters.distanceToClampPart;
    float distanceToSurface = Vector3.Distance(endPoint.position, wormCollider.ClosestPoint(endPoint.position));
    
    // More lenient unclamp threshold
    if (distanceToSurface > clampDistance * 3f) {
        isClamped = false;
    }
}

private void RotateTowardWormBody() {
    Debug.Log("RotateTowardWormBody called");
    
    if (falseWormBody == null || endPoint == null) {
        Debug.Log("falseWormBody or endPoint is null");
        return;
    }
    
    Collider wormCollider = falseWormBody.GetComponent<Collider>();
    if (wormCollider == null) {
        Debug.Log("wormCollider is null");
        return;
    }
    
    // Get closest point on surface
    Vector3 closestPoint = wormCollider.ClosestPoint(endPoint.position);
    
    // Cast from endpoint toward closest point
    Vector3 toClosest = (closestPoint - endPoint.position);
    float distance = toClosest.magnitude;
    
    Debug.Log("Distance to closest: " + distance);
    
    if (distance < 0.001f) {
        Debug.Log("Distance too small, returning");
        return;
    }
    
    Vector3 rayDirection = toClosest / distance; // Normalize
    
    RaycastHit hit;
    Vector3 inwardDirection = Vector3.zero;
    bool hitSuccess = false;
    
    // Simple single raycast first
    if (Physics.Raycast(endPoint.position, rayDirection, out hit, distance + 1f)) {
        Debug.Log("Raycast hit: " + hit.collider.name + " normal: " + hit.normal);
        
        if (hit.collider == wormCollider) {
            inwardDirection = -hit.normal;
            hitSuccess = true;
            
            Debug.DrawRay(hit.point, hit.normal * 1f, Color.blue);
            Debug.DrawRay(hit.point, inwardDirection * 1f, Color.red);
        }
    } else {
        Debug.Log("Raycast missed");
    }
    
    if (!hitSuccess) {
        Debug.Log("Using fallback");
        Vector3 surfaceNormal = (endPoint.position - closestPoint).normalized;
        if (surfaceNormal.sqrMagnitude < 0.001f) {
            surfaceNormal = (endPoint.position - falseWormBody.transform.position).normalized;
        }
        inwardDirection = -surfaceNormal;
        
        Debug.DrawRay(endPoint.position, inwardDirection * 1f, Color.yellow);
    }
    
    if (inwardDirection == Vector3.zero) {
        Debug.Log("inwardDirection is zero, returning");
        return;
    }
    
    // Current direction from part center to endpoint
    Vector3 centerToEndpointLocal = (endPoint.position - transform.position).normalized;
    Debug.DrawRay(transform.position, centerToEndpointLocal * 1f, Color.green);
    
    Debug.Log("Applying rotation - inward: " + inwardDirection + " center to endpoint: " + centerToEndpointLocal);
    
    // Calculate rotation
    Quaternion alignmentRotation = Quaternion.FromToRotation(centerToEndpointLocal, inwardDirection);
    Quaternion targetRotation = alignmentRotation * transform.rotation;
    
    // Smooth it
    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.3f);
    
    Debug.Log("Final rotation: " + transform.rotation.eulerAngles);
}

private void TryToClampToWormBody() {
    if (falseWormBody == null || endPoint == null) return;

    Collider wormCollider = falseWormBody.GetComponent<Collider>();
    if (wormCollider == null) return;

    float clampDistance = GameParameters.distanceToClampPart;

    // Use ClosestPoint - more reliable than raycast
    Vector3 closestPoint = wormCollider.ClosestPoint(endPoint.position);
    float distanceToSurface = Vector3.Distance(endPoint.position, closestPoint);

    // If within clamp distance, snap to surface
    if (distanceToSurface <= clampDistance) {
        Vector3 offset = endPoint.position - transform.position;
        transform.position = closestPoint - offset;
        isClamped = true;
    }
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