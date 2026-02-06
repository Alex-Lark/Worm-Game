using UnityEngine;

namespace CreatureBuilder
{
    [CreateAssetMenu(fileName = "CreaturePartData", menuName = "Creature Builder/Part Data")]
    public class CreaturePartData : ScriptableObject
    {
        public GameObject prefab;
        //public string partName;
        //public Sprite icon;
    }
}