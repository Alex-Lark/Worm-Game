using TMPro;
using UnityEngine;

namespace GameLoop
{
    public class GameEndScene : MonoBehaviour
    {
        public GameObject gameEndText;
    
        void Start()
        {
            ShowWinner();
        }

        private void ShowWinner()
        {
            PlayerRegister.PlayerData topPlayer = new PlayerRegister.PlayerData();
            int highestScore = int.MinValue;

            foreach (PlayerRegister.PlayerData player in PlayerRegister.Players.Values)
            {
                if (player.score > highestScore)
                {
                    highestScore = player.score;
                    topPlayer = player;
                }
            }

            gameEndText.GetComponent<TextMeshProUGUI>().text = "Winner: " + topPlayer.name;
        }
    }
}
