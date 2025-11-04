using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreatureBuilder
{
    [Serializable]
    public class PartPair
    {
        public string partName;          // The name of the part (from InventoryItem.partName)
        public GameObject model3DPrefab; // The corresponding 3D model prefab
    }

    public class CreatureBuilder : MonoBehaviour
    {
        [SerializeField] private List<PartPair> partPairs = new List<PartPair>();
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float spawnDistance = 5f;

        private Dictionary<string, GameObject> _prefabMapping = new Dictionary<string, GameObject>();

        private void Awake()
        {
            InitializePrefabMapping();

            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        private void InitializePrefabMapping()
        {
            _prefabMapping.Clear();
            foreach (var pair in partPairs)
            {
                if (!string.IsNullOrEmpty(pair.partName) && pair.model3DPrefab != null)
                    _prefabMapping[pair.partName] = pair.model3DPrefab;
            }
        }

        public void SwitchTo3DPart(string partName)
        {
            if (string.IsNullOrEmpty(partName))
            {
                Debug.LogWarning("Part name is null or empty");
                return;
            }

            if (_prefabMapping.TryGetValue(partName, out GameObject prefab3D))
            {
                Vector3 spawnPosition = CalculateWorldSpawnPosition();
                SpawnPartInWorld(prefab3D, spawnPosition);
            }
            else
            {
                Debug.LogWarning($"No 3D prefab mapping found for part name: {partName}");
            }
        }

        private Vector3 CalculateWorldSpawnPosition()
        {
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
            return ray.GetPoint(spawnDistance);
        }

        private void SpawnPartInWorld(GameObject prefab, Vector3 position)
        {
            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            instance.name = prefab.name;
        }
    }
}
