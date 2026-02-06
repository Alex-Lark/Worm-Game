using System.Collections;
using System.Collections.Generic;
using CreatureParts;
using Unity.Cinemachine;
using UnityEngine;

namespace CreatureBuilder
{
    public class CreatureBuilder : MonoBehaviour
    {
        #region public variables
        [Header("Public Variables")]
        
        public List<GameObject> parts = new List<GameObject>();
        public List<GameObject> legs = new List<GameObject>();
        
        public Camera targetCamera;
        public CinemachineCamera cinemachineCamera;
        public RectTransform creatureBuilderWindow;
        #endregion
        
        #region private variables
        [Header("Private Variables")]
        
        private Player.Player player;
        
        [SerializeField] private float spawnDistance = 5f;
        
        [SerializeField] private List<PartPair> partPairs = new List<PartPair>();
        private readonly Dictionary<string, GameObject> prefabMapping = new Dictionary<string, GameObject>();
        #endregion

        #region MonoBehaviour Methods
        private void Awake()
        {
            InitializePrefabMapping();
            player = Player.Player.Instance;
            cinemachineCamera.Follow = player.transform;
        }

        private void Start()
        {
            StartCoroutine(AddAlreadyAttachedPartsDelayed());
        }
        #endregion

        #region public methods
        
        public void SwitchFromCardTo3DPart(GameObject cardPrefab)
        {
            string cardName = cardPrefab.name.Replace("(Clone)", "").Trim();
            
            if (prefabMapping.TryGetValue(cardName, out GameObject prefab3D))
            {
                Vector3 spawnPosition = CalculateWorldSpawnPosition();
                SpawnPartInWorld(prefab3D, spawnPosition);
            }
            else
            {
                Debug.LogWarning($"No 3D prefab mapping found for card: {cardName}");
            }
        }
        
        public void SwitchFrom3DPartToCard(GameObject partPrefab, GameObject caller)
        {
            string partName = partPrefab.name.Replace("(Clone)", "").Trim();
            
            foreach (var pair in partPairs)
            {
                if (pair.part3DPrefab != null && pair.part3DPrefab.name == partName)
                {
                    SpawnCardInInventory(pair.cardPrefab);
                    
                    parts.Remove(caller);
                    Destroy(caller);
                    return;
                }
            }
        }
        
        public void AttachCreatureParts()
        {
            foreach (GameObject part in parts)
            {
                PartDragging partDragging = part.GetComponent<PartDragging>();
        
                if (partDragging != null && partDragging.isClamped)
                {
                    Transform wormSegment = FindNearestWormSegment(part);
                    AddPartToWorm(part, wormSegment);
                }
                else
                {
                    print("part not clamped, returning to player inventory");
                    ReturnPartToPlayerInventory(part);
                }
            }
    
            parts.Clear();
            SetLegOrder();
            
            // Return all remaining cards from the creature builder inventory to player
            ReturnAllCardsToPlayerInventory();
        }
        
        #endregion
        
        #region private methods
        
        private IEnumerator AddAlreadyAttachedPartsDelayed()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.5f);

