using System.Linq;
using PurrNet;
using PurrNet.Packing;
using UnityEngine;

namespace GameLoop.multiplayer
{
    public class ColorSync : PurrMonoBehaviour
    {
        public override void Subscribe(NetworkManager manager, bool asServer)
        {
            manager.Subscribe<ColorUpdateMessage>(OnColorUpdate, asServer);
        }

        public override void Unsubscribe(NetworkManager manager, bool asServer)
        {
            manager.Unsubscribe<ColorUpdateMessage>(OnColorUpdate, asServer);
        }

        public void SendColorUpdate(Material bodyMaterial)
        {
            GameLobby.ColorSelection colorSelection = FindFirstObjectByType<GameLobby.ColorSelection>();
            int materialIndex = colorSelection.availableColors
                .FindIndex(pair => pair.bodyMaterial == bodyMaterial);

            if (materialIndex < 0)
            {
                Debug.LogWarning("SendColorUpdate: material not found in availableColors.");
                return;
            }

            Network.instance.manager.SendToServer(new ColorUpdateMessage
            {
                colorIndex = materialIndex
            });
        }

        private void OnColorUpdate(PlayerID player, ColorUpdateMessage data, bool asServer)
        {
            if (asServer)
            {
                data.senderID = player;
                Network.instance.manager.SendToAll(data);
            }
            else
            {
                if (!PlayerRegister.Players.ContainsKey(data.senderID))
                    return;

                PlayerRegister.PlayerData playerData = PlayerRegister.Players[data.senderID];
                playerData.colorIndex = data.colorIndex;
                PlayerRegister.Players[data.senderID] = playerData;

                GameLobby.GameLobby lobby = FindObjectOfType<GameLobby.GameLobby>();
                if (lobby == null) return;

                // Resolve each player's index to the actual body material
                GameLobby.ColorSelection colorSelection = lobby.colorSelection;
                lobby.colorSelection.UpdateMultiplayerColors(
                    PlayerRegister.Players.Values
                        .Where(p => p.colorIndex >= 0 && p.colorIndex < colorSelection.availableColors.Count)
                        .Select(p => colorSelection.availableColors[p.colorIndex].bodyMaterial)
                        .ToList()
                );
            }
        }

        public struct ColorUpdateMessage : IPackedAuto
        {
            public int colorIndex;
            public PlayerID senderID;
        }
    }
}