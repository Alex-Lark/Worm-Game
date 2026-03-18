using System;
using System.Collections;
using System.Collections.Generic;
using CreatureParts;
using Player;
using PurrNet;
using Unity.Cinemachine;
using UnityEngine;

namespace CreatureBuilder
{
    public class CreatureBuilder : NetworkBehaviour
    {
        #region public variables
        [Header("Public Variables")]
        
        public List<GameObject> parts = new List<GameObject>();
        public List<GameObject> legs = new List<GameObject>();
        
        public Camera targetCamera;
        public CinemachineCamera cinemachineCamera;
        public RectTransform creatureBuilderWindow;
        public CreatureBuilderWindow creatureBuilderScript;
        #endregion
        
        #region private variables
        [Header("Private Variables")]
        
        private Player.Player player;
        
        [SerializeField] private float spawnDistance = 5f;
        
        [SerializeField] private List<PartPair> partPairs = new List<PartPair>();
        private readonly Dictionary<string, GameObject> prefabMapping = new Dictionary<string, GameObject>();
        #endregion

        #region Built-In Methods
        private void Awake()
        {
            if (LocalPlayer.Instance == null)
            {
                LocalPlayer.OnLocalPlayerReady += OnLocalPlayerReady;
            }
            else
            {
                InitializePrefabMapping();
                player = LocalPlayer.Instance;
                cinemachineCamera.Follow = player.transform;
                StartCoroutine(AddAlreadyAttachedPartsDelayed());
            }
        }
        
        private void OnLocalPlayerReady()
        {
            InitializePrefabMapping();
            player = LocalPlayer.Instance;
            cinemachineCamera.Follow = player.transform;
            StartCoroutine(AddAlreadyAttachedPartsDelayed());
        }

        private void OnDisable()
        {
            AttachCreatureParts();
        }

        #endregion

        #region public methods
        
        public void SwitchFromCardTo3DPart(GameObject cardPrefab)
        {
            string cardName = cardPrefab.name.Replace("(Clone)", "").Trim();
            Debug.Log($"Looking up: '{cardName}' | Available keys: {string.Join(", ", prefabMapping.Keys)}");
            
            if (prefabMapping.TryGetValue(cardName, out GameObject prefab3D))
            {
                Vector3 spawnPosition = CalculateWorldSpawnPosition();
                if (creatureBuilderScript.selectedPart)
                {
                    creatureBuilderScript.selectedPart
                        .GetComponent<PartDragging>()
                        .DeselectPart();
                }
                GameObject spawnedPart = SpawnPartInWorld(prefab3D, spawnPosition);
                creatureBuilderScript.selectedPart = spawnedPart;
                creatureBuilderScript.selectedPart.GetComponent<PartDragging>().SelectPart();
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
            if (Network.instance.manager.isServer)
            {
                Debug.Log("Running on server");
            }
            // if (!Network.instance.manager.isServer)
            //     return;
            
            Debug.Log("attaching all creature parts");
            foreach (GameObject part in parts)
            {
                Debug.Log("attaching creature part " + part.name);
                PartDragging partDragging = part.GetComponent<PartDragging>();
        
                if (partDragging != null && partDragging.isClamped)
                {
                    Debug.Log("attaching specific creature part");
                    Transform wormSegment = FindNearestWormSegment(part);
                    AddPartToWorm(part, wormSegment);
                    Destroy(part);
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

            foreach (GameObject part in Player.LocalPlayer.Instance.attachedWormParts)
            {
                AddAlreadyAttachedPart(part);
            }
            Player.LocalPlayer.Instance.attachedWormParts.Clear();
        }

        private void AddAlreadyAttachedPart(GameObject part)
        {
            var netRb = part.GetComponent<NetworkRigidbody>();
            if (netRb != null) netRb.enabled = false;
            
            PartDragging partDraggingComponent = part.GetComponent<PartDragging>();
            GameObject prefab = partDraggingComponent.Prefab;

            if (prefab == null)
            {
                Debug.LogWarning($"Prefab reference is null for {part.name}");
                return;
            }
    
            // Check for LegPart component directly
            LegPart legPart = part.GetComponent<LegPart>();
            if (legPart != null)
            {
                Player.LocalPlayer.Instance.MaxVelocity -= GameParameters.LegMaxVelocityIncrease;
                legPart.enabled = false;
            }
    
            CreateDuplicatePart(part, prefab);
        }

        private void CreateDuplicatePart(GameObject part, GameObject prefab)
        {
            Vector3 worldPosition = part.transform.position;
            Quaternion worldRotation = part.transform.rotation;
            Vector3 worldScale = part.transform.lossyScale;

            GameObject newPart = UnityProxy.InstantiateDirectly(prefab, worldPosition, worldRotation);
            Destroy(newPart.GetComponent<NetworkRigidbody>());
            DontDestroyOnLoad(newPart);
            newPart.name = prefab.name;
            newPart.transform.localScale = worldScale;
                
            PartDragging partDragging = newPart.GetComponent<PartDragging>();
            
            if (partDragging != null)
            {
                partDragging.enabled = true;
                ResetPartDragging(partDragging);
                partDragging.Clamp();
            }
            
            LegPart legPart = newPart.GetComponent<LegPart>();
            if (legPart != null)
            {
                legPart.enabled = false;
            }

            parts.Add(newPart);
            Destroy(part);
        }

        private void ResetPartDragging(PartDragging partDragging)
        {
            partDragging.targetCamera = targetCamera;
            partDragging.creatureBuilderWindow = creatureBuilderWindow;
            partDragging.dragDistance = spawnDistance;
            partDragging.axisVisual.SetActive(false);
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
            Debug.Log("Setting leg order");
            int numLegs = legs.Count;
            float totalTime = GameParameters.LegMoveTime;

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
                    Player.LocalPlayer.Instance.wormPartsInInventory.Add(pair.cardPrefab);
                    Destroy(part);
                    return;
                }
            }
            
            Destroy(part);
        }
        
        private void ReturnAllCardsToPlayerInventory()
        {
            Debug.Log("returning all cards");
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
                    
                    foreach (var pair in partPairs)
                    {
                        if (pair.cardPrefab != null && pair.cardPrefab.name == cardName)
                        {
                            Player.LocalPlayer.Instance.wormPartsInInventory.Add(pair.cardPrefab);
                            break;
                        }
                    }
                    
                    Destroy(cardInstance);
                }
            }
        }

