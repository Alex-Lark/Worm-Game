using System.Collections.Generic;
using CreatureBuilder;
using GameLoop;
using PurrNet;
using PurrNet.Packing;
using UnityEngine;

public class PartSelectionManager : PurrMonoBehaviour
{
    private List<SelectableCardsPacket> SentSelectionPackets = new List<SelectableCardsPacket>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void PickCardOptions()
    {
        SelectableCardsPacket packet = new SelectableCardsPacket();
        List<PlayerID> players = new List<PlayerID>(Network.instance.manager.players);
        while (players.Count > 0)
        {
            (packet.Card1Index, packet.Card2Index) = Pick2RandomCards();
            SentSelectionPackets.Add(packet);
            Network.instance.manager.Send(players[0], packet);
            players.RemoveAt(0);
        }
    }
    
    private (int, int) Pick2RandomCards()
    {
        int card1Index = Random.Range(0, GameLoop.GameLoop.Instance.partCards.Count);
        int card2Index = Random.Range(0, GameLoop.GameLoop.Instance.partCards.Count);

        return (card1Index, card2Index);
    }

    public struct SelectableCardsPacket : IPackedAuto
    {
        public int Card1Index;
        public int Card2Index;
    }
    
    public override void Subscribe(NetworkManager manager, bool asServer)
    {
        manager.Subscribe<SelectableCardsPacket>(PartSelection.Instance.SetCardOptions, asServer);
    }
        
    public override void Unsubscribe(NetworkManager manager, bool asServer)
    {
        manager.Unsubscribe<SelectableCardsPacket>(PartSelection.Instance.SetCardOptions, asServer);
            
    }
}
