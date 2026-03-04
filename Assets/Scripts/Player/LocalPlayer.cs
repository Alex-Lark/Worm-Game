using UnityEngine;

namespace Player
{
    public static class LocalPlayer {
        public static Player Instance { get; private set; }
        public static event System.Action OnLocalPlayerReady;

        public static void Register(Player player)
        {
            Debug.Log("local player register called");
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