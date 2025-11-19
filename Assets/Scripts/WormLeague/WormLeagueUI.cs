using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WormLeague
{
    public class WormLeagueUI : MonoBehaviour
    {
        public TextMeshProUGUI titleText;

        private string team;

        private void Start()
        {
            DisplayTitleScreen();
        }

        public void SetTeam(string inputTeam)
        {
            team = inputTeam;
        }
        
        private void DisplayTeam()
        {
            if (team == "blue")
            {
                titleText.text = "Team Blue";
                titleText.color = Color.blue;
            }
            else if (team == "red")
            {
                print("displaying team red");
                titleText.text = "Team Red";
                titleText.color = Color.red;
            }
            else
            {
                return;
            }

            titleText.alpha = 1f;
            titleText.DOFade(0f, GameParameters.teamFadeTime).SetDelay(GameParameters.teamShowTime);
        }

        private void DisplayTitleScreen()
        {
            titleText.text = "Worm League";
            
            titleText.alpha = 1f;
            
            titleText.DOFade(0f, GameParameters.titleFadeTime).SetDelay(GameParameters.titleShowTime).OnComplete(DisplayTeam);
        }
    }
}
