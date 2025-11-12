using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartSelection : MonoBehaviour
{
    public List<GameObject> partCards = new List<GameObject>();
    public Image card1Slot;
    public Image card2Slot;
    public TextMeshProUGUI card1Name;
    public TextMeshProUGUI card2Name;
    
    void Start()
    {
        PickCardOptions();
    }

    public void PickCardOptions()
    {
        int card1Index = Random.Range(0, partCards.Count);
        int card2Index = Random.Range(0, partCards.Count);
        card1Slot.sprite = partCards[card1Index].GetComponent<PartCard>().sprite;
        card2Slot.sprite = partCards[card2Index].GetComponent<PartCard>().sprite;
        card1Name.text = partCards[card1Index].GetComponent<PartCard>().cardName;
        card2Name.text = partCards[card2Index].GetComponent<PartCard>().cardName;
    }
}
