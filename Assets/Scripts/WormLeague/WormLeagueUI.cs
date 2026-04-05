using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WormLeague
{
    public class WormLeagueUI : MonoBehaviour
    {
        public WormLeague wormLeague;
        
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI teamText;
        public TextMeshProUGUI redScore;
        public TextMeshProUGUI blueScore;

        private string team;

        private void Start()
        {
            DisplayTitleScreen();
        }

        public void SetTeam(string inputTeam)
        {
            team = inputTeam;
        }

        public void GoalScored(string goalTeam, string playerName)
        {
            if (goalTeam == "blue")
            {
                titleText.color = Color.blue;
                blueScore.text = "Blue Score: " + wormLeague.teamBlueScore;
            }
            else if (goalTeam == "red")
            {
                titleText.color = Color.red;
                redScore.text = "Red Score: " + wormLeague.teamRedScore;
            }
            
            titleText.text = playerName + " Scored!";
            titleText.alpha = 1f;
            titleText.DOFade(0f, GameParameters.ScoreFadeTime).SetDelay(GameParameters.ScoreShowTime);
        }
        
        private void DisplayTeam()
        {
            if (team == "blue")
            {
                titleText.text = "Team Blue";
                titleText.color = Color.blue;
                teamText.text = "You Are Blue";
                teamText.color = Color.blue;
            }
            else if (team == "red")
            {
                titleText.text = "Team Red";
                titleText.color = Color.red;
                teamText.text = "You Are Red";
                teamText.color = Color.red;
            }
            else
            {
                return;
            }

            titleText.alpha = 1f;
            titleText.DOFade(0f, GameParameters.TeamFadeTime).SetDelay(GameParameters.TeamShowTime);
        }

        private void DisplayTitleScreen()
        {
            titleText.text = "Worm League";
            
            titleText.alpha = 1f;
            
            titleText.DOFade(0f, GameParameters.TitleFadeTime).SetDelay(GameParameters.TitleShowTime).OnComplete(DisplayTeam);
        }
    }
}
