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
        }
        
        void Update()
        {
            slider.value = player.currentPlayerHealth;
        }
    }
}
