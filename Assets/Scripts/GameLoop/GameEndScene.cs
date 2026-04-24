using TMPro;
using UnityEngine;
using System.Linq;

namespace GameLoop
{
    public class GameEndScene : MonoBehaviour
    {
        public GameObject gameFirstText;
        public GameObject gameSecondText;
        public GameObject gameThirdText;
        
        public GameObject secondPlaceWorm;
        public GameObject thirdPlaceWorm;
    
        void Start()
        {
            ShowWinner();
        }

        private void ShowWinner()
        {
            var players = PlayerRegister.Players.Values.OrderByDescending(p => p.score).ToList();
            
            if (players.Count >= 1)
            {
                gameFirstText.GetComponent<TextMeshProUGUI>().text = players[0].name;
            }

            if (players.Count >= 2)
            {
                gameSecondText.GetComponent<TextMeshProUGUI>().text = players[1].name;

                secondPlaceWorm.SetActive(true);
            }
            else
            {
                gameSecondText.GetComponent<TextMeshProUGUI>().text = "";
                secondPlaceWorm.SetActive(false);
            }
            
            if (players.Count >= 3)
            {
                gameThirdText.GetComponent<TextMeshProUGUI>().text = players[2].name;

                thirdPlaceWorm.SetActive(true);
            }
            else
            {
                gameThirdText.GetComponent<TextMeshProUGUI>().text = "";
                thirdPlaceWorm.SetActive(false);
            }
        }
    }
}