            foreach (GameObject part in Player.Player.Instance.attachedWormParts)
            {
                AddAlreadyAttachedPart(part);
            }
            Player.Player.Instance.attachedWormParts.Clear();
        }

        private void AddAlreadyAttachedPart(GameObject part)
        {
            PartDragging partDraggingComponent = part.GetComponent<PartDragging>();
            GameObject prefab = partDraggingComponent.Prefab;
        
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab reference is null for {part.name}");
                return;
            }
                
            //reduces velocity per leg since it will be increased later
            if (part.GetComponent<PartDragging>().partData.name.Equals("leg"))
            {
                Player.Player.Instance.MaxVelocity -= GameParameters.legMaxVelocityIncrease;
                //TODO: disable leg script
            }
            
            CreateDuplicatePart(part, prefab);
        }

        private void CreateDuplicatePart(GameObject part, GameObject prefab)
        {
            Vector3 worldPosition = part.transform.position;
            Quaternion worldRotation = part.transform.rotation;
            Vector3 worldScale = part.transform.lossyScale;

            GameObject newPart = Instantiate(prefab, worldPosition, worldRotation);
            newPart.name = prefab.name;
            newPart.transform.localScale = worldScale;
                
            PartDragging partDragging = newPart.GetComponent<PartDragging>();
            
            if (partDragging != null)
            {
                partDragging.enabled = true;
                ResetPartDragging(partDragging);
                partDragging.Clamp();
            }

            parts.Add(newPart);
            Destroy(part);
        }

        private void ResetPartDragging(PartDragging partDragging)
        {
            partDragging.targetCamera = targetCamera;
            partDragging.creatureBuilderWindow = creatureBuilderWindow;
            partDragging.dragDistance = spawnDistance;
        }

        private void InitializePrefabMapping()
        {
            prefabMapping.Clear();
            foreach (var pair in partPairs)
            {
                if (pair.cardPrefab != null && pair.part3DPrefab != null)
                {
                    string cardName = pair.cardPrefab.name;
                    prefabMapping[cardName] = pair.part3DPrefab;
                }
            }
        }

        private void SetLegOrder()
        {
            int numLegs = legs.Count;
            float totalTime = GameParameters.legMoveTime;

            for (int i = 0; i < legs.Count; i++)
            {
                legs[i].GetComponent<LegPart>().timeOffset = (i * (totalTime / numLegs));
            }
        }

        private void ReturnPartToPlayerInventory(GameObject part)
        {
            string partName = part.name.Replace("(Clone)", "").Trim();
            
            foreach (var pair in partPairs)
            {
                if (pair.part3DPrefab != null && pair.part3DPrefab.name == partName)
                {
                    Player.Player.Instance.wormPartsInInventory.Add(pair.cardPrefab);
                    Destroy(part);
                    return;
                }
            }
            
            Destroy(part);
        }
        
    private void ReturnAllCardsToPlayerInventory()
    {
        CreatureBuilderPartInventory inventory = FindFirstObjectByType<CreatureBuilderPartInventory>();
        
        if (inventory == null)
        {
            Debug.LogWarning("Creature builder inventory not found");
            return;
        }
        
        InventorySlot[] slots = inventory.GetComponentsInChildren<InventorySlot>();
        
        foreach (var slot in slots)
        {
            if (slot.currentItem != null)
            {
                GameObject cardInstance = slot.currentItem.gameObject;
                string cardName = cardInstance.name.Replace("(Clone)", "").Trim();
                
                // Find the original prefab
                foreach (var pair in partPairs)
                {
                    if (pair.cardPrefab != null && pair.cardPrefab.name == cardName)
                    {
                        Player.Player.Instance.wormPartsInInventory.Add(pair.cardPrefab);
                        break;
                    }
                }
                
                // Destroy the card instance
                Destroy(cardInstance);
            }
        }
    }

    private void AddPartToWorm(GameObject creaturePart, Transform wormSegment)
    {
        creaturePart.transform.parent = Player.Player.Instance.transform;
        creaturePart.GetComponent<PartDragging>().enabled = false;
        
        if (creaturePart.GetComponent<PartDragging>().partData.name.Equals("leg"))
        {
            Player.Player.Instance.MaxVelocity += GameParameters.legMaxVelocityIncrease;
            legs.Add(creaturePart);
        }
        
        Rigidbody partRigidbody = creaturePart.GetComponent<Rigidbody>();
        
        if (partRigidbody == null)
        {
            partRigidbody = creaturePart.AddComponent<Rigidbody>();
        }
        
        Rigidbody segmentRigidbody = wormSegment.GetComponent<Rigidbody>();
        if (segmentRigidbody != null)
        {
            creaturePart.GetComponent<AttachablePart>().ConfigureRigidBody(partRigidbody, segmentRigidbody, creaturePart.GetComponent<PartDragging>().partData.mass);
        }
        
        Transform endPoint = creaturePart.GetComponent<PartDragging>().endPoint;
        if (endPoint == null)
        {
            Debug.LogError("No endPoint found on part: " + creaturePart.name);
            return;
        }

        // Add hinge joint
        creaturePart.GetComponent<AttachablePart>().ConfigureHingeJoint(segmentRigidbody, endPoint);

        Player.Player.Instance.attachedWormParts.Add(creaturePart);

        IgnorePartCollisionWithWorm(creaturePart, wormSegment);
    }

        private void IgnorePartCollisionWithWorm(GameObject part, Transform nearestWormSegment)
        {
            int numSegments = GameParameters.NumSegmentCollisionsIgnored;

            // Get all colliders on the part and its children
            Collider[] partColliders = part.GetComponentsInChildren<Collider>();

            // Ignore collisions in both directions along the worm
            IgnoreCollisionsInDirection(partColliders, nearestWormSegment, true, numSegments);
            IgnoreCollisionsInDirection(partColliders, nearestWormSegment, false, numSegments);

            //ignore collisions with all other attached parts
            foreach (var attachedWormPart in Player.Player.Instance.attachedWormParts)
            {
                Physics.IgnoreCollision(part.GetComponent<Collider>(), attachedWormPart.GetComponent<Collider>(), true);
            }
        }

        private void IgnoreCollisionsInDirection(Collider[] partColliders, Transform startSegment, bool forward, int numSegments)
        {
            Transform current = startSegment;

            for (int i = 0; i < numSegments && current != null; i++)
            { 
                Collider[] segmentColliders = current.GetComponentsInChildren<Collider>();
                
                foreach (var pCol in partColliders)
                {
                    foreach (var sCol in segmentColliders)
                    {
                        Physics.IgnoreCollision(pCol, sCol, true);
                    }
                }
                
                if (forward)
                {
                    current = current.childCount > 0 ? current.GetChild(0) : null;
                }
                else
                {
                    current = current.parent;
                }
            }
        }
    
        private Transform FindNearestWormSegment(GameObject part) 
        {
            Transform nearestPart = null;
            float shortestDistance = Mathf.Infinity;
    
            foreach (Transform wormPart in player.wormBodySegments)
            {
                float distance = Vector3.Distance(part.transform.position, wormPart.position);
        
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestPart = wormPart;
                }
            }
    
            return nearestPart;
        }
        
        private void SpawnCardInInventory(GameObject cardPrefab)
        {
            CreatureBuilderPartInventory inventory = FindFirstObjectByType<CreatureBuilderPartInventory>();
    
            if (inventory != null)
            {
                bool success = inventory.AddCardToInventory(cardPrefab);
                if (!success)
                {
                    Debug.LogWarning("Failed to add card to inventory");
                }
            }
            else
            {
                Debug.LogWarning("Inventory not found");
            }
        }

        private Vector3 CalculateWorldSpawnPosition()
        {
            Vector3[] corners = new Vector3[4];
            creatureBuilderWindow.GetWorldCorners(corners);
    
            // corners[0] = bottom-left, corners[1] = top-left, corners[2] = top-right, corners[3] = bottom-right
            Vector2 mousePos = Input.mousePosition;
    
            // Calculate normalized position within the window (0-1 range)
            float viewportX = Mathf.InverseLerp(corners[0].x, corners[2].x, mousePos.x);
            float viewportY = Mathf.InverseLerp(corners[0].y, corners[2].y, mousePos.y);
    
            // Clamp to 0-1 range in case mouse is outside bounds
            viewportX = Mathf.Clamp01(viewportX);
            viewportY = Mathf.Clamp01(viewportY);

            // Create a ray from the 3D camera through the viewport point
            Ray ray = targetCamera.ViewportPointToRay(new Vector3(viewportX, viewportY, 0));
            return ray.GetPoint(spawnDistance);
        }
        
        private void SpawnPartInWorld(GameObject prefab, Vector3 position)
        {
            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            instance.name = prefab.name;
            
            PartDragging partDragging = instance.GetComponent<PartDragging>();
            if (partDragging != null)
            {
                ResetPartDragging(partDragging);
                parts.Add(partDragging.gameObject);
            }
        }
        #endregion
    }
}