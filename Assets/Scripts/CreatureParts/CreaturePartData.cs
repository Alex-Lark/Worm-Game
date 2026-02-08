using UnityEngine;

namespace CreatureParts
{
    [CreateAssetMenu(fileName = "CreaturePartData", menuName = "Creature Builder/Part Data")]
    public class CreaturePartData : ScriptableObject
    {
        public GameObject prefab;
        public float mass;
        
        //public string partName;
        //public Sprite icon;
    }
}