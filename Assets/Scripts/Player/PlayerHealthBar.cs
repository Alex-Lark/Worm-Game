using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class PlayerHealthBar : MonoBehaviour
    {
        public Slider slider;
        public Player player;
        
        void Start()
        {
            slider.maxValue = player.maxPlayerHealth;
            slider.value = player.currentPlayerHealth;
            LocalPlayer.OnLocalPlayerReady += OnLocalPlayerReady;
        }
        
        void Update()
        {
            if (player != null)
            {
                slider.value = player.currentPlayerHealth;
            }
        }

        private void OnLocalPlayerReady()
        {
            player = LocalPlayer.Instance;
        }
    }
}
