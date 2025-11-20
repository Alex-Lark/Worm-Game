using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartSelection : MonoBehaviour
{
    public Image card1Slot;
    public Image card2Slot;
    public TextMeshProUGUI card1Name;
    public TextMeshProUGUI card2Name;

    private List<GameObject> partCards;
    private GameObject card1;
    private GameObject card2;
    
    private GameObject currentCard;
    private GameObject discardedCard;
    
    void Start()
    {
        partCards = GameLoop.Instance.partCards;
        PickCardOptions();
    }

    public void PickCardOptions()
    {
        Debug.Log("Picking card options");
        int card1Index = Random.Range(0, partCards.Count);
        int card2Index = Random.Range(0, partCards.Count);
        card1 = partCards[card1Index];
        card2 = partCards[card2Index];
        
        card1Slot.sprite = card1.GetComponent<PartCard>().sprite;
        card2Slot.sprite = card2.GetComponent<PartCard>().sprite;
        card1Name.text = card1.GetComponent<PartCard>().cardName;
        card2Name.text = card2.GetComponent<PartCard>().cardName;
    }

    public void EndCardSelection()
    {
        if (currentCard == null)
        {
            currentCard = card1;
        }
        
        Player.Player.Instance.wormPartsInInventory.Add(currentCard);
        
        //discard discarded card to somoene else
        
        //get discarded card from somoene else and add it to player
        
        //fake discarded card
        int discardCardIndex = Random.Range(0, partCards.Count);
        Player.Player.Instance.wormPartsInInventory.Add(partCards[discardCardIndex]);
        
        //clear values
        card1 = null;
        card2 = null;
        currentCard = null;
        discardedCard = null;
        card1Slot.sprite = null;
        card2Slot.sprite = null;
        card1Name.text = "";
        card2Name.text = "";
        card1Slot.transform.localScale = new Vector3(1f, 1f, 1f);
        card2Slot.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void selectCard1()
    {
        card1Slot.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        card2Slot.transform.localScale = new Vector3(1f, 1f, 1f);
        currentCard = card1;
        discardedCard = card2;
    }

    public void selectCard2()
    {
        card2Slot.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        card1Slot.transform.localScale = new Vector3(1f, 1f, 1f);
        currentCard = card2;
        discardedCard = card1;
    }
}
