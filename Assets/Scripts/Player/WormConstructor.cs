using System.Collections.Generic;
using CreatureParts;
using GameLoop.multiplayer;
using PurrNet;
using UnityEngine;

namespace Player
{
    public class WormConstructor: NetworkBehaviour
    {
        private Player player;

        void Awake()
        {
            player = GetComponent<Player>();
            Debug.Log($"player: {player}");
        }
        
        public void CreateWormSegments()
        {
            if (player == null) player = GetComponent<Player>();
            
            CreaturePart previousSegment = player.wormHead.GetComponent<CreaturePart>();
    
            for (int i = 0; i < player.WormSegmentCount; i++)
            {
                GameObject newSegment = Object.Instantiate(player.wormSegmentPrefab, transform);
                newSegment.name = "Worm segment " + i;
                newSegment.GetComponent<CreatureBodySegment>().previousSegment = previousSegment;
                player.wormBodySegments.Add(newSegment.transform);
                previousSegment = newSegment.GetComponent<CreatureBodySegment>();
            }
            
            for (int i = 0; i < player.wormBodySegments.Count - 1; i++)
            {
                player.wormBodySegments[i].GetComponent<CreatureBodySegment>().nextSegment = 
                    player.wormBodySegments[i + 1].GetComponent<CreatureBodySegment>();
            }
        }

        public void ConstructWorm()
        {
            if (player == null) player = GetComponent<Player>();
            
            Vector3 currentPos = player.wormHead.position;
            Vector3 backDir = -player.wormHead.forward;

            //Debug.Log($"Construct worm called on {player.PlayerName}");
            
            for (int i = 0; i < player.wormBodySegments.Count; i++)
            {
                currentPos += backDir * player.MaxPartDistance;
                Transform segment = player.wormBodySegments[i];
                segment.position = currentPos;
                segment.rotation = player.wormHead.rotation;
                //Debug.Log($"positioning segment {segment}");
            }
        }

        [ServerRpc]
        public void AddSegmentJointsAsServer(Player playerToSetJoints)
        {
            AddSegmentJointsServerRpc(playerToSetJoints);
        }

        [ObserversRpc]
        public void AddSegmentJointsServerRpc(Player playerToSetJoints)
        {
            if (player == null) player = GetComponent<Player>();

            if (player != playerToSetJoints) return;
            
            AddSegmentJoints();
        }

        public void AddSegmentJoints()
        {
            Rigidbody previousRb = player.wormHead.GetComponent<Rigidbody>();
            
            for (int i = 0; i < player.wormBodySegments.Count; i++)
            {
                Transform segment = player.wormBodySegments[i];
                
                previousRb = segment.GetComponent<CreatureBodySegment>().AddJoint(segment, previousRb);
            }
        }
    }
    
    
}