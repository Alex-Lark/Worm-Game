using System.Collections.Generic;
using System.Linq;
using GameLoop.GameLobby;
using PurrNet;
using PurrNet.Packing;
using UnityEngine;

namespace GameLoop.multiplayer
{
    public class ColorSync : PurrMonoBehaviour
    {
        public override void Subscribe(NetworkManager manager, bool asServer)
            => manager.Subscribe<ColorUpdateMessage>(OnColorUpdate, asServer);

        public override void Unsubscribe(NetworkManager manager, bool asServer)
            => manager.Unsubscribe<ColorUpdateMessage>(OnColorUpdate, asServer);

        // Called locally by ColorSelection — passes the local player explicitly
        public void SendColorUpdate(int colorIndex, Player.Player localPlayer)
        {
            if (localPlayer == null)
            {
                Debug.LogWarning("SendColorUpdate: no local player.");
                return;
            }

            Network.instance.manager.SendToServer(new ColorUpdateMessage
            {
                colorIndex = colorIndex
            });
        }

        private void OnColorUpdate(PlayerID sender, ColorUpdateMessage data, bool asServer)
        {
            if (asServer)
            {
                data.senderID = sender;
                Network.instance.manager.SendToAll(data);
                return;
            }

            // --- Client side: apply color to the correct player, not ourselves ---

            if (!PlayerRegister.Players.ContainsKey(data.senderID))
            {
                Debug.LogWarning($"OnColorUpdate: no registered player for {data.senderID}");
                return;
            }

            // Update PlayerRegister
            PlayerRegister.PlayerData playerData = PlayerRegister.Players[data.senderID];
            playerData.colorIndex = data.colorIndex;
            PlayerRegister.Players[data.senderID] = playerData;

            // Find the Player component that belongs to senderID and apply the color
            Player.Player targetPlayer = FindPlayerByID(data.senderID);
            if (targetPlayer == null)
            {
                Debug.LogWarning($"OnColorUpdate: could not find Player object for {data.senderID}");
                return;
            }

            ColorSelection colorSelection = FindFirstObjectByType<ColorSelection>();
            if (colorSelection == null || data.colorIndex < 0 || data.colorIndex >= colorSelection.availableColors.Count)
                return;

            targetPlayer.SetColor(colorSelection.availableColors[data.colorIndex].bodyMaterial);

            // Rebuild the set of taken indices and push to the UI
            HashSet<int> taken = new HashSet<int>(
                PlayerRegister.Players.Values
                    .Where(p => p.colorIndex >= 0)
                    .Select(p => p.colorIndex)
            );
            colorSelection.RefreshTakenColors(taken);
        }
        
        private Player.Player FindPlayerByID(PlayerID id)
        {
            foreach (Player.Player p in FindObjectsByType<Player.Player>(FindObjectsSortMode.None))
            {
                if (p.owner == id)
                    return p;
            }
            return null;
        }

        public struct ColorUpdateMessage : IPackedAuto
        {
            public int colorIndex;
            public PlayerID senderID;
        }
    }
}