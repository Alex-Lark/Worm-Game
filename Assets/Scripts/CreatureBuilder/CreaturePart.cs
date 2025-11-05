using System.Collections.Generic;
using UnityEngine;

namespace CreatureBuilder
{
    public class CreaturePart : MonoBehaviour
    {
        [SerializeField] private float dragDistance = 5f;

        public GameObject prefab;
        public Camera targetCamera;
        public RectTransform creatureBuilderWindow;
    
        private CreatureBuilder _creatureBuilder;
        private List<GameObject> outlineObjects = new List<GameObject>();
        private Color outlineColor = Color.cyan;
        private float outlineWidth = 0.03f;
    
        private bool isSelected;
        private bool isDragging;
    
        void Start()
        {
            _creatureBuilder = GameObject.Find("CreatureBuilder").GetComponent<CreatureBuilder>();
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
            HighlightPart();
        }
    
        public void StopDragging()
        {
            isSelected = false;
            isDragging = false;
            RemoveHighlight();
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
