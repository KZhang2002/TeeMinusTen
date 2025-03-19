using System;

namespace _Scripts {
    public static class ShellEvents {
        public static event Action OnPlayerScored;
        public static event Action<int> OnHealthChanged;
    }
}