    private void AddPartToWorm(GameObject creaturePart, Transform wormSegment)
    {
        Vector3 position = creaturePart.transform.position;
        Quaternion rotation = creaturePart.transform.rotation;
        GameObject prefab = creaturePart.GetComponent<PartDragging>().Prefab;
        
        Destroy(creaturePart);
        Debug.Log("destroyed creature part non-networked");
        
        GameObject networkedPart = Instantiate(prefab, position, rotation, LocalPlayer.Instance.transform);
        
        networkedPart.GetComponent<AttachablePart>().GiveOwnership(LocalPlayer.Instance.localPlayer.Value);
        Debug.Log("networked part owner: " + networkedPart.GetComponent<NetworkRigidbody>().owner);
        
        // Now parent and configure the networked part
        networkedPart.GetComponent<PartDragging>().DeselectPart();
        networkedPart.GetComponent<PartDragging>().enabled = false;

        var rb = networkedPart.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        LegPart legPart = networkedPart.GetComponent<LegPart>();
        if (legPart != null)
        {
            Player.LocalPlayer.Instance.MaxVelocity += GameParameters.LegMaxVelocityIncrease;
            legPart.enabled = true;
            legs.Add(networkedPart);
        }

        Rigidbody partRigidbody = networkedPart.GetComponent<Rigidbody>();
        if (partRigidbody == null)
            partRigidbody = networkedPart.AddComponent<Rigidbody>();

        Rigidbody segmentRigidbody = wormSegment.GetComponent<Rigidbody>();
        if (segmentRigidbody != null)
            networkedPart.GetComponent<AttachablePart>().ConfigureRigidBody(partRigidbody, segmentRigidbody, networkedPart.GetComponent<PartDragging>().partData.mass);

        Transform endPoint = networkedPart.GetComponent<PartDragging>().endPoint;
        if (endPoint == null)
        {
            Debug.LogError("No endPoint found on part: " + networkedPart.name);
            return;
        }

        networkedPart.GetComponent<AttachablePart>().ConfigureHingeJoint(segmentRigidbody, endPoint);
        Player.LocalPlayer.Instance.attachedWormParts.Add(networkedPart);
        IgnorePartCollisionWithWorm(networkedPart, wormSegment);
    }

        private void IgnorePartCollisionWithWorm(GameObject part, Transform nearestWormSegment)
        {
            Debug.Log("ignoring part collision with worm");
            int numSegments = GameParameters.NumSegmentCollisionsIgnored;
            
            Collider[] partColliders = part.GetComponentsInChildren<Collider>();
            
            IgnoreCollisionsInDirection(partColliders, nearestWormSegment, true, numSegments);
            IgnoreCollisionsInDirection(partColliders, nearestWormSegment, false, numSegments);
            
            foreach (var attachedWormPart in Player.LocalPlayer.Instance.attachedWormParts)
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
        
        private GameObject SpawnPartInWorld(GameObject prefab, Vector3 position)
        {
            Debug.Log("running spawn part in world");
            GameObject newPart = UnityProxy.InstantiateDirectly(prefab, position, Quaternion.identity);
            DontDestroyOnLoad(newPart);
            
            var netRb = newPart.GetComponent<NetworkRigidbody>();
            if (netRb != null) netRb.enabled = false;
            
            newPart.name = prefab.name;
    
            PartDragging partDragging = newPart.GetComponent<PartDragging>();
            if (partDragging != null)
            {
                ResetPartDragging(partDragging);
                parts.Add(partDragging.gameObject);
            }
            
            LegPart legPart = newPart.GetComponent<LegPart>();
            if (legPart != null)
            {
                legPart.enabled = false;
            }
            return newPart;
        }
        
        #endregion
    }
}