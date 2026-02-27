using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(LineRenderer))]
    public class WormRenderer : MonoBehaviour
    {
        /* THIS IS AFFECTS NO GAMEPLAY MECHANICS, ONLY VISUAL APPEARANCE OF WORM. TO BE REFACTORED AT A LATER DATE. */
        
        [Header("Worm Visual Settings")]
        public Material wormMaterial;
        public int tubeResolution = 8; // Number of vertices around the circumference
        public int smoothingSubdivisions = 3; // Extra points between segments (increased default)
        public int capSubdivisions = 4; // Rings for rounded ends
        public AnimationCurve radiusCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.8f);
    
        [Header("Smoothing Settings")]
        [Range(0f, 1f)]
        public float smoothingStrength = 0.5f; // How much to smooth the curve
        public bool useAdaptiveSubdivision = true; // Add more points where needed
        [Range(0.01f, 0.5f)]
        public float minSegmentDistance = 0.05f; // Minimum distance to subdivide

        private LineRenderer lineRenderer;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh wormMesh;

        private global::Player.Player player;

        void Start()
        {
            player = GetComponent<global::Player.Player>();
            SetupWormRenderer();
    
            // Force an initial update after a frame to ensure positions are set
            StartCoroutine(DelayedInitialUpdate());
        }

        public void Restart()
        {
            Start();
        }
    
        IEnumerator DelayedInitialUpdate()
        {
            yield return new WaitForEndOfFrame();
            UpdateWormVisual();
        }

        void SetupWormRenderer()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.material = wormMaterial;
            lineRenderer.startWidth = GameParameters.WormBodyWidth;
            lineRenderer.endWidth = GameParameters.WormBodyWidth;
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
            originalPositions.AddRange(player.wormBodySegments.ConvertAll(p => p.position));

            // Generate smooth curve with adaptive subdivision
            List<Vector3> wormPositions = GenerateSmoothCurve(originalPositions);

            // Directions with better smoothing
            List<Vector3> wormDirections = CalculateSmoothedDirections(wormPositions);

            // --- Head cap ---
            int prevRingStart = GenerateHeadCap(wormPositions, wormDirections, vertices, uvs, triangles);

            // --- Tube body ---
            GenerateTubeBody(wormPositions, wormDirections, vertices, uvs, triangles);

            // --- Tail cap ---
            GenerateTailCap(wormPositions, wormDirections, vertices, uvs, triangles);
        
            // --- Update mesh ---
            wormMesh.Clear();
            wormMesh.vertices = vertices.ToArray();
            wormMesh.uv = uvs.ToArray();
            wormMesh.triangles = triangles.ToArray();
            wormMesh.RecalculateNormals();
            wormMesh.RecalculateBounds();
        }

        List<Vector3> GenerateSmoothCurve(List<Vector3> originalPositions)
        {
            List<Vector3> wormPositions = new List<Vector3>();
        
            for (int i = 0; i < originalPositions.Count - 1; i++)
            {
                Vector3 p0 = (i == 0) ? originalPositions[0] : originalPositions[i - 1];
                Vector3 p1 = originalPositions[i];
                Vector3 p2 = originalPositions[i + 1];
                Vector3 p3 = (i == originalPositions.Count - 2) ? originalPositions[originalPositions.Count - 1] : originalPositions[i + 2];

                float segmentDistance = Vector3.Distance(p1, p2);
            
                // Adaptive subdivision: use more subdivisions for longer segments
                int pointsPerSegment = smoothingSubdivisions + 1;
                if (useAdaptiveSubdivision)
                {
                    pointsPerSegment = Mathf.Max(1, Mathf.CeilToInt(segmentDistance / minSegmentDistance));
                }

                for (int sub = 0; sub < pointsPerSegment; sub++)
                {
                    float t = (float)sub / pointsPerSegment;
                    Vector3 splinePoint = CatmullRomSpline(p0, p1, p2, p3, t);
                
                    // Apply smoothing to prevent sharp transitions
                    if (smoothingStrength > 0 && i > 0 && sub > 0)
                    {
                        Vector3 linearPoint = Vector3.Lerp(p1, p2, t);
                        splinePoint = Vector3.Lerp(linearPoint, splinePoint, smoothingStrength);
                    }
                
                    wormPositions.Add(splinePoint);
                }
            }
            wormPositions.Add(originalPositions[originalPositions.Count - 1]);

            return wormPositions;
        }

        List<Vector3> CalculateSmoothedDirections(List<Vector3> positions)
        {
            List<Vector3> directions = new List<Vector3>();
        
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 direction;
            
                if (i == 0)
                {
                    direction = (positions[1] - positions[0]).normalized;
                }
                else if (i == positions.Count - 1)
                {
                    direction = (positions[i] - positions[i - 1]).normalized;
                }
                else
                {
                    // Average forward and backward directions for smoother transitions
                    Vector3 backward = (positions[i] - positions[i - 1]).normalized;
                    Vector3 forward = (positions[i + 1] - positions[i]).normalized;
                
                    // Weight directions by distance for better smoothing
                    float backDist = Vector3.Distance(positions[i], positions[i - 1]);
                    float forwardDist = Vector3.Distance(positions[i + 1], positions[i]);
                    float totalDist = backDist + forwardDist;
                
                    if (totalDist > 0.001f)
                    {
                        direction = (backward * backDist + forward * forwardDist) / totalDist;
                    }
                    else
                    {
                        direction = (backward + forward) * 0.5f;
                    }
                
                    direction.Normalize();
                }

                directions.Add(direction);
            }

            return directions;
        }

        int GenerateHeadCap(List<Vector3> positions, List<Vector3> directions, 
            List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
        {
            Vector3 headCenter = positions[0];
            Vector3 headDir = directions[0];
            Vector3 right = GetStableRight(headDir);
            Vector3 up = Vector3.Cross(right, headDir).normalized;

            int prevRingStart = 0;

            for (int i = 0; i <= capSubdivisions; i++)
            {
                float t = (float)i / capSubdivisions;
                float radius = GameParameters.WormBodyWidth * radiusCurve.Evaluate(0) * Mathf.Sin(t * Mathf.PI * 0.5f);
                Vector3 ringOffset = headDir * (-Mathf.Cos(t * Mathf.PI * 0.5f) * radiusCurve.Evaluate(0) * GameParameters.WormBodyWidth);

                int ringStart = vertices.Count;
                for (int j = 0; j < tubeResolution; j++)
                {
                    float angle = (float)j / tubeResolution * 2f * Mathf.PI;
                    Vector3 offset = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;
                    vertices.Add(headCenter + ringOffset + offset);
                    uvs.Add(new Vector2((float)j / tubeResolution, t * 0.1f));
                }

                if (i > 0)
                {
                    AddRingTriangles(prevRingStart, ringStart, tubeResolution, triangles, false);
                }
                prevRingStart = ringStart;
            }

            return prevRingStart;
        }

        void GenerateTubeBody(List<Vector3> positions, List<Vector3> directions,
            List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
        {
            Vector3 prevRight = GetStableRight(directions[0]);

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 center = positions[i];
                Vector3 forward = directions[i];
                float t = (float)i / (positions.Count - 1);
                float radius = GameParameters.WormBodyWidth * radiusCurve.Evaluate(t);

                // Use rotation minimizing frames for stable tube rotation
                Vector3 right = GetStableRight(forward, prevRight);
                Vector3 up = Vector3.Cross(right, forward).normalized;
                prevRight = right;

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
                    AddRingTriangles(prev, baseIndex, tubeResolution, triangles, false);
                }
            }
        }

        void GenerateTailCap(List<Vector3> positions, List<Vector3> directions,
            List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
        {
            int lastIndex = positions.Count - 1;
            Vector3 tailCenter = positions[lastIndex];
            Vector3 tailDir = directions[lastIndex];
            Vector3 right = GetStableRight(tailDir);
            Vector3 up = Vector3.Cross(right, tailDir).normalized;

            int prevRingStart = vertices.Count - tubeResolution;

            for (int i = capSubdivisions - 1; i >= 0; i--)
            {
                float t = (float)i / capSubdivisions;
                float radius = GameParameters.WormBodyWidth * radiusCurve.Evaluate(1.0f) * Mathf.Sin(t * Mathf.PI * 0.5f);
                Vector3 ringOffset = tailDir * (Mathf.Cos(t * Mathf.PI * 0.5f) * radiusCurve.Evaluate(1.0f) * GameParameters.WormBodyWidth);

                int ringStart = vertices.Count;
                for (int j = 0; j < tubeResolution; j++)
                {
                    float angle = (float)j / tubeResolution * 2f * Mathf.PI;
                    Vector3 offset = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;
                    vertices.Add(tailCenter + ringOffset + offset);
                    uvs.Add(new Vector2((float)j / tubeResolution, 0.9f + t * 0.1f));
                }

                AddRingTriangles(prevRingStart, ringStart, tubeResolution, triangles, false);
                prevRingStart = ringStart;
            }
        }

        void AddRingTriangles(int prevRing, int currentRing, int resolution, List<int> triangles, bool reversed)
        {
            for (int j = 0; j < resolution; j++)
            {
                int next = (j + 1) % resolution;
            
                if (!reversed)
                {
                    triangles.Add(prevRing + j);
                    triangles.Add(currentRing + j);
                    triangles.Add(prevRing + next);

                    triangles.Add(prevRing + next);
                    triangles.Add(currentRing + j);
                    triangles.Add(currentRing + next);
                }
                else
                {
                    triangles.Add(prevRing + j);
                    triangles.Add(prevRing + next);
                    triangles.Add(currentRing + j);

                    triangles.Add(prevRing + next);
                    triangles.Add(currentRing + next);
                    triangles.Add(currentRing + j);
                }
            }
        }

        Vector3 GetStableRight(Vector3 forward, Vector3 previousRight = default)
        {
            // Rotation minimizing frame for stable tube orientation
            if (previousRight == default || Vector3.Dot(forward, previousRight) > 0.99f)
            {
                // Initial case or nearly parallel
                Vector3 upGuide = Mathf.Abs(forward.y) > 0.99f ? Vector3.forward : Vector3.up;
                return Vector3.Cross(forward, upGuide).normalized;
            }
            else
            {
                // Project previous right onto plane perpendicular to forward
                Vector3 right = previousRight - forward * Vector3.Dot(forward, previousRight);
                return right.normalized;
            }
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
            positions.AddRange(player.wormBodySegments.ConvertAll(p => p.position));

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
}