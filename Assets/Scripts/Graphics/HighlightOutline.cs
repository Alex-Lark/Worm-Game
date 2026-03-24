using System.Collections.Generic;
using System.Linq;
using CreatureParts;
using UnityEngine;

namespace CreatureBuilder
{
    public class HighlightOutline : MonoBehaviour
    {
        #region Public Methods
        
        public void HighlightPart(Color? color = null)
        {
            Color outlineColor = color ?? GameParameters.PartDraggingOutlineColor;
    
            foreach (Renderer partRenderer in GetComponentsInChildren<Renderer>())
            {
                if (!partRenderer.enabled) continue;

                var materials = partRenderer.sharedMaterials.ToList();
    
                Material outlineMat = new Material(Shader.Find("Custom/Outline"));
                outlineMat.SetColor("_OutlineColor", outlineColor);
                outlineMat.SetFloat("_OutlineWidth", GameParameters.PartDraggingOutlineWidth);
    
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
