using UnityEngine;

namespace CreatureParts
{
    public class WormHead : CreaturePart
    {
        public GameObject visualHead;
        public GameObject wormVisualHeadWithMaterial;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        public new void SetMaterial(Material material)
        {
            Debug.Log("setMaterial called on wormHead");
            wormVisualHeadWithMaterial.GetComponent<MeshRenderer>().material = material;
        }
        
    }
}
