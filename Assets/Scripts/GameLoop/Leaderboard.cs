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
            print("populating leaderboard");
            foreach (Player.Player player in GameLoop.Instance.players)
            {
                print("adding a player");
                string text = player.PlayerName + ": " + player.PlayerScore;
            
                GameObject textObject = Instantiate(textPrefab, leaderboardBackground.transform);
            
                textObject.GetComponent<TextMeshProUGUI>().text = text;
            }
        }
    }
}
