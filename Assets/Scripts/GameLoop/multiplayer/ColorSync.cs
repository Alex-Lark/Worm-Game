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

        public void SendColorUpdate(Color color)
        {
            ColorUpdateMessage message = new ColorUpdateMessage
            {
                color = color
            };
        
            Network.instance.manager.SendToServer<ColorUpdateMessage>(message);
        }

        private void OnColorUpdate(PlayerID player, ColorUpdateMessage data, bool asServer)
        {
            if (asServer)
            {
                // Server receives from client, embed sender ID and broadcast to all
                data.senderID = player;
                Network.instance.manager.SendToAll<ColorUpdateMessage>(data);
            }
            else
            {
                // Client receives from server, update the player's color
                if (PlayerRegister.Players.ContainsKey(data.senderID))
                {
                    // Get the struct, modify it, and put it back
                    PlayerRegister.PlayerData playerData = PlayerRegister.Players[data.senderID];
                    playerData.color = data.color;
                    PlayerRegister.Players[data.senderID] = playerData;
        
                    // Update color selection UI
                    GameLobby.GameLobby lobby = FindObjectOfType<GameLobby.GameLobby>();
                    if (lobby != null)
                    {
                        lobby.colorSelection.UpdateMultiplayerColors(
                            PlayerRegister.Players.Values.Select(p => p.color).ToList()
                        );
                    }
                }
            }
        }

        public struct ColorUpdateMessage : IPackedAuto
        {
            public Color color;
            public PlayerID senderID;
        }
    }
}