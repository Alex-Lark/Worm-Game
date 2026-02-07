using TMPro;
using UnityEngine;

namespace GameLoop
{
    public class Leaderboard : MonoBehaviour
    {
        public GameObject leaderboardBackground;
        public GameObject textPrefab;
    
        void Start()
        {
            PopulateLeaderboard();
        }

        private void PopulateLeaderboard()
        {
            foreach (Player.Player player in GameLoop.Instance.players)
            {
                string text = player.PlayerName + ": " + player.playerScore;
            
                GameObject textObject = Instantiate(textPrefab, leaderboardBackground.transform);
            
                textObject.GetComponent<TextMeshProUGUI>().text = text;
            }
        }
    }
}
