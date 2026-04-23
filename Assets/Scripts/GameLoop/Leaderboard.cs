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
            foreach (PlayerRegister.PlayerData player in PlayerRegister.Players.Values)
            {
                string text = player.name + ": " + player.score;
            
                GameObject textObject = Instantiate(textPrefab, leaderboardBackground.transform);
            
                textObject.GetComponentInChildren<TextMeshProUGUI>().text = text;
            }
        }
    }
}
