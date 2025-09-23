using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WormRenderer : MonoBehaviour
{
    [Header("Worm Visual Settings")]
    public Material wormMaterial;
    public int tubeResolution = 8; // Number of vertices around the circumference
    public int smoothingSubdivisions = 2; // Extra points between segments
    public int capSubdivisions = 4; // Rings for rounded ends
    public AnimationCurve radiusCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.8f);

    private LineRenderer lineRenderer;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh wormMesh;

    private Player player;

    void Start()
    {
        player = GetComponent<Player>();
        SetupWormRenderer();
    }

    void SetupWormRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = wormMaterial;
        lineRenderer.startWidth = GameParameters.SegmentMaxPartDistance * 2f;
        lineRenderer.endWidth = GameParameters.SegmentMaxPartDistance * 1.6f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = false;

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
        GenerateTubeMesh();
    }

    void GenerateTubeMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        // Original positions (head + segments)
        List<Vector3> originalPositions = new List<Vector3> { player.wormHead.position };
        originalPositions.AddRange(player.wormParts.ConvertAll(p => p.position));

        // Generate smooth curve using Catmull-Rom
        List<Vector3> wormPositions = new List<Vector3>();
        int pointsPerSegment = smoothingSubdivisions + 1;
        for (int i = 0; i < originalPositions.Count - 1; i++)
        {
            Vector3 p0 = (i == 0) ? originalPositions[0] : originalPositions[i - 1];
            Vector3 p1 = originalPositions[i];
            Vector3 p2 = originalPositions[i + 1];
            Vector3 p3 = (i == originalPositions.Count - 2) ? originalPositions[originalPositions.Count - 1] : originalPositions[i + 2];

            for (int sub = 0; sub < pointsPerSegment; sub++)
            {
                float t = (float)sub / pointsPerSegment;
                wormPositions.Add(CatmullRomSpline(p0, p1, p2, p3, t));
            }
        }
        wormPositions.Add(originalPositions[originalPositions.Count - 1]);

        // Directions
        List<Vector3> wormDirections = new List<Vector3>();
        for (int i = 0; i < wormPositions.Count; i++)
        {
            Vector3 direction;
            if (i == 0) direction = (wormPositions[1] - wormPositions[0]).normalized;
            else if (i == wormPositions.Count - 1) direction = (wormPositions[i] - wormPositions[i - 1]).normalized;
            else direction = ((wormPositions[i] - wormPositions[i - 1]).normalized + (wormPositions[i + 1] - wormPositions[i]).normalized).normalized;

            wormDirections.Add(direction);
        }

        // --- Head cap first ---
        int prevRingStart = 0;
        {
            Vector3 headCenter = wormPositions[0];
            Vector3 headDir = wormDirections[0];
            Vector3 right = Vector3.Cross(headDir, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, headDir).normalized;

            for (int i = 0; i <= capSubdivisions; i++)
            {
                float t = (float)i / capSubdivisions;
                float radius = GameParameters.SegmentMaxPartDistance * radiusCurve.Evaluate(0) * Mathf.Sin(t * Mathf.PI * 0.5f);
                Vector3 ringOffset = headDir * (-Mathf.Cos(t * Mathf.PI * 0.5f) * radiusCurve.Evaluate(0) * GameParameters.SegmentMaxPartDistance);

                int ringStart = vertices.Count;
                for (int j = 0; j < tubeResolution; j++)
                {
                    float angle = (float)j / tubeResolution * 2f * Mathf.PI;
                    Vector3 offset = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;
                    vertices.Add(headCenter + ringOffset + offset);
                    uvs.Add(new Vector2((float)j / tubeResolution, t));
                }

                if (i > 0)
                {
                    for (int j = 0; j < tubeResolution; j++)
                    {
                        int next = (j + 1) % tubeResolution;
                        triangles.Add(prevRingStart + j);
                        triangles.Add(ringStart + j);
                        triangles.Add(prevRingStart + next);

                        triangles.Add(prevRingStart + next);
                        triangles.Add(ringStart + j);
                        triangles.Add(ringStart + next);
                    }
                }
                prevRingStart = ringStart;
            }
        }

        // --- Tube ---
        for (int i = 0; i < wormPositions.Count; i++)
        {
            Vector3 center = wormPositions[i];
            Vector3 forward = wormDirections[i];
            float t = (float)i / (wormPositions.Count - 1);
            float radius = GameParameters.SegmentMaxPartDistance * radiusCurve.Evaluate(t);

            Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, forward).normalized;

            int baseIndex = vertices.Count;
            for (int j = 0; j < tubeResolution; j++)
            {
                float angle = (float)j / tubeResolution * 2f * Mathf.PI;
                Vector3 offset = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;
                vertices.Add(center + offset);
                uvs.Add(new Vector2((float)j / tubeResolution, t));
            }

            if (i > 0)
            {
                int prev = baseIndex - tubeResolution;
                for (int j = 0; j < tubeResolution; j++)
                {
                    int next = (j + 1) % tubeResolution;
                    triangles.Add(prev + j);
                    triangles.Add(baseIndex + j);
                    triangles.Add(prev + next);

                    triangles.Add(prev + next);
                    triangles.Add(baseIndex + j);
                    triangles.Add(baseIndex + next);
                }
            }
        }

        // --- Tail cap ---
        int prevRingStartTail = 0;
        {
            int lastIndex = wormPositions.Count - 1;
            Vector3 tailCenter = wormPositions[lastIndex];
            Vector3 tailDir = wormDirections[lastIndex]; // keep original direction, not negated
            Vector3 right = Vector3.Cross(tailDir, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, tailDir).normalized;

            // Loop backwards to create the cap from base to tip
            for (int i = capSubdivisions; i >= 0; i--)
            {
                float t = (float)i / capSubdivisions;
                float radius = GameParameters.SegmentMaxPartDistance * radiusCurve.Evaluate(0) * Mathf.Sin(t * Mathf.PI * 0.5f);
                Vector3 ringOffset = tailDir * (Mathf.Cos(t * Mathf.PI * 0.5f) * radiusCurve.Evaluate(0) * GameParameters.SegmentMaxPartDistance);

                int ringStart = vertices.Count;
                for (int j = 0; j < tubeResolution; j++)
                {
                    float angle = (float)j / tubeResolution * 2f * Mathf.PI;
                    Vector3 offset = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;
                    vertices.Add(tailCenter + ringOffset + offset);
                    uvs.Add(new Vector2((float)j / tubeResolution, t));
                }

                if (i < capSubdivisions) // connect with previous ring
                {
                    for (int j = 0; j < tubeResolution; j++)
                    {
                        int next = (j + 1) % tubeResolution;

                        // Triangle winding reversed compared to head
                        triangles.Add(prevRingStartTail + j);
                        triangles.Add(ringStart + j);
                        triangles.Add(prevRingStartTail + next);

                        triangles.Add(prevRingStartTail + next);
                        triangles.Add(ringStart + j);
                        triangles.Add(ringStart + next);
                    }
                }

                prevRingStartTail = ringStart;
            }
        }
        
        // --- Update mesh ---
        wormMesh.Clear();
        wormMesh.vertices = vertices.ToArray();
        wormMesh.uv = uvs.ToArray();
        wormMesh.triangles = triangles.ToArray();
        wormMesh.RecalculateNormals();
        wormMesh.RecalculateBounds();
    }

    Vector3 CatmullRomSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    void UpdateLineRenderer()
    {
        List<Vector3> positions = new List<Vector3> { player.wormHead.position };
        positions.AddRange(player.wormParts.ConvertAll(p => p.position));

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }

    public void ToggleRenderMode()
    {
        meshRenderer.enabled = !meshRenderer.enabled;
        lineRenderer.enabled = !lineRenderer.enabled;

        if (lineRenderer.enabled) UpdateLineRenderer();
    }
}