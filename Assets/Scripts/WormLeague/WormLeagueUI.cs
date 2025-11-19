using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WormLeague
{
    public class WormLeagueUI : MonoBehaviour
    {
        public TextMeshProUGUI titleText;

        private void Start()
        {
            DisplayTitleScreen();
        }

        private void DisplayTitleScreen()
        {
            titleText.text = "Worm League";
        }
    }
}
