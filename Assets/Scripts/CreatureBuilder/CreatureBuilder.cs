using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace CreatureBuilder
{
    [Serializable]
    public class PartPair
    {
        public GameObject cardPrefab;   // The card prefab GameObject
        public GameObject part3DPrefab; // The corresponding 3D model prefab
    }

    public class CreatureBuilder : MonoBehaviour
    {
        [SerializeField] private List<PartPair> partPairs = new List<PartPair>();
        private List<GameObject> parts = new List<GameObject>();
        
        public Camera targetCamera;
        public CinemachineCamera cinemachineCamera;
        public RectTransform creatureBuilderWindow;
        private Player.Player _player;
        
        [SerializeField] private float spawnDistance = 5f;

        private Dictionary<string, GameObject> _prefabMapping = new Dictionary<string, GameObject>();

        private void Awake()
        {
            InitializePrefabMapping();
            _player = Player.Player.Instance;
            cinemachineCamera.Follow = _player.transform;
        }

        private void Start()
        {
            AddAlreadyAttachedParts();
        }

        private void AddAlreadyAttachedParts() 
        {
            StartCoroutine(AddAlreadyAttachedPartsDelayed());
        }

        private IEnumerator AddAlreadyAttachedPartsDelayed()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.5f);

            foreach (GameObject part in Player.Player.Instance.attachedWormParts)
            {
                PartDragging partDraggingComponent = part.GetComponent<PartDragging>();
                GameObject prefab = partDraggingComponent.prefab;
        
                if (prefab == null)
                {
                    Debug.LogWarning($"Prefab reference is null for {part.name}");
                    continue;
                }
                
                if (part.GetComponent<PartDragging>().partData.name.Equals("leg"))
                {
                    Player.Player.Instance.MaxVelocity -= GameParameters.legMaxVelocityIncrease;
                }

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
                    partDragging.targetCamera = targetCamera;
                    partDragging.creatureBuilderWindow = creatureBuilderWindow;
                    partDragging.dragDistance = spawnDistance;
                    partDragging.Clamp();
                }

                parts.Add(newPart);
                Destroy(part);
            }

            Player.Player.Instance.attachedWormParts.Clear();
        }

        private void InitializePrefabMapping()
        {
            _prefabMapping.Clear();
            foreach (var pair in partPairs)
            {
                if (pair.cardPrefab != null && pair.part3DPrefab != null)
                {
                    // Use the prefab's name as the key
                    string keyName = pair.cardPrefab.name;
                    _prefabMapping[keyName] = pair.part3DPrefab;
                    Debug.Log($"Mapped: {keyName} -> {pair.part3DPrefab.name}");
                }
            }
        }

        public void SwitchTo3DPart(GameObject cardPrefab)
        {
            if (cardPrefab == null)
            {
                Debug.LogWarning("Card prefab is null");
                return;
            }

            // Use the name for lookup
            string keyName = cardPrefab.name.Replace("(Clone)", "").Trim();
            
            if (_prefabMapping.TryGetValue(keyName, out GameObject prefab3D))
            {
                Vector3 spawnPosition = CalculateWorldSpawnPosition();
                SpawnPartInWorld(prefab3D, spawnPosition);
            }
            else
            {
                Debug.LogWarning($"No 3D prefab mapping found for card: {keyName}");
            }
        }

        public void SwitchTo2DCard(GameObject partPrefab)
        {
            if (partPrefab == null)
            {
                Debug.LogWarning("Part prefab is null");
                return;
            }

            // Use the name for lookup
            string keyName = partPrefab.name.Replace("(Clone)", "").Trim();
    
            // Find the matching pair by searching for the 3D prefab
            foreach (var pair in partPairs)
            {
                if (pair.part3DPrefab != null && pair.part3DPrefab.name == keyName)
                {
                    // Found the matching card prefab
                    SpawnCardInInventory(pair.cardPrefab);
            
                    // Destroy the 3D part
                    parts.Remove(partPrefab);
                    Destroy(partPrefab);
                    return;
                }
            }
    
            Debug.LogWarning($"No 2D card mapping found for part: {keyName}");
        }

        public void AttachCreatureParts()
{
    print("attach creature part called");
    
    // Process 3D parts that are spawned in the world
    foreach (GameObject part in parts)
    {
        PartDragging partDragging = part.GetComponent<PartDragging>();
        
        if (partDragging != null && partDragging.isClamped)
        {
            print("found clamped part");
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
    
    // Return all remaining cards from the creature builder inventory to player
    ReturnAllCardsToPlayerInventory();
}

private void ReturnPartToPlayerInventory(GameObject part)
{
    // Find the matching card prefab
    string keyName = part.name.Replace("(Clone)", "").Trim();
    
    foreach (var pair in partPairs)
    {
        if (pair.part3DPrefab != null && pair.part3DPrefab.name == keyName)
        {
            Player.Player.Instance.wormPartsInInventory.Add(pair.cardPrefab);
            Destroy(part);
            return;
        }
    }
    
    Debug.LogWarning($"No card mapping found for part: {keyName}");
    Destroy(part);
}

private void ReturnAllCardsToPlayerInventory()
{
    CreatureBuilderPartInventory inventory = FindObjectOfType<CreatureBuilderPartInventory>();
    
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
    print("adding part to worm");
    creaturePart.transform.parent = Player.Player.Instance.transform;
    creaturePart.GetComponent<PartDragging>().enabled = false;
    
    if (creaturePart.GetComponent<PartDragging>().partData.name.Equals("leg"))
    {
        Player.Player.Instance.MaxVelocity += GameParameters.legMaxVelocityIncrease;
    }
    
    // Configure or get rigidbody
    Rigidbody partRigidbody = creaturePart.GetComponent<Rigidbody>();
    if (partRigidbody == null)
    {
        partRigidbody = creaturePart.AddComponent<Rigidbody>();
    }
    
    // Match the worm segment's rigidbody settings
    Rigidbody segmentRigidbody = wormSegment.GetComponent<Rigidbody>();
    if (segmentRigidbody != null)
    {
        partRigidbody.mass = segmentRigidbody.mass;
        partRigidbody.linearDamping = segmentRigidbody.linearDamping;
        partRigidbody.angularDamping = segmentRigidbody.angularDamping;
        partRigidbody.interpolation = segmentRigidbody.interpolation;
        partRigidbody.collisionDetectionMode = segmentRigidbody.collisionDetectionMode;
    }
    
    // Setup the fixed joint
    FixedJoint fixedJoint = creaturePart.AddComponent<FixedJoint>();
    fixedJoint.connectedBody = segmentRigidbody;
    fixedJoint.breakForce = Mathf.Infinity;
    fixedJoint.breakTorque = Mathf.Infinity;
    fixedJoint.enableCollision = false;
    fixedJoint.enablePreprocessing = true;
    
    Player.Player.Instance.attachedWormParts.Add(creaturePart);
    
    
    IgnorePartCollisionWithWorm(creaturePart, wormSegment);
}

private void IgnorePartCollisionWithWorm(GameObject part, Transform nearestWormSegment)
{
    if (part == null || nearestWormSegment == null)
        return;

    int numSegments = GameParameters.NumSegmentCollisionsIgnored;

    // Get all colliders on the part and its children
    Collider[] partColliders = part.GetComponentsInChildren<Collider>();

    // Ignore collisions in both directions along the worm
    IgnoreCollisionsInDirection(partColliders, nearestWormSegment, true, numSegments);
    IgnoreCollisionsInDirection(partColliders, nearestWormSegment, false, numSegments);
}

private void IgnoreCollisionsInDirection(Collider[] partColliders, Transform startSegment, bool forward, int numSegments)
{
    Transform current = startSegment;

    for (int i = 0; i < numSegments && current != null; i++)
    {
        // Get all colliders on this worm segment and its children
        Collider[] segmentColliders = current.GetComponentsInChildren<Collider>();

        // Ignore collisions between every part collider and every segment collider
        foreach (var pCol in partColliders)
        {
            foreach (var sCol in segmentColliders)
            {
                Physics.IgnoreCollision(pCol, sCol, true);
            }
        }

        // Move to next or previous segment safely
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

private void IgnoreCollisionsInDirection(Collider partCollider, Transform startSegment, bool useNext, int count) {
    WormBodySegment current = startSegment.GetComponent<WormBodySegment>();
    
    for (int i = 0; i < count && current != null; i++)
    {
        Collider segmentCollider = current.GetComponent<Collider>();
        if (segmentCollider != null)
        {
            Physics.IgnoreCollision(partCollider, segmentCollider, true);
        }
        
        current = useNext ? (current.nextSegment as WormBodySegment) : (current.previousSegment as WormBodySegment);
    }
}

        private Transform FindNearestWormSegment(GameObject part) 
        {
            Transform nearestPart = null;
            float shortestDistance = Mathf.Infinity;
    
            foreach (Transform wormPart in _player.wormBodySegments)
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
            // Find the inventory to spawn the card back into
            CreatureBuilderPartInventory inventory = FindObjectOfType<CreatureBuilderPartInventory>();
    
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
            // Get the screen-space corners of the CreatureBuilderWindow
            Vector3[] corners = new Vector3[4];
            creatureBuilderWindow.GetWorldCorners(corners);
    
            // corners[0] = bottom-left, corners[1] = top-left, corners[2] = top-right, corners[3] = bottom-right
            Vector2 mousePos = Input.mousePosition;
    
            Debug.Log($"Mouse: {mousePos}, BottomLeft: {corners[0]}, TopRight: {corners[2]}");
    
            // Calculate normalized position within the window (0-1 range)
            float viewportX = Mathf.InverseLerp(corners[0].x, corners[2].x, mousePos.x);
            float viewportY = Mathf.InverseLerp(corners[0].y, corners[2].y, mousePos.y);
    
            // Clamp to 0-1 range in case mouse is outside bounds
            viewportX = Mathf.Clamp01(viewportX);
            viewportY = Mathf.Clamp01(viewportY);
    
            Debug.Log($"ViewportX: {viewportX}, ViewportY: {viewportY}");

            // Create a ray from the 3D camera through the viewport point
            Ray ray = targetCamera.ViewportPointToRay(new Vector3(viewportX, viewportY, 0));
            return ray.GetPoint(spawnDistance);
        }
        private void SpawnPartInWorld(GameObject prefab, Vector3 position)
        {
            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            instance.name = prefab.name;
    
            // Set up the creature part
            PartDragging partDragging = instance.GetComponent<PartDragging>();
            if (partDragging != null)
            {
                partDragging.targetCamera = targetCamera;
                partDragging.creatureBuilderWindow = creatureBuilderWindow;
                partDragging.dragDistance = spawnDistance;
                parts.Add(partDragging.gameObject);
            }
        }
    }
}