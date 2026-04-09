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
                GameObject textObject = Instantiate(textPrefab, leaderboardBackground.transform);

                TextMeshProUGUI tmp = textObject.GetComponentInChildren<TextMeshProUGUI>();

                //replace with old score whenever we get that saving/loading between scenes in idk
                tmp.text = player.name + ": 0";

                ScoreAnimator animator = textObject.AddComponent<ScoreAnimator>();
                animator.duration = 0.5f;
                animator.AnimateScore(tmp, player.name, 0, player.score);
            }
        }
    }
}