using System.Collections.Generic;
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
                if (!partRenderer.TryGetComponent<MeshFilter>(out MeshFilter meshFilter)) continue;
                
                GameObject outlineObj = new GameObject(partRenderer.name + "_Outline");
                outlineObj.transform.SetParent(partRenderer.transform);
                outlineObj.transform.localPosition = Vector3.zero;
                outlineObj.transform.localRotation = Quaternion.identity;
                outlineObj.transform.localScale = Vector3.one;
            
                MeshFilter outlineMeshFilter = outlineObj.AddComponent<MeshFilter>();
                MeshRenderer outlineMeshRenderer = outlineObj.AddComponent<MeshRenderer>();
            
                outlineMeshFilter.mesh = CreateInvertedMesh(meshFilter.mesh, GameParameters.PartDraggingOutlineWidth);
                
                Material outlineMat = new Material(Shader.Find("Unlit/Color")) { color = GameParameters.PartDraggingOutlineColor };
                outlineMeshRenderer.material = outlineMat;
                outlineMeshRenderer.sortingOrder = -1;
            
                outlineObjects.Add(outlineObj);
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
        
        #endregion
        
    }
}
