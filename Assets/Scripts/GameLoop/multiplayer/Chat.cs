using PurrNet;
using PurrNet.Packing;
using PurrNet.Transports;
using TMPro;
using UnityEngine;

public class Chat : PurrMonoBehaviour
{
    public GameObject chatTextPrefab;
    public Transform chatImage;
    public TMP_InputField chatInputField;
    public int maxMessages = 10;

    public void SendChatMessage(string message)
    {
        if (message == "")return;
        ChatMessage chatMessage = new ChatMessage();
        chatMessage.message = message;
        Network.instance.manager.SendToServer<ChatMessage>(chatMessage);
    }
    
    public void PostChatMessage(string message)
    {
        if (message == "")
        {
            return;
        }

        string finalMessage = "<" + Player.Player.Instance.PlayerName + "> " + message;

        GameObject messageObject = Instantiate(chatTextPrefab, chatImage);
        TextMeshProUGUI messageText = messageObject.GetComponent<TextMeshProUGUI>();
        messageText.text = finalMessage;

        if (chatImage.childCount > maxMessages)
        {
            Destroy(chatImage.GetChild(0).gameObject);
        }

        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }
    
        public override void Subscribe(NetworkManager manager, bool asServer)
        {
            manager.Subscribe<ChatMessage>(OnChatMessage, asServer);
        }
        
        public override void Unsubscribe(NetworkManager manager, bool asServer)
        {
            manager.Unsubscribe<ChatMessage>(OnChatMessage, asServer);
            
        }

        // Called when a ChatMessage broadcast is sent from either the Server or a Client
        private void OnChatMessage(PlayerID player, ChatMessage data, bool asServer)
        {
            if (asServer)   // The broadcast was sent to the Server from a Client
            {
                // Send the broadcast down to the Clients
                Network.instance.manager.SendToAll<ChatMessage>(data);
            }
            else    // The broadcast was sent to the Clients from the Server
            {
                PostChatMessage(data.message);
            }
        }
    

    public void DeactivateInputField()
    {
        chatInputField.DeactivateInputField();
    }
    
    public struct ChatMessage : IPackedAuto
    {
        public string message;
    }
}
