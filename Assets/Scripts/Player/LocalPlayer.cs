namespace Player
{
    public static class LocalPlayer {
        public static Player Instance { get; private set; }
        public static event System.Action OnLocalPlayerReady;

        public static void Register(Player player)
        {
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