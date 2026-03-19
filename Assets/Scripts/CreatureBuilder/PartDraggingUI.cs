using System.Collections.Generic;
using CreatureParts;
using UnityEngine;

namespace CreatureBuilder
{
    public class PartDraggingUI : MonoBehaviour
    {
        private List<GameObject> outlineObjects = new List<GameObject>();
        
        #region Public Methods
        
        public void HighlightPart()
        {
            foreach (Renderer partRenderer in GetComponentsInChildren<Renderer>())
            {
                if (!partRenderer.enabled) continue;
                
                if (partRenderer is SkinnedMeshRenderer skinnedRenderer)
                {
                    
                    SkinnedMeshBaker baker = partRenderer.GetComponentInParent<SkinnedMeshBaker>();
                    MeshCollider col = baker != null ? baker.GetComponent<MeshCollider>() : null;
                    Mesh meshToUse = (col != null && col.sharedMesh != null) ? col.sharedMesh : null;

                    if (meshToUse == null)
                    {
                        Mesh bakedMesh = new Mesh();
                        skinnedRenderer.BakeMesh(bakedMesh);
                        meshToUse = bakedMesh;
                    }

                    GameObject outlineObj = new GameObject(partRenderer.name + "_Outline");
                    outlineObj.transform.SetParent(baker != null ? baker.transform : transform);
                    outlineObj.transform.localPosition = Vector3.zero;
                    outlineObj.transform.localRotation = Quaternion.identity;
                    outlineObj.transform.localScale = Vector3.one;

                    MeshFilter outlineMeshFilter = outlineObj.AddComponent<MeshFilter>();
                    MeshRenderer outlineMeshRenderer = outlineObj.AddComponent<MeshRenderer>();

                    // Pass directly - inverted triangles + inward normals = expands outward
                    outlineMeshFilter.mesh = CreateInvertedMesh(meshToUse, GameParameters.PartDraggingOutlineWidth);

                    Material outlineMat = new Material(Shader.Find("Unlit/Color")) { color = GameParameters.PartDraggingOutlineColor };
                    outlineMeshRenderer.material = outlineMat;

                    outlineObjects.Add(outlineObj);
                }
                
                else if (partRenderer.TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
                {
                    if (!meshFilter.mesh.isReadable)
                    {
                        Debug.LogWarning($"[PartDraggingUI] Mesh '{meshFilter.mesh.name}' is not readable.");
                        continue;
                    }

                    GameObject outlineObj = new GameObject(partRenderer.name + "_Outline");
                    // Parent to the renderer's transform like before
                    outlineObj.transform.SetParent(partRenderer.transform);
                    outlineObj.transform.localPosition = Vector3.zero;
                    outlineObj.transform.localRotation = Quaternion.identity;
                    // Match the local scale exactly so the outline sits in the same space
                    outlineObj.transform.localScale = Vector3.one;

                    MeshFilter outlineMeshFilter = outlineObj.AddComponent<MeshFilter>();
                    MeshRenderer outlineMeshRenderer = outlineObj.AddComponent<MeshRenderer>();

                    // Scale thickness by the inverse of the object's lossy scale
                    // so outline width appears consistent regardless of object scale
                    float scaledThickness = GameParameters.PartDraggingOutlineWidth / partRenderer.transform.lossyScale.x;
    
                    outlineMeshFilter.mesh = CreateInvertedMesh(meshFilter.mesh, scaledThickness);

                    Material outlineMat = new Material(Shader.Find("Unlit/Color")) { color = GameParameters.PartDraggingOutlineColor };
                    outlineMeshRenderer.material = outlineMat;

                    outlineObjects.Add(outlineObj);
                }
            }
        }
        
        public void RemoveHighlight()
        {
            foreach (GameObject obj in outlineObjects)
                if (obj != null) Destroy(obj);
            outlineObjects.Clear();
        }
        
        #endregion
        
        #region Private Methods
    
        private Mesh CreateInvertedMesh(Mesh originalMesh, float thickness)
        {
            Mesh mesh = new Mesh
            {
                vertices = originalMesh.vertices,
                normals = originalMesh.normals,
                uv = originalMesh.uv
            };
        
            int[] triangles = originalMesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
                (triangles[i], triangles[i + 2]) = (triangles[i + 2], triangles[i]);
            mesh.triangles = triangles;
        
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] += normals[i] * thickness;
            mesh.vertices = vertices;
        
            mesh.RecalculateBounds();
            return mesh;
        }
        
        private Mesh FlipNormals(Mesh original)
        {
            Mesh mesh = new Mesh
            {
                vertices = original.vertices,
                triangles = original.triangles,
                uv = original.uv
            };
    
            Vector3[] normals = original.normals;
            for (int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;
    
            return mesh;
        }
        
        #endregion
        
    }
}
