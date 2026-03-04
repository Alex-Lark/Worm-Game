using UnityEngine;

namespace Player
{
    public static class LocalPlayer {
        public static Player Instance { get; private set; }
        public static event System.Action OnLocalPlayerReady;

        public static void Register(Player player)
        {
            if (Instance != null)
            {
                Object.Destroy(player.gameObject);
                return;
            }
            Instance = player;
            OnLocalPlayerReady?.Invoke();
        }

        public static void Unregister(Player player) {
            if (Instance == player)
            {
                Instance = null;
            }
        }
    }
}