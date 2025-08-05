using System;
using UnityEngine;

namespace _Scripts {
    public class ShellEvent : MonoBehaviour {
        public static event Action<Shell> OnShellRegistered;
        
        public static void ShellRegistered(Shell shell) {
            Debug.Log("SHELLEVENT.ShellRegistered triggered.");
            OnShellRegistered?.Invoke(shell);
        }
        
        // regular gameplay
        public static event Action<int> OnShellLanded;
        public static event Action OnShellFired;
        public static event Action OnShellLoaded;

        public static void ShellLanded(int zoneID) {
            Debug.Log("SHELLEVENT.ShellLanded triggered at zone " + zoneID);
            // if (zoneID < 0) {
            //     Debug.LogWarning("zone id is less than 0, exiting early.");
            //     return;
            // }
            
            OnShellLanded?.Invoke(zoneID);
        }

        public static void ShellFired() {
            Debug.Log("SHELLEVENT.ShellFired triggered.");
            OnShellFired?.Invoke();
        }

        public static void ShellLoaded() {
            Debug.Log("SHELLEVENT.ShellLoaded triggered.");
            OnShellLoaded?.Invoke();
        }
    }
}