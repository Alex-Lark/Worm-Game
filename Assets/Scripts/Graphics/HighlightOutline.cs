using System.Linq;
using UnityEngine;

namespace Graphics
{
    public class HighlightOutline : MonoBehaviour
    {
        #region Public Methods
        
        public void HighlightPart(Color? color = null, float? width = null)
        {
            Color outlineColor = color ?? GameParameters.PartDraggingOutlineColor;
            float outlineWidth = width ?? GameParameters.PartDraggingOutlineWidth;

            foreach (Renderer partRenderer in GetComponentsInChildren<Renderer>())
            {
                if (!partRenderer.enabled) continue;

                var materials = partRenderer.sharedMaterials.ToList();
                
                if (materials.Any(m => m != null && m.shader.name == "Custom/Outline"))
                    continue;

                Material outlineMat = new Material(Shader.Find("Custom/Outline"));
                outlineMat.SetColor("_OutlineColor", outlineColor);
                outlineMat.SetFloat("_OutlineWidth", outlineWidth);

                materials.Add(outlineMat);
                partRenderer.materials = materials.ToArray();
            }
        }
        
        public void RemoveHighlight()
        {
            foreach (Renderer partRenderer in GetComponentsInChildren<Renderer>())
            {
                var materials = partRenderer.sharedMaterials.ToList();
                materials.RemoveAll(m => m != null && m.shader.name == "Custom/Outline");
                partRenderer.materials = materials.ToArray();
            }
        }
        
        #endregion
        
    }
}
