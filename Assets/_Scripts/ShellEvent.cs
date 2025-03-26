using System;
using UnityEngine;

namespace _Scripts {
    public class ShellEvent : MonoBehaviour {
        // public static event Action<int> OnHealthChanged;
        
        public static event Action OnShellLanded;
        public static event Action OnShellFired;
        public static event Action OnShellLoaded;

        public static void ShellLanded() {
            OnShellLanded?.Invoke();
        }

        public static void ShellFired() {
            OnShellFired?.Invoke();
        }

        public static void ShellLoaded() {
            OnShellLoaded?.Invoke();
        }
    }
}