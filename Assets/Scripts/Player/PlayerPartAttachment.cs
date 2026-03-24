using System.Collections;
using System.Collections.Generic;
using CreatureBuilder;
using CreatureParts;
using PurrNet;
using UnityEngine;

namespace Player
{
    public class PlayerPartAttachment : NetworkBehaviour
    {
        public void AttachCreatureParts(List<GameObject> parts, List<PartPair> partPairs)
        {
            foreach (GameObject part in parts)
            {
                Debug.Log("attaching creature part " + part.name);
                PartDragging partDragging = part.GetComponent<PartDragging>();
        
                if (partDragging != null && partDragging.isClamped)
                {
                    AddPartToWorm(part);
                    Destroy(part);
                }
                else
                {
                    print("part not clamped, returning to player inventory");
                    ReturnPartToPlayerInventory(part, partPairs);
                }
            }
    
            parts.Clear();
            SyncLegOrderRpc(gameObject);
        }
        
        [ServerRpc]
        public void AddAttachedPartServerSide(GameObject prefab, Vector3 position, Quaternion rotation, GameObject attachedSegment, float partMass, GameObject player)
        {
            GameObject networkedPart = Instantiate(prefab, position, rotation, player.transform);
            networkedPart.GetComponent<AttachablePart>().attachedSegmentRigidbody = attachedSegment.GetComponent<Rigidbody>();
            networkedPart.GetComponent<AttachablePart>().attachmentPosition = position;
            networkedPart.GetComponent<AttachablePart>().attachmentRotation = rotation;
            networkedPart.GetComponent<AttachablePart>().GiveOwnership(player.GetComponent<NetworkTransform>().owner);
            
            AddAttachedPartForClients(networkedPart, player, partMass, attachedSegment.gameObject);
            SyncLegOrderRpc(player);
        }

        [ObserversRpc(runLocally: true)]
        public void AddAttachedPartForClients(GameObject part, GameObject partPlayer, float partMass, GameObject attachedSegment)
        {
            part.GetComponent<AttachablePart>().attachedSegmentRigidbody = attachedSegment.GetComponent<Rigidbody>();
            part.GetComponent<AttachablePart>().attachmentPosition = part.transform.position;
            part.GetComponent<AttachablePart>().attachmentRotation = part.transform.rotation;
            
            Debug.Log($"AddAttachedPartForClients: part instanceID={part.GetInstanceID()} name={part.name}");
            Player targetPlayer = partPlayer.GetComponent<Player>();
            targetPlayer.attachedWormParts.Add(part);

            part.GetComponent<PartDragging>().DeselectPart();
            part.GetComponent<PartDragging>().enabled = false;

            var rb = part.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            LegPart legPart = part.GetComponent<LegPart>();
            if (legPart != null)
            {
                targetPlayer.MaxVelocity += GameParameters.LegMaxVelocityIncrease;
                legPart.enabled = true;
            }

            Rigidbody partRigidbody = part.GetComponent<Rigidbody>();
            if (partRigidbody == null)
                partRigidbody = part.AddComponent<Rigidbody>();
            
            part.GetComponent<AttachablePart>().ConfigureRigidBody(partRigidbody, partMass);

            Transform endPoint = part.GetComponent<PartDragging>().endPoint;
            if (endPoint == null)
            {
                return;
            }

            part.GetComponent<AttachablePart>().ConfigureHingeJoint(endPoint);
            partPlayer.GetComponent<WormPhysics>().IgnorePartCollisionWithWorm(part,  part.GetComponent<AttachablePart>().attachedSegmentRigidbody.transform);
        }
        
        [ObserversRpc]
        public void SyncLegOrderRpc(GameObject specificPlayer)
        {
            List<LegPart> legParts = new List<LegPart>();
            foreach (GameObject part in specificPlayer.GetComponent<Player>().attachedWormParts)
            {
                LegPart leg = part.GetComponent<LegPart>();
                if (leg != null)
                    legParts.Add(leg);
            }

            float totalTime = GameParameters.LegMoveTime;
            for (int i = 0; i < legParts.Count; i++)
            {
                legParts[i].timeOffset = i * (totalTime / legParts.Count);
            }
        }

        [ServerRpc]
        public void DestroyPart(GameObject part)
        {
            Destroy(part);
        }
        
        public IEnumerator ReactivateAttachedParts()
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
    
            foreach (GameObject attachedPart in GetComponent<Player>().attachedWormParts)
            {
                Debug.Log($"ReactivateAttachedParts: part instanceID={attachedPart.GetInstanceID()} name={attachedPart.name}");
                attachedPart.SetActive(true);
                attachedPart.GetComponent<AttachablePart>().enabled = true;
                attachedPart.GetComponent<AttachablePart>().ResetJoint();
            }
    
            GetComponent<WormPhysics>().IgnoreWormSelfCollision();
        }
        
        #region Private Methods
        
        private void AddPartToWorm(GameObject creaturePart)
        {
            AttachablePart attachablePart = creaturePart.GetComponent<AttachablePart>();

            Rigidbody attachedSegment = attachablePart.attachedSegmentRigidbody;
            Vector3 position = attachablePart.attachmentPosition;
            Quaternion rotation = attachablePart.attachmentRotation;
            GameObject prefab = creaturePart.GetComponent<PartDragging>().Prefab;
            float partMass = creaturePart.GetComponent<PartDragging>().partData.mass;
    
            AddAttachedPartServerSide(
                prefab,
                position,
                rotation,
                attachedSegment.gameObject,
                partMass,
                gameObject
            );
        }
        
        private void ReturnPartToPlayerInventory(GameObject part, List<PartPair> partPairs)
        {
            string partName = part.name.Replace("(Clone)", "").Trim();
            
            foreach (var pair in partPairs)
            {
                if (pair.part3DPrefab != null && pair.part3DPrefab.name == partName)
                {
                    GetComponent<Player>().wormPartsInInventory.Add(pair.cardPrefab);
                    Destroy(part);
                    return;
                }
            }
            
            Destroy(part);
        }
        
        #endregion
    }
}
