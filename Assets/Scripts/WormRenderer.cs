using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WormRenderer : MonoBehaviour
{
    [Header("Worm Visual Settings")]
    public Material wormMaterial;
    public int tubeResolution = 8; // Number of vertices around the circumference
    public AnimationCurve radiusCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.8f); // Head to tail radius variation
    
    private LineRenderer lineRenderer;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh wormMesh;
    
    private Player player; // Reference to your Player script
    
    void Start()
    {
        player = GetComponent<Player>();
        SetupWormRenderer();
    }
    
    void SetupWormRenderer()
    {
        // Setup LineRenderer (for debugging/simple visualization)
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = wormMaterial;
        lineRenderer.startWidth = GameParameters.SegmentMaxPartDistance * 2f;
        lineRenderer.endWidth = GameParameters.SegmentMaxPartDistance * 1.6f; // Slightly tapered
        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = false; // We'll use the mesh instead
        
        // Setup Mesh components for the tube
        if (meshFilter == null)
        {
            GameObject meshObj = new GameObject("WormMesh");
            meshObj.transform.SetParent(transform);
            
            meshFilter = meshObj.AddComponent<MeshFilter>();
            meshRenderer = meshObj.AddComponent<MeshRenderer>();
            meshRenderer.material = wormMaterial;
            
            wormMesh = new Mesh();
            wormMesh.name = "WormTube";
            meshFilter.mesh = wormMesh;
        }
    }
    
    void Update()
    {
        UpdateWormVisual();
    }
    
    void UpdateWormVisual()
    {
        // Generate tube mesh based on worm segments
        GenerateTubeMesh();
    }
    
    void GenerateTubeMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        
        // Get all worm positions (head + body segments)
        List<Vector3> wormPositions = new List<Vector3>();
        List<Vector3> wormDirections = new List<Vector3>();
        
        wormPositions.Add(player.wormHead.position);
        
        for (int i = 0; i < player.wormParts.Count; i++)
        {
            wormPositions.Add(player.wormParts[i].position);
        }
        
        // Calculate directions for each segment
        for (int i = 0; i < wormPositions.Count; i++)
        {
            Vector3 direction;
            if (i == 0)
            {
                // Head direction
                direction = (wormPositions[1] - wormPositions[0]).normalized;
            }
            else if (i == wormPositions.Count - 1)
            {
                // Tail direction
                direction = (wormPositions[i] - wormPositions[i - 1]).normalized;
            }
            else
            {
                // Middle segments - average of adjacent segments
                Vector3 dirToPrev = (wormPositions[i] - wormPositions[i - 1]).normalized;
                Vector3 dirToNext = (wormPositions[i + 1] - wormPositions[i]).normalized;
                direction = (dirToPrev + dirToNext).normalized;
            }
            
            wormDirections.Add(direction);
        }
        
        // Generate tube geometry
        for (int i = 0; i < wormPositions.Count; i++)
        {
            Vector3 center = wormPositions[i];
            Vector3 forward = wormDirections[i];
            
            // Calculate radius for this segment
            float t = (float)i / (wormPositions.Count - 1);
            float radius = GameParameters.SegmentMaxPartDistance * radiusCurve.Evaluate(t);
            
            // Create perpendicular vectors
            Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, forward).normalized;
            
            // Generate vertices around the circumference
            int baseIndex = vertices.Count;
            for (int j = 0; j < tubeResolution; j++)
            {
                float angle = (float)j / tubeResolution * 2f * Mathf.PI;
                Vector3 offset = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;
                
                vertices.Add(center + offset);
                uvs.Add(new Vector2((float)j / tubeResolution, t));
            }
            
            // Generate triangles (connect this ring to the previous ring)
            if (i > 0)
            {
                int prevBaseIndex = baseIndex - tubeResolution;
                
                for (int j = 0; j < tubeResolution; j++)
                {
                    int next = (j + 1) % tubeResolution;
                    
                    // Create two triangles for each quad
                    // Triangle 1
                    triangles.Add(prevBaseIndex + j);
                    triangles.Add(baseIndex + j);
                    triangles.Add(prevBaseIndex + next);
                    
                    // Triangle 2
                    triangles.Add(prevBaseIndex + next);
                    triangles.Add(baseIndex + j);
                    triangles.Add(baseIndex + next);
                }
            }
        }
        
        // Update mesh
        wormMesh.Clear();
        wormMesh.vertices = vertices.ToArray();
        wormMesh.uv = uvs.ToArray();
        wormMesh.triangles = triangles.ToArray();
        wormMesh.RecalculateNormals();
        wormMesh.RecalculateBounds();
    }
    
    // Alternative: Simple LineRenderer approach (less flexible but easier)
    void UpdateLineRenderer()
    {
        List<Vector3> positions = new List<Vector3>();
        positions.Add(player.wormHead.position);
        
        for (int i = 0; i < player.wormParts.Count; i++)
        {
            positions.Add(player.wormParts[i].position);
        }
        
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }
    
    // Toggle between mesh and line renderer for testing
    public void ToggleRenderMode()
    {
        meshRenderer.enabled = !meshRenderer.enabled;
        lineRenderer.enabled = !lineRenderer.enabled;
        
        if (lineRenderer.enabled)
        {
            UpdateLineRenderer();
        }
    }
}