using System.Collections.Generic;
using CreatureBuilder;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLoop
{
    public class PartSelection : MonoBehaviour
    {
        #region Public Variables
        [Header("Public Variables")] 
        
        public Image card1Slot;
        public Image card2Slot;
        public TextMeshProUGUI card1Name;
        public TextMeshProUGUI card2Name;
        
        #endregion

        #region Private Variables
        [Header("Private Variables")] 
        
        private List<GameObject> partCards;
        private GameObject card1;
        private GameObject card2;
    
        private GameObject currentCard;
        private GameObject discardedCard;

        public static PartSelection Instance;
        
        #endregion
    
        #region Built-In Methods
        
        void Start()
        {
            Instance = this;
            partCards = GameLoop.Instance.partCards;
        }
        
        #endregion
        
        #region Public Methods

        public void SetCardOptions(PurrNet.PlayerID player, PartSelectionManager.SelectableCardsPacket packet, bool asServer)
        {
            card1 = partCards[packet.Card1Index];
            card2 = partCards[packet.Card2Index];
            
            card1Slot.sprite = card1.GetComponent<PartCard>().sprite;
            card2Slot.sprite = card2.GetComponent<PartCard>().sprite;
            card1Name.text = card1.GetComponent<PartCard>().cardName;
            card2Name.text = card2.GetComponent<PartCard>().cardName;
        }

        public void EndCardSelection()
        {
            //if no card was selected, auto select card 1
            if (currentCard == null)
            {
                currentCard = card1;
            }
        
            Player.LocalPlayer.Instance.wormPartsInInventory.Add(currentCard);
        
            //TODO: discard card and get discarded card from opponent
        
            //fake discarded card
            int discardCardIndex = Random.Range(0, partCards.Count); ////HHHHHHHHHEEEEEEEEEEEEEEEEEELLLLLLLLLLLLLLLLLLLLLLLLPPPPPPPPPPPPPPPPPPPPPPPPPP
            Player.LocalPlayer.Instance.wormPartsInInventory.Add(partCards[discardCardIndex]);
            
            ResetPartSelection();
        }

        public void SelectCard1()
        {
            card1Slot.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            card2Slot.transform.localScale = new Vector3(1f, 1f, 1f);
            currentCard = card1;
            discardedCard = card2;
        }

        public void SelectCard2()
        {
            card2Slot.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            card1Slot.transform.localScale = new Vector3(1f, 1f, 1f);
            currentCard = card2;
            discardedCard = card1;
        }
        
        #endregion
        
        #region Private Methods

        private void ResetPartSelection()
        {
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
        
        #endregion
    }
}
