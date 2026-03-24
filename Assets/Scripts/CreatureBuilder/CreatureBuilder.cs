using System;
using System.Collections;
using System.Collections.Generic;
using CreatureParts;
using Player;
using PurrNet;
using Unity.Cinemachine;
using UnityEditor.SceneManagement;
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
        public CreatureBuilderWindow creatureBuilderScript;
        #endregion
        
        #region private variables
        [Header("Private Variables")]
        
        private Player.Player player;

        private InventoryItem hiddenCard;
        
        [SerializeField] private float spawnDistance = 5f;
        
        [SerializeField] private List<PartPair> partPairs = new List<PartPair>();
        private readonly Dictionary<string, GameObject> prefabMapping = new Dictionary<string, GameObject>();
        #endregion

        #region Built-In Methods
        
        private void Awake()
        {
            if (LocalPlayer.Instance == null)
                LocalPlayer.OnLocalPlayerReady += OnLocalPlayerReady;
            else
            {
                InitializePrefabMapping();
                player = LocalPlayer.Instance;
                cinemachineCamera.Follow = player.transform;
            }
        }

        private void OnLocalPlayerReady()
        {
            InitializePrefabMapping();
            player = LocalPlayer.Instance;
            cinemachineCamera.Follow = player.transform;
        }
        
        void Update()
        {
            if (Input.GetMouseButtonUp(0) && hiddenCard != null)
            {
                hiddenCard.gameObject.SetActive(true);
                hiddenCard = null;
            }
        }

        private void OnDisable()
        {
            LocalPlayer.Instance.GetComponent<PlayerPartAttachment>().AttachCreatureParts(parts, partPairs);
            ReturnAllCardsToPlayerInventory();
        }

        #endregion

        #region public methods
        
        public void OnWormReady()
        {
            StartCoroutine(AddAlreadyAttachedPartsDelayed());
        }
        
        public void SwitchFromCardTo3DPart(GameObject cardPrefab, InventoryItem card)
        {
            if (card.infiniteSlot == true)
            {
                hiddenCard = card;
            }
            else
            {
                card.DestroySelf();
            }
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
        
        #endregion
        
        #region private methods
        
        private IEnumerator AddAlreadyAttachedPartsDelayed()
        {
            Debug.Log("adding already attached parts, LocalPlayer owner: " + LocalPlayer.Instance.owner);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.5f);

            foreach (GameObject part in LocalPlayer.Instance.attachedWormParts)
            {
                Debug.Log("adding already attached part " + part.name);
                AddAlreadyAttachedPart(part);
            }
            
            player.GetComponent<PlayerPartAttachment>().ClearAttachedParts(player);
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
            newPart.GetComponent<NetworkRigidbody>().enabled = false;
            newPart.GetComponent<Rigidbody>().isKinematic = true;
            newPart.GetComponent<Rigidbody>().useGravity = false;
            
            AttachablePart newAttachablePart = newPart.GetComponent<AttachablePart>();
            AttachablePart oldAttachablePart = part.GetComponent<AttachablePart>();
            newAttachablePart.attachedSegmentRigidbody = oldAttachablePart.attachedSegmentRigidbody;
            newAttachablePart.attachmentPosition = oldAttachablePart.attachmentPosition;
            newAttachablePart.attachmentRotation = oldAttachablePart.attachmentRotation;
            
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
            player.GetComponent<PlayerPartAttachment>().DestroyPart(part);
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
        
        public void ReturnAllCardsToPlayerInventory()
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
            if (newPart.GetComponent<MeshCollider>() != null)
            {
                newPart.GetComponent<MeshCollider>().convex = false;
            }
            DontDestroyOnLoad(newPart);
            
            foreach (Rigidbody rb in newPart.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            
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